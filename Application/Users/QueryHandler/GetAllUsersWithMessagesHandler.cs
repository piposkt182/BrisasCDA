using Application.Abstractions;
using Application.Abstractions.Interfaces.QueryHandler;
using Application.Users.Queries;
using Application.Utilities.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Http;

namespace Application.Users.QueryHandler
{
    public class GetAllUsersWithMessagesHandler : IQueryHandler<GetAllUsersWithMessagesQuery, IEnumerable<User>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBlobService _blobService;

        public GetAllUsersWithMessagesHandler(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor, IBlobService blobService)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
            _blobService = blobService;
        }

        public async Task<IEnumerable<User>> HandleAsync(GetAllUsersWithMessagesQuery query, CancellationToken cancellationToken = default)
        {
            var users = await _userRepository.GetAllUsersWithMessages();

            foreach (var user in users)
            {
                if (user.Messages != null && !string.IsNullOrEmpty(user.Messages.ImageUrl))
                {
                    user.Messages.ImageUrl = await _blobService.GetImageWithSasFromUrl(user.Messages.ImageUrl);
                }
            }

            return users;
        }

    }
}
