using Application.Abstractions.Interfaces.Commands;
using Domain.Models;

namespace Application.SystemUsers.Commands
{
    public record SendReferralListCommand(string ToNumber) : ICommand<Message>
    {
    }
}
