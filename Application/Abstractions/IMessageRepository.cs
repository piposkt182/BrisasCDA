using Domain.Dto;
using Domain.Models;

namespace Application.Abstractions
{
    public interface IMessageRepository
    {
        Task<Message> CreateMessage(Message message);
        Task<IEnumerable<Message>> GetAllMessages();
        Task<IEnumerable<Message>> SetAllMessagesForAgreement(List<string> plate);
        Task<PaidAgreementsResult> PaidAgreementsAsync(List<int> ids);
        Task<Message> ApproveMessage(int id);
    }
}
