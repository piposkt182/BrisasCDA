
using Application.Abstractions.Interfaces.Commands;

namespace Application.Manychat.Command
{
    public record SaveSurveyUserCommand (string WhatsappNumber, string Field1, string Field2, string Field3) : ICommand<bool>
    {
    }
}
