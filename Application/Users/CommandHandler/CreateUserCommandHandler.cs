using Application.Abstractions;
using Application.Abstractions.Interfaces.CommandHandler;
using Application.Users.Commands;
using Domain.Models;

namespace Application.Users.CommandHandler
{
    public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, User>
    {
        private readonly IUserRepository _userRepository;

        public CreateUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
        {
            var user = new User
            {
                Name = command.Name,
                Ws_Id = command.Ws_Id
            };
            return await _userRepository.CreateUser(user);
        }
    }
}
