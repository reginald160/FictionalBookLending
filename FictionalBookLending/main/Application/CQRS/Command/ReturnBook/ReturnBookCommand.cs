using FictionalBookLending.src.Application.Contracts;

namespace FictionalBookLending.src.Application.CQRS.Command.ReturnBook
{
    public sealed record ReturnBookCommand(Guid id) : BaseActivityTrace;

}
