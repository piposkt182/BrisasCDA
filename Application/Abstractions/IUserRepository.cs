using Domain.Models;

namespace Application.Abstractions
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersWithMessages();
        Task<User> CreateUser(User user);
        Task<User> GetUser(string whatsappNumber);
        Task<IEnumerable<User>> GetAllUsersWithMessagesAgreement(List<int> messagesId);
    }
}
