
using FictionalBookLending.src.Application.Abstractions.Events;
using FictionalBookLending.src.Application.Contracts;
using FictionalBookLending.src.Application.CQRS.Command.BookCheckOut;
using FictionalBookLending.src.Application.CQRS.Command;
using FictionalBookLending.src.Application.Events;
using FictionalBookLending.src.Domain.Enum.Book;
using FictionalBookLending.src.Application.CQRS.Command.ReturnBook;
using Microsoft.Extensions.Logging;
using static FictionalBookLending.src.Application.CQRS.Query.ReturnBookCommandHandler;
using FictionalBookLending.main.Application.Abstractions;

namespace FictionalBookLending.src.Application.CQRS.Query
{
    public sealed class ReturnBookCommandHandler : ICommandHandler<ReturnBookCommand>
    {
        private readonly IBookRepository _repo;
        private readonly IEventPublisher _events;

        public ReturnBookCommandHandler(IBookRepository repo, IEventPublisher events)
        {
            _repo=repo;
            _events=events;
        }

        public async Task<IResult> Handle(ReturnBookCommand command, CancellationToken ct)
        {
            var traceId = command.TraceId;
            var book = await _repo.GetAsync(command.id, ct);

            if (book is null) return ApiResponse.Fail(new[] { "Book not found" }, "opertaion failed", traceId);
            if (book.Status != BookStatus.CheckedOut) return ApiResponse.Fail(new[] { "Book not available for return" }, "opertaion failed", traceId);

            book.Status = BookStatus.Available;
            var ev = new BookReturnedEvent(book.Id, "login user", DateTime.Now);
            await _repo.SaveAsync(book, ct);
            await _events.PublishAsync(new[] { ev }, ct);
            return ApiResponse.Ok("Book returns successfully", traceId);
        }
    }
}
