using Application.Abstractions.Interfaces.Queries;
using Domain.Models;

namespace Application.Users.Queries
{
    public class GetAllUsersWithMessagesQuery : IQuery<IEnumerable<User>>
    {
    }
}
