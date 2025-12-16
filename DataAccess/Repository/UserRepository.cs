using Application.Abstractions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly CDABrisasDbContext _dbContext;
        public UserRepository(CDABrisasDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<User>> GetAllUsersWithMessages()
        {
            return await _dbContext.Users.Include(m => m.Messages).ThenInclude(s => s.PaymentStatus).ToListAsync();
        }

        public async Task<User> CreateUser(User user)
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<User> GetUser(string whatsappNumber)
        {
            return await _dbContext.Users.Where(u => u.Ws_Id == whatsappNumber).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<User>> GetAllUsersWithMessagesAgreement(List<int> messagesId)
        {
            return await _dbContext.Users
                .Where(u => u.Messages.Any(m => messagesId.Contains(m.Id)))
                .Include(u => u.Messages
                    .Where(m => messagesId.Contains(m.Id)))
                    .ThenInclude(m => m.PaymentStatus)
                .ToListAsync();
        }
    }
}
