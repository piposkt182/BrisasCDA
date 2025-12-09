using Domain.Models;

namespace Application.Abstractions
{
    public interface IMessageRepository
    {
        Task<Message> CreateMessage(Message message);
        Task<IEnumerable<Message>> GetAllMessages();
    }
}
