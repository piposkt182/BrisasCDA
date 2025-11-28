
namespace Application.Utilities.Dtos
{
    public class UserDto
    {
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public string? phone { get; set; }
        public string? whatsapp_phone { get; set; }
        public string? email { get; set; }
        public string? gender { get; set; }
        public Boolean has_opt_in_sms { get; set; }
        public Boolean has_opt_in_email { get; set; }
        public string? consent_phrase { get; set; }
        public string? SubscriberId { get; set; }
    }
}
