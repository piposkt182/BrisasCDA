
using System.Text.Json.Serialization;

namespace Domain.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Number { get; set; }
        public string? Text { get; set; }
        public DateTime DateMessage { get; set; }
        public string? ImageUrl { get; set; }
        public string? MimeType { get; set; }
        public int PaymentStatusId { get; set; }
        public string? ImageName { get; set; }
        public int? AgreementId { get; set; }
        public string? PlateVehicle {  get; set; }
        public DateTime? DateCreated { get; set; }

        [JsonIgnore]
        public User User { get; set; } 
        public PaymentStatus PaymentStatus { get; set; }
        public Agreement? Agreements { get; set; }
    }
}
