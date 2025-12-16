using Application.Abstractions.Interfaces.Queries;
using Domain.Models;

namespace Application.Users.Queries
{
    public record GetAllUsersWithMessagesAgreemenQuery (List<int> MessageIds) : IQuery<IEnumerable<User>>
    {
    }
}
