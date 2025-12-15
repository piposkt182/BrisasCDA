using Application.Abstractions.Interfaces.Commands;
using Domain.Models;

namespace Application.Messages.Commands
{
    public record SetUserForAgreementCommand(List<Dictionary<string, string>> ExcelField) : ICommand<IEnumerable<Message>>
    {
    }
}
