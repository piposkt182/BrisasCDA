using Application.Abstractions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class MessageRepository : IMessageRepository
    {
        private readonly CDABrisasDbContext _dbContext;
        public MessageRepository(CDABrisasDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Message> CreateMessage(Message message)
        {
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();
            return message;
        }

        public async Task<IEnumerable<Message>> GetAllMessages()
        {
            return await _dbContext.Messages.Include(p => p.PaymentStatus).ToListAsync();
        }
    }
}
