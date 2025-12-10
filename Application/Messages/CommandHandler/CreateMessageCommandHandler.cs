using Application.Abstractions;
using Application.Abstractions.Interfaces.CommandHandler;
using Application.Messages.Commands;
using Application.Utilities.Interfaces;
using Domain.Dto;
using Domain.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Application.Messages.CommandHandler
{
    public class CreateMessageCommandHandler : ICommandHandler<CreateMessageCommand, Message>
    {

        private readonly IMessageRepository _messageRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _accessToken;
        private readonly IBlobService _blobService;
        public CreateMessageCommandHandler(IMessageRepository messageRepository, IHttpClientFactory httpClientFactory, IOptions<MetaSettings> metaOptions, IBlobService blobService)
        {
            _messageRepository = messageRepository;
            _httpClientFactory = httpClientFactory;
            _accessToken = metaOptions.Value.AccessToken;
            _blobService = blobService;
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
                var imageUrl = await SaveImageAsync(command.mimeType, command.mediaId);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    var message = new Message
                    {
                        UserId = command.userId,
                        Number = command.number,
                        Text = command.text,
                        DateMessage = command.dateMessage,
                        ImageUrl = imageUrl,
                        MimeType = command.mimeType,
                        PaymentStatusId = 1
                    };
                    return await _messageRepository.CreateMessage(message);
                }
                else
                    throw new InvalidOperationException("Hubo un problema al guardar la imagen en el servidor.");
            }else
                throw new InvalidOperationException("Se esta intentando guardar un tipo de mensaje incorrecto.");
        }

        private async Task<string> SaveImageAsync( string mimeType, string mediaId)
        {
            try
            {
                string filePath = string.Empty;
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

                // 2️⃣ Preparar ruta de guardado
                var extension = ObtenerExtensionDesdeMime(mimeType);
                var folderName = DateTime.UtcNow.ToString("yyyyMMdd"); // Carpeta con la fecha
                var fileName = $"{Guid.NewGuid()}{extension}";         // Nombre único para evitar colisiones

                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folderName);
                Directory.CreateDirectory(uploadPath); // Crea la carpeta si no existe

                filePath = Path.Combine(uploadPath, fileName);

                // 3️⃣ Descargar y guardar imagen
                var imageBytes = await client.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(filePath, imageBytes);
                Console.WriteLine($"✅ Imagen guardada en: {filePath}");

                
                await _blobService.UploadToAzureAsync("referidos", imageBytes, "prueba");
                Console.WriteLine($"✅ Imagen guardada en Azure: {filePath}");
                return filePath;
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

        private string ObtenerExtensionDesdeMime(string mimeType)
        {
            return mimeType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".bin"
            };
        }
    }
}
