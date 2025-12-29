
using Application.Utilities.Interfaces;
using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Domain.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Application.Services
{
    public class AzureService
    {
        private readonly IBlobService _blobService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _AzureIAEnpoint;
        private readonly string _AzureApiKey;
        private readonly string _accessToken;
        public AzureService(IBlobService blobService, IHttpClientFactory httpClientFactory, IConfiguration configuration, IOptions<MetaSettings> metaOptions)
        {
            _blobService = blobService;
            _httpClientFactory = httpClientFactory;

            _AzureIAEnpoint = configuration["AzureIA:Endpoint"];
            _AzureApiKey = configuration["AzureIA:ApiKey"];
            _accessToken = metaOptions.Value.AccessToken;
        }
        public async Task<(string filePath, string fileName, string plateImg)> SaveImageAsync(string mimeType, string mediaId, string number)
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

        //Private methods
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

        private static string Normalize(string input) =>
        Regex.Replace(input.ToUpperInvariant(), @"[^A-Z0-9]", "");
    }
}
