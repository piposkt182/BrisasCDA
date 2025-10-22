
namespace Application.Utilities.Interfaces
{
    public interface IWhatsAppWebhookParser
    {
        (string Name, string WsId, string? Text, DateTime Timestamp, string MimeType, string mediaId, int typeMessage)? ExtractContactInfo(string json);
    }
}
