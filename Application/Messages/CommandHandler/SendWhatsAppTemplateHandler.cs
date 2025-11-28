using Application.Abstractions.Interfaces.CommandHandler;
using Application.Messages.Commands;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Application.Messages.CommandHandler
{
    public class SendWhatsAppTemplateHandler : ICommandHandler<SendWhatsAppTemplateCommand, string>
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public SendWhatsAppTemplateHandler(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;

            // ✅ Configurar la URL base de ManyChat
            _httpClient.BaseAddress = new Uri("https://api.manychat.com/");

            // ✅ Leer el token desde appsettings.json o variable de entorno
            var token = _config["ManyChat:AccessToken"];
            if (string.IsNullOrEmpty(token))
                throw new Exception("El token de ManyChat no está configurado. Agrega ManyChat:AccessToken en appsettings.json.");

            // ✅ Configurar headers por defecto
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<string> HandleAsync(SendWhatsAppTemplateCommand command, CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                subscriber_id = command.SubscriberId,
                data = new
                {
                    template_name = "RecordatorioMensual",
                    language = new { policy = "deterministic", code = "es" },
                    components = new[]
            {
                new
                {
                    type = "body",
                    parameters = new[]
                    {
                        new { type = "text", text = command.Name },
                        new { type = "text", text = command.VehiclePlate },
                        new { type = "text", text = command.RTMDate }
                    }
                }
            }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.manychat.com/fb/sending/sendTemplate", content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error al enviar mensaje ({response.StatusCode}): {responseText}");
            }

            return $"Mensaje enviado correctamente. Respuesta: {responseText}";
        }
    }
}
