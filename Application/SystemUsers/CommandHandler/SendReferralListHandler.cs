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
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

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
            Console.WriteLine($"📩 Respuesta de Meta: {responseContent}");

            response.EnsureSuccessStatusCode();
        }


        public async Task SendWhatsAppTemplateMessageAsync(string toNumber)
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

            // 📡 Hacer la petición POST
            var response = await client.PostAsync(MetaApiUrl, content);

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"📩 Respuesta de Meta: {responseContent}");

            response.EnsureSuccessStatusCode();
        }
    }
}
