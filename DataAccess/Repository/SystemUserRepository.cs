using Application.Abstractions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class SystemUserRepository : ISystemUserRepository
    {
        private readonly CDABrisasDbContext _dbContext;

        public SystemUserRepository(CDABrisasDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SystemUser> CreateSystemUser(SystemUser user)
        {
            _dbContext.SystemUsers.Add(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<SystemUser> GetUserByCredencials(string userName, string password)
        {
          return await _dbContext.SystemUsers.FirstOrDefaultAsync(u => u.UserName == userName && u.PasswordHash == password);
        }
    }
}
