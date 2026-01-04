using Application.Abstractions;
using Application.Abstractions.Interfaces.CommandHandler;
using Application.Messages.Commands;
using Domain.Models;

namespace Application.Messages.CommandHandler
{
    public class ApproveMessageHandler : ICommandHandler<ApproveMessageCommand, Message>
    {
        private readonly IMessageRepository _messageRepository;
        public ApproveMessageHandler(IMessageRepository messageRepository)
        {
                _messageRepository = messageRepository;
        }
        public async Task<Message> HandleAsync(ApproveMessageCommand command, CancellationToken cancellationToken = default)
        {
            return await _messageRepository.ApproveMessage(command.id);
        }
    }
}
