using Application.Abstractions;
using Application.Abstractions.Interfaces.CommandHandler;
using Application.Manychat.Command;
using Domain.Models;

namespace Application.Manychat.CommandHandler
{
    public class SaveSurveyUserHandler : ICommandHandler<SaveSurveyUserCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        public SaveSurveyUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<bool> HandleAsync(SaveSurveyUserCommand command, CancellationToken cancellationToken = default)
        {
            var survey = new SurveyResult
            {
                WhatsappNumber = command.WhatsappNumber,
                DateCreated = DateTime.Now,
                Field1 = command.Field1,
                Field2 = command.Field2,
                Field3 = command.Field3
            };
            return await _userRepository.SaveSurveyUser(survey);
        }
    }
}
