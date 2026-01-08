using Application.Abstractions.Interfaces.Commands;

namespace Application.Messages.Commands
{
    public  record SendGenericWhatsappCommand(string id) : ICommand<bool> { }
}
