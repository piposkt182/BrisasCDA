using Application.Abstractions.Interfaces.Queries;
using Domain.Models;

namespace Application.Messages.Queries
{
    public class GetAllMessagesQuery :IQuery<IEnumerable<Message>>
    {
    }
}
