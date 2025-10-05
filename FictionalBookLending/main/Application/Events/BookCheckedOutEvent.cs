using FictionalBookLending.src.Domain.Abstractions;
using FictionalBookLending.src.Domain.ValueObjects.Book;

namespace FictionalBookLending.src.Application.Events
{
    public sealed record BookCheckedOutEvent(Guid bookId, string chekoutBy, DateTime checkedOutAt) : IDomainEvent
    {
        public string Message => $"{chekoutBy} checked out book with ID {bookId} on {checkedOutAt:MMMM dd, yyyy}.";

        public string EventType => nameof(BookCheckedOutEvent);
        public IDictionary<string, string> Attributes => new Dictionary<string, string>
        {
            ["BookId"] = bookId.ToString(),
            ["UserEmail"] = chekoutBy,
            ["CheckedOutAt"] = checkedOutAt.ToString("O") // ISO 8601
        };
    }
}
