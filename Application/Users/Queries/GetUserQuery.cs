using Application.Abstractions.Interfaces.Queries;
using Domain.Models;

namespace Application.Users.Queries
{
    public record GetUserQuery(string whatsappNumber) : IQuery<User>
    {
    }
}
