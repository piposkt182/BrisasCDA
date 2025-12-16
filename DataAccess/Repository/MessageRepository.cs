using Application.Abstractions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

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

        public async Task<IEnumerable<Message>> SetAllMessagesForAgreement(List<string> plates)
        {
            var normalizedPlates = plates
          .Where(p => !string.IsNullOrWhiteSpace(p))
          .Select(p => p.ToLower())
          .ToList();

            var affectedRows =  await _dbContext.Messages
                .Where(m => normalizedPlates.Contains(m.Text!.ToLower()) && m.PaymentStatusId == 1)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(
                        m => m.PaymentStatusId,
                        _ => 2
                    )
                );

            if (affectedRows == 0)
            {
                return Enumerable.Empty<Message>();
            }

            return await _dbContext.Messages
                .Where(m =>
                    normalizedPlates.Contains(m.Text!.ToLower()) &&
                    m.PaymentStatusId == 2
                )
                .ToListAsync();
        }
    }
}
