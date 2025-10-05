using Amazon.DynamoDBv2.DataModel;
using FictionalBookLending.src.Domain.Enum.Book;
using FictionalBookLending.src.Domain.ValueObjects.Book;
using System.Text.Json.Serialization;

namespace FictionalBookLending.src.Domain.Entities.Book
{
    [DynamoDBTable("tbl_Books")]
    public sealed class BookModel
    {
        // Partition Key
        [DynamoDBHashKey("id")]
        public Guid Id { get; init; } 

        [DynamoDBProperty("title")]
        public string Title { get; init; } = string.Empty;

        [DynamoDBProperty("author")]
        public string Author { get; init; } = string.Empty;


        [DynamoDBProperty("isbn")]
        public string ISBN { get; set; } = string.Empty;

        [DynamoDBProperty("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BookStatus Status { get; set; }

        [DynamoDBProperty("checkedOutAt")]
        public DateTime? CheckedOutAt { get; set; }

        [DynamoDBProperty("checkedOutBy")]
        public string? CheckedOutBy { get; set; }

        [DynamoDBProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DynamoDBProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
