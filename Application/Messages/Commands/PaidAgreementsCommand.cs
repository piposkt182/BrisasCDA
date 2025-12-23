using Application.Abstractions.Interfaces.Commands;
using Domain.Dto;

namespace Application.Messages.Commands
{
    public record PaidAgreementsCommand(List<int> ids) : ICommand<PaidAgreementsResult> {}
}
