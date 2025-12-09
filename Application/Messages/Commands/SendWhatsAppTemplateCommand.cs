
using Application.Abstractions.Interfaces.Commands;

namespace Application.Messages.Commands
{
    public record SendWhatsAppTemplateCommand(string SubscriberId, string Name, string VehiclePlate, string RTMDate) : ICommand<string>
    {
    }
}
