using FictionalBookLending.src.Application.Contracts;

namespace FictionalBookLending.src.Application.CQRS.Command.AddBook
{
    public sealed record AddBookCommand(string title, string author, string isbn) : BaseActivityTrace;
}
