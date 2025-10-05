
using Amazon.Runtime.Internal.Util;
using FictionalBookLending.main.Application.Abstractions;
using FictionalBookLending.src.Application.Abstractions.Events;
using FictionalBookLending.src.Application.Contracts;
using FictionalBookLending.src.Application.DTOs;
using FictionalBookLending.src.Application.Events;
using FictionalBookLending.src.Domain.Enum.Book;
using FictionalBookLending.src.Infrastructure.Persistence.Cache;
using System.Diagnostics;

namespace FictionalBookLending.src.Application.CQRS.Query.GetAllBook
{
    public sealed class GetAllBooksQueryHandler : IQueryHandler<BaseQuery>
    {
        private readonly IBookRepository repo;
        private readonly ICacheService _cache;
        private readonly IConfiguration _config;


        public GetAllBooksQueryHandler(IBookRepository repo, ICacheService cache, IConfiguration config)
        {
            this.repo=repo;
            _cache=cache;
            _config=config;
        }

        public async Task<IResult> Handle(BaseQuery query, CancellationToken ct)
        {
            var CacheKey = _config.GetValue<string>("Redis:AllBook");
            var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
            var cached = await _cache.GetAsync<IEnumerable<GetAllBooksDTO>>(CacheKey, ct);
            if (cached is not null)
            {
                return ApiResponse.Ok(cached, "Book returns successfully", traceId);
            }

            var result = await repo.ListAsync(ct);
            var books  = result.Select(x => new GetAllBooksDTO
            {
                Id = x.Id,
                Title = x.Title,
                Author = x.Author,
                ISBN = x.ISBN,
                Status = x.Status.ToString()
            });

            await _cache.SetAsync(CacheKey, books, TimeSpan.FromMinutes(5), ct);

            return ApiResponse.Ok(books, "Book returns successfully", traceId);

        }

    
    }
}
