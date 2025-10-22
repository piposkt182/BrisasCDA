using Application.Abstractions.Interfaces.Commands;
using Domain.Models;

namespace Application.Users.Commands
{
    public record CreateUserCommand(string Name, string Ws_Id) : ICommand<User> 
    {
    }
}
