
using Application.Abstractions.Interfaces.Commands;
using Application.Utilities.Dtos;

namespace Application.Manychat.Command
{
    public record SetSurveyFieldsToUsersCommand(int SubscriberIds) : ICommand<ManyChatUserResponse> 
    { }
    
}
