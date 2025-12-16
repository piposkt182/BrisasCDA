using Application.Abstractions;
using Application.Abstractions.Interfaces.QueryHandler;
using Application.Users.Queries;
using Application.Utilities.Interfaces;
using Domain.Models;

namespace Application.Users.QueryHandler
{
    public class GetAllUsersWithMessagesAgreemenHandler : IQueryHandler<GetAllUsersWithMessagesAgreemenQuery, IEnumerable<User>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IBlobService _blobService;

        public GetAllUsersWithMessagesAgreemenHandler(IUserRepository userRepository, IBlobService blobService)
        {
            _userRepository = userRepository;
            _blobService = blobService;
        }
        public async Task<IEnumerable<User>> HandleAsync(GetAllUsersWithMessagesAgreemenQuery query, CancellationToken cancellationToken = default)
        {
            var users = await _userRepository.GetAllUsersWithMessagesAgreement(query.MessageIds);
            foreach (var user in users)
            {
                foreach (var messages in user?.Messages)
                {
                    if (user.Messages != null && !string.IsNullOrEmpty(messages.ImageUrl))
                    {
                        messages.ImageUrl = await _blobService.GetImageWithSasFromUrl(messages.ImageUrl);
                    }
                }
            }
            return users;
        }
    }
}
