using Application.Utilities.Dtos;
using Application.Utilities.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Application.Utilities
{
    public class ManyChatService : IManyChatService
    {
        private readonly HttpClient _httpClient;
        public ManyChatService(IConfiguration config)
        {
            var token = config["ManyChat:AccessToken"];
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.manychat.com/")
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public async Task<string> CrearUsuarioAsync(UserDto user)
        {
            var payload = new UserManyChat
            {
                first_name = user.first_name,
                last_name = user.last_name,
                phone = user.phone,
                whatsapp_phone = user.whatsapp_phone,
                email = user.email,
                gender = user.gender,
                has_opt_in_sms = user.has_opt_in_sms,
                has_opt_in_email = user.has_opt_in_email,
                consent_phrase = user.consent_phrase
            };
            var response = await _httpClient.PostAsync(
                "fb/subscriber/createSubscriber",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            );

            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error creando usuario {user.first_name} {user.last_name}: {content}");

            using var doc = JsonDocument.Parse(content);
            var subscriberId = doc.RootElement
               .GetProperty("data")
               .GetProperty("id")
               .GetString();
            return doc.RootElement.GetProperty("data").GetProperty("id").GetRawText();
        }

        public async Task AgregarCaracteristicasAsync(UserDto usuario)
        {
            var payload = new
            {
                subscriber_id = usuario.SubscriberId,
                fields = new[]
                {
                    new { field_id = 13821070, field_value = usuario.email },   // ejemplo
                    new { field_id = 13821066, field_value = usuario.email }        // ejemplo
                }
            };
            var response = await _httpClient.PostAsync(
                "fb/subscriber/setCustomField",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
                Console.WriteLine($"❌ Error asignando {usuario.email} al usuario {usuario.email}");
        }

        public async Task EjecutarFlowAsync(UserDto usuario, string flowNs)
        {
            var payload = new
            {
                subscriber_id = usuario.SubscriberId,
                flow_ns = flowNs
            };

            var response = await _httpClient.PostAsync(
                "fb/sending/sendFlow",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error ejecutando flow para {usuario.first_name} {usuario.last_name}: {msg}");
            }
        }
    }
}
