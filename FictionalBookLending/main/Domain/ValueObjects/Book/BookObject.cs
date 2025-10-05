using Amazon.DynamoDBv2.DataModel;
using FictionalBookLending.src.Domain.Common;
using FictionalBookLending.src.Domain.Entities.Book;
using FictionalBookLending.src.Domain.Enum.Book;
using System.Text.Json.Serialization;

namespace FictionalBookLending.src.Domain.ValueObjects.Book
{
    public sealed class BookObject : Entity
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Author { get; init; } = string.Empty;
        public ISBN ISBN { get; init; }

        public BookStatus Status { get; private set; }

        public DateTime? CheckedOutAt { get; private set; }
        public string? CheckedOutBy { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public BookObject(Guid id, string title, string author, string isbn)
        {
            Id = id; 
            Title = title;
            Author = author; 
            ISBN = ISBN.Parse(isbn);
            Status = BookStatus.Available;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        public  BookModel NewBookItem()
        {
            if (string.IsNullOrWhiteSpace(Title)) throw new ArgumentException("Title required");
            if (string.IsNullOrWhiteSpace(Author)) throw new ArgumentException("Author required");

            return new BookModel
            {
                Id = Id,
                Title = Title.Trim(),
                Author = Author.Trim(),
                ISBN = ISBN.Parse(ISBN.Value).ToString(),
                Status = BookStatus.Available,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
 
        }
    }
}
