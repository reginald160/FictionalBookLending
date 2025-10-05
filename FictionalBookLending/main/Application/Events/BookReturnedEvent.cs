using FictionalBookLending.src.Domain.Abstractions;

namespace FictionalBookLending.src.Application.Events
{
    public sealed record BookReturnedEvent(Guid bookId, string returnedBy, DateTime returnedAt) : IDomainEvent
    {
        public string Message => $"{returnedBy} returned book with ID {bookId} on {returnedAt:MMMM dd, yyyy}.";
        public string EventType => nameof(BookReturnedEvent);
        public IDictionary<string, string> Attributes => new Dictionary<string, string>
        {
            ["BookId"] = bookId.ToString(),
            ["UserEmail"] = returnedBy,
            ["ReturedOutAt"] = returnedAt.ToString("O") // ISO 8601
        };
    }
}
