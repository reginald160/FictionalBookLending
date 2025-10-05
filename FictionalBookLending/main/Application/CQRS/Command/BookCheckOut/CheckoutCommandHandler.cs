using FictionalBookLending.main.Application.Abstractions;
using FictionalBookLending.src.Application.Abstractions.Events;
using FictionalBookLending.src.Application.Contracts;
using FictionalBookLending.src.Application.CQRS.Command.AddBook;
using FictionalBookLending.src.Application.Events;
using FictionalBookLending.src.Domain.Enum.Book;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FictionalBookLending.src.Application.CQRS.Command.BookCheckOut
{
    public class CheckoutCommandHandler : ICommandHandler<CheckOutBookCommand>
    {
        private readonly IBookRepository _repo;
        private readonly IEventPublisher _events;

        public CheckoutCommandHandler(IBookRepository repo, IEventPublisher events)
        {
            _repo = repo;
            _events = events;
        }

        public async Task<IResult> Handle(CheckOutBookCommand cmd, CancellationToken ct)
        {
            var traceId = cmd.TraceId;

            var book = await _repo.GetAsync(cmd.id, ct);

            if (book is null)
                return ApiResponse.Fail(new[] { "Book not found" }, "Checkout failed", traceId);

            if (!book.Status.Equals(BookStatus.Available))
                return ApiResponse.Fail(new[] { "Book already checked out" }, "Checkout failed", traceId);

            book.Status = BookStatus.CheckedOut;
            var ev = new BookAddedEvent(book.Id, book.Title, book.Author, book.ISBN, DateTime.Now);
            await _repo.SaveAsync(book, ct);
            await _events.PublishAsync(new[] { ev }, ct);

            return ApiResponse.Ok("Book checked out successfully", cmd.TraceId);
        }

  

    }
}
