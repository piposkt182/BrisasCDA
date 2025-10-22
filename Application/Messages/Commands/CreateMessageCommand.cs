

using Application.Abstractions.Interfaces.Commands;
using Domain.Models;

namespace Application.Messages.Commands
{
    public record CreateMessageCommand(int userId, string number, string text, DateTime dateMessage, string mimeType, string mediaId, int typeMessage) : ICommand<Message>
    {
    }
}
