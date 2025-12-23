using Application.Abstractions;
using Application.Abstractions.Interfaces.CommandHandler;
using Application.Messages.Commands;
using Domain.Dto;

namespace Application.Messages.CommandHandler
{
    public class PaidAgreementsHandler : ICommandHandler<PaidAgreementsCommand, PaidAgreementsResult>
    {
        private readonly IMessageRepository _messageRepository;

        public PaidAgreementsHandler(IMessageRepository messageRepository)
        {
                _messageRepository = messageRepository;
        }

        async Task<PaidAgreementsResult> ICommandHandler<PaidAgreementsCommand, PaidAgreementsResult>.HandleAsync(PaidAgreementsCommand command, CancellationToken cancellationToken)
        {
            return await _messageRepository.PaidAgreementsAsync(command.ids);
        }
    }
}
