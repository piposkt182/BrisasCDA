using Application.Abstractions;
using Application.Abstractions.Interfaces.QueryHandler;
using Application.Users.Queries;
using Domain.Models;
using Microsoft.AspNetCore.Http;

namespace Application.Users.QueryHandler
{
    public class GetAllUsersWithMessagesHandler : IQueryHandler<GetAllUsersWithMessagesQuery, IEnumerable<User>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetAllUsersWithMessagesHandler(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<User>> HandleAsync(GetAllUsersWithMessagesQuery query, CancellationToken cancellationToken = default)
        {
            var users = await _userRepository.GetAllUsersWithMessages();

            // Construir la URL base (ej: https://localhost:7124)
            var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";

            foreach (var user in users)
            {
                if (user.Messages != null && !string.IsNullOrEmpty(user.Messages.ImageUrl))
                {
                    // Convertir ruta física a relativa (wwwroot/uploads/...)
                    var relativePath = Path.GetRelativePath(
                        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                        user.Messages.ImageUrl
                    );

                    // Reemplazar \ por / para que funcione en URL
                    var publicUrl = $"{baseUrl}/{relativePath.Replace("\\", "/")}";

                    // Asignar URL pública
                    user.Messages.ImageUrl = publicUrl;
                }
            }

            return users;
        }

    }
}
