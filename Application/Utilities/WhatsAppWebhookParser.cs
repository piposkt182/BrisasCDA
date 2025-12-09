using Application.Utilities.Interfaces;
using Domain.Dto.CDABrisasAPI.Dto;
using System.Text.Json;

namespace Application.Utilities
{
    public class WhatsAppWebhookParser : IWhatsAppWebhookParser
    {
        public (string Name, string WsId, string? Text, DateTime Timestamp, string MimeType, string mediaId, int typeMessage)? ExtractContactInfo(string json)
        {
            string? text = string.Empty;
            string timestamp = string.Empty;
            string mimeType = string.Empty;
            string caption = string.Empty;
            string mediaId = string.Empty;
            int typeMessage = 0;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var webhookEvent = JsonSerializer.Deserialize<WhatsAppWebhookDto>(json, options);
            var name = webhookEvent?.Entry?[0]?.Changes?[0]?.Value?.Contacts?[0]?.Profile?.Name;
            var wsId = webhookEvent?.Entry?[0]?.Changes?[0]?.Value?.Contacts?[0]?.Wa_Id;
            timestamp = webhookEvent?.Entry?[0]?.Changes?[0]?.Value?.Messages?[0]?.Timestamp;
            var dateMessage = timestamp != null ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(timestamp)).DateTime : DateTime.UtcNow;

            if (webhookEvent?.Entry?[0]?.Changes?[0]?.Value?.Messages?[0].Type == "text")
            {
                text = webhookEvent?.Entry?[0]?.Changes?[0]?.Value?.Messages?[0]?.Text?.Body;
                if (wsId == "573012282168" && text!.Contains("Referidos pendientes"))
                    typeMessage = 3;
                else
                    typeMessage = 1;
            }
            else if (webhookEvent?.Entry?[0]?.Changes?[0]?.Value?.Messages?[0].Type == "image")
            {
                 (mimeType, text, mediaId) = whatsAppImage(json).Value;
                typeMessage = 2;
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(wsId))
                return null;

            return (name, wsId, text, dateMessage, mimeType, mediaId, typeMessage);
        }

        public ( string MimeType, string Text, string mediaId)? whatsAppImage(string json)
        {
            string? mimeType = string.Empty;
            string? text = string.Empty;
            string? mediaId = string.Empty;

            var jsonObj = JsonDocument.Parse(json);
            var messages = jsonObj.RootElement
                                  .GetProperty("entry")[0]
                                  .GetProperty("changes")[0]
                                  .GetProperty("value")
                                  .GetProperty("messages");

            if (messages.GetArrayLength() > 0 && messages[0].GetProperty("type").GetString() == "image")
            {
                var imageElement = messages[0].GetProperty("image");
                mimeType = imageElement.GetProperty("mime_type").GetString();
                text = imageElement.TryGetProperty("caption", out var captionProp) ? captionProp.GetString() : null;
                mediaId = imageElement.GetProperty("id").GetString();
            }
            return (mimeType, text, mediaId);
        }

        
    }
}
