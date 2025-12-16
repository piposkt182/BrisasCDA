using Application.Abstractions;
using Application.Abstractions.Interfaces.CommandHandler;
using Application.Messages.Commands;
using Application.Utilities.Mapper;
using Domain.Models;

namespace Application.Messages.CommandHandler
{
    public class SetUserForAgreementHandler : ICommandHandler<SetUserForAgreementCommand, IEnumerable<Message>>
    {
        private readonly IMessageRepository _messageRepository;
        public SetUserForAgreementHandler(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }
        public async Task<IEnumerable<Message>> HandleAsync(SetUserForAgreementCommand command, CancellationToken cancellationToken = default)
        {
            if (command.ExcelField.Count == 0)
                throw new InvalidOperationException("Excel field does not have records.");

            var aggrements = VerificationAgreementMapper.Map(command.ExcelField);
            List<string> plates = [.. aggrements.Select(a => a.Placa)];
            var messages = await _messageRepository.SetAllMessagesForAgreement(plates);

            return messages;
        }
    }
}
