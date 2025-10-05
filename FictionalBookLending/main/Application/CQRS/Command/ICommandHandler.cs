using FictionalBookLending.src.Application.CQRS.Command.AddBook;

namespace FictionalBookLending.src.Application.CQRS.Command
{
    public interface ICommandHandler<TCommand>
    {
        Task<IResult> Handle(TCommand command, CancellationToken ct);
    }
}
