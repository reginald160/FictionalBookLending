using FictionalBookLending.src.Domain.Entities.Book;

namespace FictionalBookLending.main.Application.Abstractions
{
    public interface IBookRepository
    {
        Task<BookModel> GetAsync(Guid id, CancellationToken ct);
        Task<IReadOnlyList<BookModel>> ListAsync(CancellationToken ct);
        Task SaveAsync(BookModel book, CancellationToken ct);
    }
}
