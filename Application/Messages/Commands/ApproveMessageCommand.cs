using Application.Abstractions.Interfaces.Commands;
using Domain.Models;

namespace Application.Messages.Commands
{
    public record ApproveMessageCommand (int id) : ICommand<Message>
    {
    }
}
