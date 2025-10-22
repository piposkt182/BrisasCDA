using Application.Abstractions;
using Application.Abstractions.Interfaces.QueryHandler;
using Application.Messages.Queries;
using Domain.Models;

namespace Application.Messages.QueryHandler
{
    public class GetAllMessagesHandler : IQueryHandler<GetAllMessagesQuery, IEnumerable<Message>>
    {
        private readonly IMessageRepository _repository;

        public GetAllMessagesHandler(IMessageRepository repository)
        {
            _repository = repository;
        }
        public async Task<IEnumerable<Message>> HandleAsync(GetAllMessagesQuery query, CancellationToken cancellationToken = default)
        {
            return await _repository.GetAllMessages();
        }
    }
}
