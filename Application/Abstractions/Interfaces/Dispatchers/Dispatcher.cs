using Application.Abstractions.Interfaces.CommandHandler;
using Application.Abstractions.Interfaces.Commands;
using Application.Abstractions.Interfaces.Dispatchers.Interfaz;
using Application.Abstractions.Interfaces.Queries;
using Application.Abstractions.Interfaces.QueryHandler;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Abstractions.Interfaces.Dispatchers
{
    public class Dispatcher : IDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public Dispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResult> SendCommandAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : ICommand<TResult>
        {
            var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
            return await handler.HandleAsync(command, cancellationToken);
        }

        public async Task<TResult> SendQueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
            where TQuery : IQuery<TResult>
        {
            var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
            return await handler.HandleAsync(query, cancellationToken);
        }
    }

}
