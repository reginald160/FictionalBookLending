using FictionalBookLending.src.Domain.Enum.Book;
using FictionalBookLending.src.Domain.ValueObjects.Book;

namespace FictionalBookLending.src.Application.DTOs
{
    public class GetAllBooksDTO
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; } 
        public string? ISBN { get; set; }

        public string? Status { get; set; }
    }
}
