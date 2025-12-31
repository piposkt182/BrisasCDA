
namespace Domain.Models
{
    public class Agreement
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? CellPhoneNumber { get; set; }
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
