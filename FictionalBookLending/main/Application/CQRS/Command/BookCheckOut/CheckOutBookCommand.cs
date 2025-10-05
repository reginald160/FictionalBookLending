using FictionalBookLending.src.Application.Contracts;

namespace FictionalBookLending.src.Application.CQRS.Command.BookCheckOut
{
    public sealed record CheckOutBookCommand (Guid id) : BaseActivityTrace;


}
