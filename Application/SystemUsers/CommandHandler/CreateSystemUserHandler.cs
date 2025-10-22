using Application.Abstractions;
using Application.Abstractions.Interfaces.CommandHandler;
using Application.SystemUsers.Commands;
using Domain.Models;

namespace Application.SystemUsers.CommandHandler
{
    public class CreateSystemUserHandler : ICommandHandler<CreateSystemUserCommand, SystemUser>
    {
        private readonly ISystemUserRepository _repository;
        public CreateSystemUserHandler(ISystemUserRepository repository)
        {
            _repository = repository;
        }
        public async Task<SystemUser> HandleAsync(CreateSystemUserCommand command, CancellationToken cancellationToken = default)
        {
            var systemUser = new SystemUser { UserName = command.userName, PasswordHash = command.password, RoleId = command.roleId};
            return await _repository.CreateSystemUser(systemUser);
        }
    }
}
