using Application.Abstractions.Interfaces.CommandHandler;
using Application.Messages.Commands;
using Application.Services;

namespace Application.Messages.CommandHandler
{
    public class SendGenericWhatsappHandler : ICommandHandler<SendGenericWhatsappCommand, bool>
    {
        public readonly WhatsAppMessages _whatsAppMessages;
        public SendGenericWhatsappHandler(WhatsAppMessages whatsAppMessages)
        {
            _whatsAppMessages = whatsAppMessages;
        }
        public async Task<bool> HandleAsync(SendGenericWhatsappCommand command, CancellationToken cancellationToken = default)
        {
            await _whatsAppMessages.SendGenericWhatsAppAsync(command.id);
            return true;
        }
    }
}
