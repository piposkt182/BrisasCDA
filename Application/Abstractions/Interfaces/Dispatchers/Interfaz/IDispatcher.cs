using Application.Abstractions.Interfaces.Commands;
using Application.Abstractions.Interfaces.Queries;

namespace Application.Abstractions.Interfaces.Dispatchers.Interfaz
{
    public interface IDispatcher
    {
        Task<TResult> SendCommandAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : ICommand<TResult>;

        Task<TResult> SendQueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
            where TQuery : IQuery<TResult>;
    }
}
