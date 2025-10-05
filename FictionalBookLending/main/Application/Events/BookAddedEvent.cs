using FictionalBookLending.src.Domain.Abstractions;
using FictionalBookLending.src.Domain.ValueObjects.Book;

namespace FictionalBookLending.src.Application.Events
{
    public sealed record BookAddedEvent(Guid bookId, string title, string author, string isbn, DateTime time) : IDomainEvent
    {
        public string Message => $"A new book '{title}' by {author} (ISBN: {isbn}) was added to the catalog at {time:MMMM dd, yyyy}.";
        public string EventType => nameof(BookAddedEvent);
        public IDictionary<string, string> Attributes => new Dictionary<string, string>
        {
            ["BookId"] = bookId.ToString(),
            ["Title"] = title,
            ["Author"] = author,
            ["Isbn"] = isbn
        };
    }
}
