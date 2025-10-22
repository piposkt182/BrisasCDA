using Application.Abstractions.Interfaces.Commands;
using Domain.Models;

namespace Application.SystemUsers.Commands
{
    public record CreateSystemUserCommand(string userName, string password, int roleId): ICommand<SystemUser>
    {
    }
}
