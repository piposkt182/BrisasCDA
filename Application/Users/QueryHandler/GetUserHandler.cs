using Application.Abstractions;
using Application.Abstractions.Interfaces.QueryHandler;
using Application.Users.Queries;
using Domain.Models;

namespace Application.Users.QueryHandler
{
    public class GetUserHandler : IQueryHandler<GetUserQuery, User>
    {
        private readonly IUserRepository _userRepository;
        public GetUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<User> HandleAsync(GetUserQuery query, CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetUser(query.whatsappNumber);
        }
    }
}
