using Amazon.Runtime.Internal.Util;
using Amazon.Runtime.Telemetry.Tracing;
using FictionalBookLending.main.Application.Abstractions;
using FictionalBookLending.src.Application.Abstractions.Events;
using FictionalBookLending.src.Application.Contracts;
using FictionalBookLending.src.Application.DTOs;
using FictionalBookLending.src.Application.Events;
using FictionalBookLending.src.Domain.Entities.Book;
using FictionalBookLending.src.Domain.ValueObjects.Book;
using FictionalBookLending.src.Infrastructure.Persistence.Cache;
using System.Diagnostics;

namespace FictionalBookLending.src.Application.CQRS.Command.AddBook
{

    public class AddBookCommadHandler : ICommandHandler<AddBookCommand>
    {
        private readonly IBookRepository repo;
        private readonly IEventPublisher events;
        private readonly IConfiguration _config;
        private readonly ICacheService _cache;

        public AddBookCommadHandler(IBookRepository repo, IEventPublisher events, IConfiguration config, ICacheService cache)
        {
            this.repo=repo;
            this.events=events;
            _config=config;
            _cache=cache;
        }

        public async Task<IResult> Handle(AddBookCommand cmd, CancellationToken ct)
        {
            var id = Guid.NewGuid();
            var bookObj = new BookObject(id, cmd.isbn, cmd.author, cmd.isbn); ;

            var entity = bookObj.NewBookItem();
            var ev = new BookAddedEvent(id, cmd.title, cmd.author, cmd.isbn, DateTime.Now);
            await repo.SaveAsync(entity, ct);
            await events.PublishAsync(new[] { ev }, ct);
            var CacheKey = _config.GetValue<string>("Redis:AllBook");

            await _cache.RemoveAsync(CacheKey, ct);

            return ApiResponse.Ok(null, "New Book added successfully", cmd.TraceId);
        }

    }
}
