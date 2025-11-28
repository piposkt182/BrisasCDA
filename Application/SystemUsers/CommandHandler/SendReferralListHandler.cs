using Application.Abstractions;
using Application.Abstractions.Interfaces.CommandHandler;
using Application.SystemUsers.Commands;
using Domain.Dto;
using Domain.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Application.SystemUsers.CommandHandler
{
    public class SendReferralListHandler : ICommandHandler<SendReferralListCommand, Message>
    {
        private readonly IMessageRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string MetaApiUrl = "https://graph.facebook.com/v22.0/831368603392808/messages";
        private readonly string _accessToken;

        public SendReferralListHandler(IMessageRepository repository, IHttpClientFactory httpClientFactory, IOptions<MetaSettings> metaOptions)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _accessToken = metaOptions.Value.AccessToken;
        }
        public async Task<Message> HandleAsync(SendReferralListCommand query, CancellationToken cancellationToken = default)
        {
            var messageBody = GetAllMessages().Result;
            await SendWhatsAppTextMessageAsync(query.ToNumber, messageBody);
            //await SendWhatsAppTemplateMessageAsync(query.ToNumber);
            return new Message
            {
                Text = $"✅ Plantilla enviada al número {query.ToNumber}",
                Number = query.ToNumber,
                DateMessage = DateTime.UtcNow
            };
        }

        public async Task<string> GetAllMessages()
        {
            var messagesDB = await _repository.GetAllMessages();
            var messages =  messagesDB.Select(m => new { m.Number, m.Text }).ToList();
            var sb = new StringBuilder();

            foreach (var msg in messages)
            {
                sb.AppendLine($"📱 Número: {msg.Number}");
                sb.AppendLine($"💬 Mensaje: {msg.Text}");
                sb.AppendLine(new string('-', 30));
            }
            return sb.ToString();
        }

        public async Task SendWhatsAppTextMessageAsync(string toNumber, string message)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                //var body = new
                //{
                //    messaging_product = "whatsapp",
                //    to = toNumber,
                //    type = "template",
                //    template = new
                //    {
                //        name = "hello_world",
                //        language = new
                //        {
                //            code = "en_US"
                //        }
                //    }
                //};

                var body = new
                {
                    messaging_product = "whatsapp",
                    to = toNumber,
                    type = "text",
                    text = new
                    {
                        body = message
                    }
                };

                var jsonBody = JsonSerializer.Serialize(body);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(MetaApiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Error HTTP {response.StatusCode}");
                    throw new InvalidOperationException($"Error al enviar mensaje a WhatsApp ({toNumber}). Detalle: {responseContent}");
                }

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                // Errores de red o HTTP (por ejemplo: 400, 401, 403, 500)
                Console.WriteLine($"❌ Error HTTP al enviar mensaje a {toNumber}: {ex.Message}");
                throw new InvalidOperationException($"Error al enviar mensaje a WhatsApp ({toNumber}).", ex);
            }
            catch (TaskCanceledException ex)
            {
                // Timeout o cancelación de la solicitud
                Console.WriteLine($"⏰ Tiempo de espera agotado al enviar mensaje a {toNumber}: {ex.Message}");
                throw new TimeoutException($"El envío del mensaje a {toNumber} excedió el tiempo de espera.", ex);
            }
            catch (Exception ex)
            {
                // Cualquier otro tipo de error inesperado
                Console.WriteLine($"⚠️ Error inesperado al enviar mensaje a {toNumber}: {ex.Message}");
                throw; // Re-lanza para que se maneje más arriba si es necesario
            }
        }

        public async Task SendWhatsAppTemplateMessageAsync(string toNumber, string message = "holaaaaaaaaaaaaaaa")
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                var body = new
                {
                    messaging_product = "whatsapp",
                    to = toNumber,
                    type = "template",
                    template = new
                    {
                        name = "hello_world",
                        language = new
                        {
                            code = "en_US"
                        }
                    }
                };
                var jsonBody = JsonSerializer.Serialize(body);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(MetaApiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"📩 Respuesta de Meta: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Error HTTP {response.StatusCode}");
                    throw new InvalidOperationException($"Error al enviar mensaje a WhatsApp ({toNumber}). Detalle: {responseContent}");
                }

                //response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                // Errores de red o HTTP (por ejemplo: 400, 401, 403, 500)
                Console.WriteLine($"❌ Error HTTP al enviar mensaje a {toNumber}: {ex.Message}");
                throw new InvalidOperationException($"Error al enviar mensaje a WhatsApp ({toNumber}).", ex);
            }
            catch (TaskCanceledException ex)
            {
                // Timeout o cancelación de la solicitud
                Console.WriteLine($"⏰ Tiempo de espera agotado al enviar mensaje a {toNumber}: {ex.Message}");
                throw new TimeoutException($"El envío del mensaje a {toNumber} excedió el tiempo de espera.", ex);
            }
            catch (Exception ex)
            {
                // Cualquier otro tipo de error inesperado
                Console.WriteLine($"⚠️ Error inesperado al enviar mensaje a {toNumber}: {ex.Message}");
                throw; // Re-lanza para que se maneje más arriba si es necesario
            }
        }
    }
}
