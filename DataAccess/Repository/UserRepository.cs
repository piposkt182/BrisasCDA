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
            return await _dbContext.Users.AsNoTracking()
                           .Select(u => new User
                           {
                               Id = u.Id,
                               Name = u.Name,
                               Ws_Id = u.Ws_Id,
                               Messages = u.Messages
                                   .OrderByDescending(m => m.DateCreated)
                                   .Select(m => new Message
                                   {
                                       Id = m.Id,
                                       UserId = m.UserId,
                                       Number = m.Number,
                                       Text = m.Text,
                                       DateMessage = m.DateMessage,
                                       ImageUrl = m.ImageUrl,
                                       MimeType = m.MimeType,
                                       PaymentStatusId = m.PaymentStatusId,
                                       ImageName = m.ImageName,
                                       AgreementId = m.AgreementId,
                                       PlateVehicle = m.PlateVehicle,
                                       DateCreated = m.DateCreated,

                                       PaymentStatus = m.PaymentStatus
                                   })
                                   .ToList()
                           }).ToListAsync();
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
