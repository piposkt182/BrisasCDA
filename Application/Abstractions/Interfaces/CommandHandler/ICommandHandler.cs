using Application.Abstractions.Interfaces.Commands;

namespace Application.Abstractions.Interfaces.CommandHandler
{
    public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
    {
        Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    }
}
