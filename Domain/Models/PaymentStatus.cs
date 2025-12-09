using System.Text.Json.Serialization;

namespace Domain.Models
{
    public class PaymentStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public ICollection<Message> Messages { get; set; }
    }
}
