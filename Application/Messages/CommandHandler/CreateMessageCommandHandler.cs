using Application.Abstractions;
using Application.Abstractions.Interfaces.CommandHandler;
using Application.Messages.Commands;
using Application.Services;
using Application.Utilities.Enums;
using Application.Utilities.Interfaces;
using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Azure.Storage.Blobs;
using CDABrisasAPI.Dto;
using Domain.Dto;
using Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Application.Messages.CommandHandler
{
    public class CreateMessageCommandHandler : ICommandHandler<CreateMessageCommand, Message>
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _accessToken;
        private readonly IBlobService _blobService;
        private readonly string _AzureIAEnpoint;
        private readonly string _AzureApiKey;
        private readonly ImageAnalysisClient _client;
        private readonly string _connectionString;
        private readonly WhatsAppMessages _whatsAppMessages;
        private readonly AzureService _azureService;
        public CreateMessageCommandHandler(
                                            IMessageRepository messageRepository,
                                            IHttpClientFactory httpClientFactory,
                                            IOptions<MetaSettings> metaOptions,
                                            IBlobService blobService,
                                            IConfiguration configuration,
                                            WhatsAppMessages whatsAppMessages,
                                            AzureService azureService)
        {
            _messageRepository = messageRepository;
            _httpClientFactory = httpClientFactory;
            _accessToken = metaOptions.Value.AccessToken;
            _blobService = blobService;
            _AzureIAEnpoint = configuration["AzureIA:Endpoint"];
            _AzureApiKey = configuration["AzureIA:ApiKey"];
            _connectionString = configuration["Azure:BlobStorage"];
            _whatsAppMessages = whatsAppMessages;
            _client = new ImageAnalysisClient(
                new Uri(_AzureIAEnpoint),
                new AzureKeyCredential(_AzureApiKey)
            );
            _azureService = azureService;
        }
        public async Task<Message> HandleAsync(CreateMessageCommand command, CancellationToken cancellationToken = default)
        {
            if (command.typeMessage == 1)
            {
                var result = ValidateImage("https://referidosstorage.blob.core.windows.net/referidos/20251226_214608_573012282168");
                var message = new Message
                {
                    UserId = command.userId,
                    Number = command.number,
                    Text = command.text,
                    DateMessage = command.dateMessage,
                    PaymentStatusId = 1
                };
                return await _messageRepository.CreateMessage(message);
            }
            else if (command.typeMessage == 2)
            {
                string imageUrl = string.Empty;
                string fileName = string.Empty;
                string plateImg = string.Empty;
                (imageUrl, fileName, plateImg) = await _azureService.SaveImageAsync(command.mimeType, command.mediaId, command.number);

                var evaluateVehicleDistance = ValidateImage(imageUrl);
                await _whatsAppMessages.SendWhatsAppTextAsync(command.number, evaluateVehicleDistance.Result);
                var message = new Message
                {
                    UserId = command.userId,
                    Number = command.number,
                    Text = command.text,
                    PlateVehicle = plateImg,
                    DateMessage = command.dateMessage,
                    ImageUrl = imageUrl,
                    MimeType = command.mimeType,
                    PaymentStatusId = (int)PaymentStatusId.Pending,
                    ImageName = fileName
                };
                if (evaluateVehicleDistance.Result == VehicleDistanceResult.Far)
                {
                    //(imageUrl, fileName, plateImg) = await SaveImageAsync(command.mimeType, command.mediaId, command.number);
                    var messageSaved = await _messageRepository.CreateMessage(message);
                    return messageSaved;
                }
                return message;
            }
            else
                throw new InvalidOperationException("Se esta intentando guardar un tipo de mensaje incorrecto.");
        }

        private async Task<(string filePath, string fileName, string plateImg)> SaveImageAsync(string mimeType, string mediaId, string number)
        {
            try
            {
                string filePath = string.Empty;
                string fileName = string.Empty;
                string plateImg = string.Empty;

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

                // 1️⃣ Obtener URL de descarga firmada desde Graph API
                var graphUrl = $"https://graph.facebook.com/v22.0/{mediaId}?fields=url";
                var response = await client.GetAsync(graphUrl);
                var json = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"📡 Respuesta Meta (mediaId={mediaId}): {json}");

                response.EnsureSuccessStatusCode();
                using var jsonDoc = JsonDocument.Parse(json);

                if (!jsonDoc.RootElement.TryGetProperty("url", out var urlProp))
                {
                    Console.WriteLine($"❌ No se encontró la propiedad 'url' en la respuesta: {json}");
                    throw new InvalidOperationException("No se pudo obtener la URL de la imagen desde Meta.");
                }

                var downloadUrl = urlProp.GetString();
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    throw new InvalidOperationException("La URL de descarga de la imagen está vacía.");
                }
                var imageBytes = await client.GetByteArrayAsync(downloadUrl);

                //Save image in azure portal
                fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{number}";
                filePath = await _blobService.UploadToAzureAsync("referidos", imageBytes, fileName);
                Console.WriteLine($"✅ Imagen guardada en Azure: {filePath}");

                var sas = _blobService.GetImageWithSasFromUrl(filePath);
                plateImg = await DetectPlateAsync(sas.Result);

                return (filePath, fileName, plateImg);
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"🌐 Error HTTP al descargar imagen: {httpEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al guardar imagen: {ex}");
                throw;
            }
        }



        public async Task<string?> DetectPlateAsync(string imageUrl)
        {
            var client = new ImageAnalysisClient(
                new Uri(_AzureIAEnpoint),
                new AzureKeyCredential(_AzureApiKey)
            );

            Response<ImageAnalysisResult> response = await client.AnalyzeAsync(
                new Uri(imageUrl),
                VisualFeatures.Read
            );

            ImageAnalysisResult result = response.Value;
            if (result.Read == null)
                return null;

            foreach (DetectedTextBlock block in result.Read.Blocks)
            {
                foreach (DetectedTextLine line in block.Lines)
                {
                    var text = Normalize(line.Text);

                    if (Regex.IsMatch(text, @"^[A-Z]{3}\d{3}$"))
                        return text;
                }
            }
            return null;
        }


        private async Task<VehicleDistanceResult> ValidateImage(string imageUrl)
        {
            var blobClient = TakeImageFromBlobStorage(imageUrl);
            var analysis = await AnalyzeImageFromBlobAsync(blobClient?.Result);
            Console.WriteLine(JsonSerializer.Serialize(analysis, new JsonSerializerOptions { WriteIndented = true }));
            return EvaluateVehicleDistance(analysis);
            //if (distanceResult == VehicleDistanceResult.Close)
            //{
            //    //await _whatsAppMessages.SendWhatsAppTextAsync(
            //    //         toPhoneNumber: whatsappNumber,
            //    //         messageText: "Hola 👋 Por favor toma la foto del vehículo un poco más lejos para que se vea el entorno 🚗📸"
            //    //     );
            //    return false;
            //}
            //else if (distanceResult == VehicleDistanceResult.Far)
            //{
            //    //await _whatsAppMessages.SendWhatsAppTextAsync(
            //    //        toPhoneNumber: whatsappNumber,
            //    //        messageText: "✅ ¡Listo!\r\nLa foto es válida y ya estamos procesando los datos del vehículo.\r\nGracias por enviarla."
            //    //    );
            //    return true;
            //}
            //else
            //{
            //    //await _whatsAppMessages.SendWhatsAppTextAsync(
            //    //       toPhoneNumber: whatsappNumber,
            //    //       messageText: "❌ La imagen no es válida\r\n \r\nNo logramos identificar correctamente el vehículo ni su entorno.\r\n \r\nPor favor envía una nueva foto donde se vea el vehículo completo y el lugar donde se encuentra 🚗📸"
            //    //   );
            //    return false;
            //}

        }

        public async Task<ImageAnalysisResultDto> AnalyzeImageFromBlobAsync(BlobClient blobClient)
        {
            try
            {
                // ⬇️ DESCARGA CORRECTA (incluye headers)
                var download = await blobClient.DownloadStreamingAsync();

                using var stream = download.Value.Content;

                // 🔴 MUY IMPORTANTE
                if (stream.CanSeek)
                    stream.Position = 0;

                var result = await _client.AnalyzeAsync(
                    BinaryData.FromStream(stream),
                    VisualFeatures.Tags |
                    VisualFeatures.Objects |
                    VisualFeatures.Caption
                );

                var analysis = result.Value;

                var dto = new ImageAnalysisResultDto
                {
                    Caption = analysis.Caption?.Text,
                    CaptionConfidence = analysis.Caption?.Confidence
                };

                // TAGS
                if (analysis.Tags?.Values != null)
                {
                    foreach (var tag in analysis.Tags.Values)
                    {
                        dto.Tags.Add(new ImageTagDto
                        {
                            Name = tag.Name,
                            Confidence = tag.Confidence
                        });
                    }
                }

                // OBJECTS
                if (analysis.Objects?.Values != null)
                {
                    foreach (var obj in analysis.Objects.Values)
                    {
                        if (obj?.Tags == null || obj.Tags.Count == 0)
                            continue;

                        var mainTag = obj.Tags[0];

                        dto.Objects.Add(new ImageObjectDto
                        {
                            Name = mainTag.Name,
                            Confidence = mainTag.Confidence
                        });
                    }
                }

                return dto;
            }
            catch (Exception ex)
            {

                throw;
            }

        }


        public VehicleDistanceResult EvaluateVehicleDistance(ImageAnalysisResultDto analysis)
        {
            if (analysis == null || analysis.Tags == null || !analysis.Tags.Any())
                return VehicleDistanceResult.Uncertain;

            var keywords = new[] { "car", "truck" };
            var keywordsdistance = new[] { "parked" };
            // Helpers
            double TagConfidence(string name) =>
                analysis.Tags
                    .FirstOrDefault(t =>
                        t.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    ?.Confidence ?? 0;

            bool HasTag(string name, double minConfidence) =>
                TagConfidence(name) >= minConfidence;

            // 🔑 Valores clave
            var plateConfidence = TagConfidence("vehicle registration plate");
            var textConfidence = TagConfidence("text");
            var outdoorConfidence = TagConfidence("outdoor");
            var sky = TagConfidence("sky");

            var caption = analysis.Caption?.ToLowerInvariant() ?? "";

            // ===== REGLA SEMÁNTICA POR OBJECT =====
            var mainObject = analysis.Objects?.FirstOrDefault()?.Name?.ToLowerInvariant();

            bool containsAnyKeyword = keywords.Any(k => caption?.Contains(k, StringComparison.OrdinalIgnoreCase) == true);

            if (string.IsNullOrEmpty(caption) || (!containsAnyKeyword))
                return VehicleDistanceResult.Uncertain;

            // ===== REGLAS FUERTES: CARRO CERCA =====
            bool containsAnyKeywordTags = keywordsdistance.Any(k => caption.Contains(k, StringComparison.OrdinalIgnoreCase) == true);
            if (!containsAnyKeywordTags ||
                analysis.Objects.Any(o => o.Name != null && o.Name.Contains("plate", StringComparison.OrdinalIgnoreCase)) ||
                //sky < 0.96 ||
                //textConfidence >= 0.98 ||
                caption.Contains("close-up")
            )
            {
                return VehicleDistanceResult.Close;
            }

            // ===== REGLAS FUERTES: CARRO LEJOS =====
            if (
                outdoorConfidence >= 0.98
            //&& sky > 0.96
            )
            {
                return VehicleDistanceResult.Far;
            }

            return VehicleDistanceResult.Uncertain;
        }


        //PRIVATE METHODS
        private static string Normalize(string input) =>
          Regex.Replace(input.ToUpperInvariant(), @"[^A-Z0-9]", "");

        private BlobContainerClient GetContainerClient(string containerName)
        {
            return new BlobContainerClient(_connectionString, containerName);
        }

        private async Task<BlobClient> TakeImageFromBlobStorage(string imageUrl)
        {
            var uri = new Uri(imageUrl);

            var segments = uri.AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length < 2)
                throw new ArgumentException("La URL no es una URL válida de Azure Blob.");

            var containerName = segments[0];
            var blobName = string.Join('/', segments.Skip(1));

            var container = GetContainerClient(containerName);
            var blobClient = container.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
            {
                throw new Exception($"El blob '{blobName}' no existe");
            }
            return blobClient;
        }
    }
}
