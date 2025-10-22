using Application.Abstractions;
using Application.Abstractions.Interfaces.QueryHandler;
using Application.SystemUsers.Queries;
using Domain.Models;

namespace Application.SystemUsers.QueryHandler
{
    public class GetSystemUserByUserNameHandler : IQueryHandler<GetSystemUserByUserNameQuery, SystemUser>
    {
        private readonly ISystemUserRepository _repository;
        public GetSystemUserByUserNameHandler(ISystemUserRepository repository)
        {
            _repository = repository;
        }
        public async Task<SystemUser> HandleAsync(GetSystemUserByUserNameQuery query, CancellationToken cancellationToken = default)
        {
            return await _repository.GetUserByCredencials(query.userName, query.password);
        }
    }
}
