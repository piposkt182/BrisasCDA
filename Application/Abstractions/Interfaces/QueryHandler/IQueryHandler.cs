using Application.Abstractions.Interfaces.Queries;

namespace Application.Abstractions.Interfaces.QueryHandler
{
    public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
    }
}
