

namespace FictionalBookLending.src.Application.CQRS.Query
{
    public interface IQueryHandler<TQuery>
    {
        Task<IResult> Handle(TQuery ? query, CancellationToken ct);
    }
}
