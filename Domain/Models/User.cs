
namespace Domain.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Ws_Id { get; set; }
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
