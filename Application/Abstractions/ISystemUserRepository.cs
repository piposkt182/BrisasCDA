using Domain.Models;

namespace Application.Abstractions
{
    public interface ISystemUserRepository
    {
        Task<SystemUser> CreateSystemUser(SystemUser user);
        Task<SystemUser> GetUserByCredencials(string userName, string password);
    }
}
