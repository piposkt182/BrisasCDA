using Application.Abstractions;
using Application.Abstractions.Interfaces.CommandHandler;
using Application.Messages.Commands;
using Application.Utilities.Enums;
using Application.Utilities.Interfaces;
using Azure;
using Azure.AI.Vision.ImageAnalysis;
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

        public CreateMessageCommandHandler(IMessageRepository messageRepository, IHttpClientFactory httpClientFactory, IOptions<MetaSettings> metaOptions, IBlobService blobService, IConfiguration configuration)
        {
            _messageRepository = messageRepository;
            _httpClientFactory = httpClientFactory;
            _accessToken = metaOptions.Value.AccessToken;
            _blobService = blobService;
            _AzureIAEnpoint = configuration["AzureIA:Endpoint"];
            _AzureApiKey = configuration["AzureIA:ApiKey"];
        }
        public async Task<Message> HandleAsync(CreateMessageCommand command, CancellationToken cancellationToken = default)
        {
            if (command.typeMessage == 1)
            {
                var message = new Message
                {
                    UserId = command.userId,
                    Number = command.number,
                    Text = command.text,
                    DateMessage = command.dateMessage,
                    PaymentStatusId = 1
                };
                return await _messageRepository.CreateMessage(message);
            }else if (command.typeMessage == 2)
            {
                string imageUrl = string.Empty;
                string fileName = string.Empty;
                string plateImg = string.Empty;
                (imageUrl, fileName, plateImg) = await SaveImageAsync(command.mimeType, command.mediaId, command.number);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    var message = new Message
                    {
                        UserId = command.userId,
                        Number = command.number,
                        Text = command.text,
                        PlateVehicle = plateImg,
                        DateMessage = command.dateMessage,
                        ImageUrl = imageUrl,
                        MimeType = command.mimeType,
                        PaymentStatusId = (int)PaymentStatusId.Paid,
                        ImageName = fileName
                    };
                    return await _messageRepository.CreateMessage(message);
                }
                else
                    throw new InvalidOperationException("Hubo un problema al guardar la imagen en el servidor.");
            }else
                throw new InvalidOperationException("Se esta intentando guardar un tipo de mensaje incorrecto.");
        }

        private async Task<(string filePath, string fileName, string plateImg)> SaveImageAsync( string mimeType, string mediaId, string number)
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

                //QUITAR
                //filePath = "https://referidosstorage.blob.core.windows.net/referidos/20251223_213910_573012282168";
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

            // ✅ Firma correcta: (Uri, VisualFeatures, options?)
            Response<ImageAnalysisResult> response = await client.AnalyzeAsync(
                new Uri(imageUrl),
                VisualFeatures.Read
            );

            ImageAnalysisResult result = response.Value;

            // ✅ OCR vive en result.Read (no ReadResult)
            if (result.Read == null)
                return null;

            foreach (DetectedTextBlock block in result.Read.Blocks)
            {
                foreach (DetectedTextLine line in block.Lines)
                {
                    var text = Normalize(line.Text);

                    // Placa Colombia básica: ABC123
                    if (Regex.IsMatch(text, @"^[A-Z]{3}\d{3}$"))
                        return text;
                }
            }

            return null;
        }

        private static string Normalize(string input) =>
            Regex.Replace(input.ToUpperInvariant(), @"[^A-Z0-9]", "");
    }
}
