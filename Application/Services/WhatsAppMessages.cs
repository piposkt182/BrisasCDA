using Application.Utilities.Enums;
using Domain.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Application.Services
{
    public class WhatsAppMessages
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string MetaApiUrl = "https://graph.facebook.com/";
        private readonly string _phoneWhatsappId;
        private readonly string _accessToken;

        public WhatsAppMessages(IHttpClientFactory httpClientFactory, IConfiguration config, IOptions<MetaSettings> metaOptions)
        {
            _httpClientFactory = httpClientFactory;
            _phoneWhatsappId = config["WhasppAgreements:PhoneNumberId"];
            _accessToken = metaOptions.Value.AccessToken;
        }
        public async Task SendWhatsAppTextAsync(string toPhoneNumber,  VehicleDistanceResult evaluateVehicleDistance)
        {
            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

            var messageText = MessageText(evaluateVehicleDistance);

            var payload = new
            {
                messaging_product = "whatsapp",
                to = toPhoneNumber,
                type = "text",
                text = new
                {
                    body = messageText
                }
            };

            var json = JsonSerializer.Serialize(payload);

            var response = await client.PostAsync(
                $"{MetaApiUrl}{_phoneWhatsappId}/messages",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error enviando WhatsApp: {error}");
            }
        }

        //private methods
        private string MessageText(VehicleDistanceResult vehicleDistance)
        {
            var message = vehicleDistance switch
            {
                VehicleDistanceResult.Far =>
                    "✅ !!La foto es correcta!!\r\n \r\n  Estamos procesando la información.",

                VehicleDistanceResult.Close =>
                    "⚠️ Por favor toma la foto un poco más lejos, que se vea el entorno.",

                VehicleDistanceResult.Uncertain =>
                    "❌ La imagen no es válida\r\n \r\nNo logramos identificar correctamente el vehículo ni su entorno.\r\n \r\nPor favor envía una nueva foto donde se vea el vehículo completo y el lugar donde se encuentra 🚗📸",

                _ => "Resultado desconocido"
            };
            return message;
        }

    }
}
