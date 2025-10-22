using Application.Abstractions.Interfaces.Queries;
using Domain.Models;

namespace Application.SystemUsers.Queries
{
    public record GetSystemUserByUserNameQuery (string userName, string password) : IQuery<SystemUser>{}
}
