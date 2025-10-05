using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using FictionalBookLending.src.Domain.Enum.Book;
using FictionalBookLending.src.Domain.ValueObjects.Book;
using FictionalBookLending.src.Domain.Entities.Book;
using Amazon.DynamoDBv2.DataModel;
using FictionalBookLending.src.Infrastructure.Persistence.Cache;
using Amazon.Runtime.Internal.Util;
using FictionalBookLending.src.Application.Contracts;
using FictionalBookLending.main.Application.Abstractions;

namespace FictionalBookLending.src.Infrastructure.Persistence.Repository
{
    public sealed class BookRepository : IBookRepository
    {
        private readonly IAmazonDynamoDB _db;
        private readonly DynamoDBContext _ctx;
        private const string TableName = "tbl_Books";

        public BookRepository(IAmazonDynamoDB db)
        {
            _db = db; _ctx = new DynamoDBContext(db);
     
        }

        public async Task SaveAsync(BookModel book, CancellationToken ct) =>
            await _ctx.SaveAsync(book, ct);


        public async Task<BookModel> GetAsync(Guid id, CancellationToken ct) => 
            await _ctx.LoadAsync<BookModel>(id, ct);

        //public async Task<BookModel> GetByISBNAsync(Guid id, CancellationToken ct) =>
        //    await _ctx.LoadAsync<BookModel>(id, ct);

        public async Task<IReadOnlyList<BookModel>> ListAsync(CancellationToken ct)
        {
            
            var conditions = new List<ScanCondition>();
            var books =  await _ctx.ScanAsync<BookModel>(conditions).GetRemainingAsync(ct);
            return books;
 
        }
    }

}
