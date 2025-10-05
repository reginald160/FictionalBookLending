using Amazon.DynamoDBv2.DataModel;
using FictionalBookLending.src.Domain.ValueObjects.Book;
using System.ComponentModel.DataAnnotations;

namespace FictionalBookLending.src.Application.DTOs
{
    public class AddBookDTO
    {

        [Required]
        public string Title { get; set; }

        [Required]
        public string Author { get; init; }

        [Required]
        public string ISBN { get; init; }
    }
}
