using Application.Abstractions;
using Application.Utilities.Enums;
using Domain.Dto;
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

        public async Task<Message> ApproveMessage(int id)
        {
            var message = await _dbContext.Messages.FirstOrDefaultAsync(m => m.Id == id);

            if (message is null)
                throw new KeyNotFoundException($"Message with id {id} was not found.");

            message.PaymentStatusId = (int)PaymentStatusId.Pending;

            await _dbContext.SaveChangesAsync();

            return message;
        }

        public async Task<IEnumerable<Message>> SetAllMessagesForAgreement(List<string> plates)
        {
            var normalizedPlates = plates.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.ToLower()).ToList();
            var affectedRows = await _dbContext.Messages
                .Where(m => normalizedPlates.Contains(m.PlateVehicle!.ToLower()) && m.PaymentStatusId == (int)PaymentStatusId.Pending)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(
                        m => m.PaymentStatusId,
                        _ => (int)PaymentStatusId.Review
                    )
                );

            if (affectedRows == 0)
            {
                return Enumerable.Empty<Message>();
            }

            return await _dbContext.Messages
                .Where(m =>
                    normalizedPlates.Contains(m.Text!.ToLower()) &&
                    m.PaymentStatusId == (int)PaymentStatusId.Review
                )
                .ToListAsync();
        }
        public async Task<PaidAgreementsResult> PaidAgreementsAsync(List<int> ids)
        {
            var messages = await _dbContext.Messages
                .Where(m => ids.Contains(m.Id))
                .ToListAsync();

            messages.ForEach(m => m.PaymentStatusId = (int)PaymentStatusId.Paid);

            await _dbContext.SaveChangesAsync();

            return new PaidAgreementsResult(
                Requested: ids.Count,
                Updated: messages.Count
            );
        }
    }
}
