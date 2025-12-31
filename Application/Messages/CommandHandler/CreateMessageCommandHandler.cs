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
        private readonly string _AzureIAEnpoint;
        private readonly string _AzureApiKey;
        private readonly ImageAnalysisClient _client;
        private readonly string _connectionString;
        private readonly WhatsAppMessages _whatsAppMessages;
        private readonly AzureService _azureService;
        private readonly IAgreementRepository _agreementgeRepository;
        public CreateMessageCommandHandler(
                                            IMessageRepository messageRepository,
                                            IOptions<MetaSettings> metaOptions,
                                            IBlobService blobService,
                                            IConfiguration configuration,
                                            WhatsAppMessages whatsAppMessages,
                                            AzureService azureService,
                                            IAgreementRepository agreementgeRepository)
        {
            _messageRepository = messageRepository;
            _AzureIAEnpoint = configuration["AzureIA:Endpoint"];
            _AzureApiKey = configuration["AzureIA:ApiKey"];
            _connectionString = configuration["Azure:BlobStorage"];
            _whatsAppMessages = whatsAppMessages;
            _client = new ImageAnalysisClient(
                new Uri(_AzureIAEnpoint),
                new AzureKeyCredential(_AzureApiKey)
            );
            _azureService = azureService;
            _agreementgeRepository = agreementgeRepository;

        }
        public async Task<Message> HandleAsync(CreateMessageCommand command, CancellationToken cancellationToken = default)
        {
            return command.typeMessage switch
            {
                1 => await HandleTextMessageAsync(command),
                2 => await HandleImageMessageAsync(command),
                _ => throw new InvalidOperationException(
                    "Se está intentando guardar un tipo de mensaje incorrecto.")
            };
        }

        private async Task<Message> HandleTextMessageAsync(CreateMessageCommand command)
        {
            var evaluation = await ValidateImage("https://referidosstorage.blob.core.windows.net/referidos/camionetaroja.png");
            await _whatsAppMessages.SendWhatsAppTextAsync(
                command.number,
                evaluation);

            var message = new Message
            {
                UserId = command.userId,
                Number = command.number,
                Text = command.text,
                PlateVehicle = "plate",
                DateMessage = command.dateMessage,
                ImageUrl = "https://referidosstorage.blob.core.windows.net/referidos/AveoAzul.png",
                MimeType = command.mimeType,
                ImageName = "Prueba",
            };
            if (evaluation == VehicleDistanceResult.Far || evaluation == VehicleDistanceResult.Rejected || evaluation == VehicleDistanceResult.Close)
            {
                message.PaymentStatusId = evaluation == VehicleDistanceResult.Far ? (int)PaymentStatusId.Pending : (int)PaymentStatusId.Rejected;
                message.AgreementId = (await _agreementgeRepository.GetAgreementByCellPhone(command.number))?.Id;
                return await _messageRepository.CreateMessage(message);
            }
            return message;
        }

        private async Task<Message> HandleImageMessageAsync(CreateMessageCommand command)
        {
            var (imageUrl, fileName, plateImg) =
                await _azureService.SaveImageAsync(
                    command.mimeType,
                    command.mediaId,
                    command.number);

            var evaluation = await ValidateImage(imageUrl);

            await _whatsAppMessages.SendWhatsAppTextAsync(
                command.number,
                evaluation);

            var message = new Message
            {
                UserId = command.userId,
                Number = command.number,
                Text = command.text,
                PlateVehicle = plateImg,
                DateMessage = command.dateMessage,
                ImageUrl = imageUrl,
                MimeType = command.mimeType,
                ImageName = fileName,
            };
            if (evaluation == VehicleDistanceResult.Far || evaluation == VehicleDistanceResult.Rejected || evaluation ==  VehicleDistanceResult.Close)
            {
                message.PaymentStatusId = evaluation == VehicleDistanceResult.Far ? (int)PaymentStatusId.Pending : (int)PaymentStatusId.Rejected; 
                message.AgreementId = (await _agreementgeRepository.GetAgreementByCellPhone(command.number))?.Id;
                return await _messageRepository.CreateMessage(message);
            }
            return message;
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
            //Console.WriteLine(JsonSerializer.Serialize(analysis, new JsonSerializerOptions { WriteIndented = true }));
            return EvaluateVehicleDistance(analysis);
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

            var keywords = new[] { "car", "truck", "motorcycle", "license plate" };
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

            bool containsAnyKeyword = keywords.Any(k => caption?.Contains(k, StringComparison.OrdinalIgnoreCase) == true);

            if (string.IsNullOrEmpty(caption) || (!containsAnyKeyword))
                return VehicleDistanceResult.Uncertain;

            // ===== REGLAS FUERTES: CARRO CERCA =====
            bool containsAnyKeywordTags = keywordsdistance.Any(k => caption.Contains(k, StringComparison.OrdinalIgnoreCase) == true);
            if (!containsAnyKeywordTags)
            {
                return VehicleDistanceResult.Rejected;
            }else if (caption.Contains("parked"))
            {
                return VehicleDistanceResult.Far;
            }

            return VehicleDistanceResult.Rejected;
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
