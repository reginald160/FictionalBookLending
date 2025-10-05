using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using FictionalBookLending.src.Domain.Entities;
using System.Text.Json;
using FictionalBookLending.src.Application.Abstractions;

namespace FictionalBookLending.src.Infrastructure.Persistence.Repository
{
    public sealed class IdempotencyRepository : IIdempotencyService
    {
        private readonly IAmazonDynamoDB _dynamo;
        private readonly DynamoDBContext _ctx;
        public IdempotencyRepository(IAmazonDynamoDB dynamo)
        {
            _dynamo = dynamo;
            _ctx = new DynamoDBContext(dynamo);
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken ct)
        {
            var item = await _ctx.LoadAsync<IdempotencyRecord>(key, ct);
            return item is not null;
        }

        public async Task SaveAsync(string key, object response, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(response);
            var record = new IdempotencyRecord { Key = key, Response = json };
            await _ctx.SaveAsync(record, ct);
        }

        public async Task<T?> GetResponseAsync<T>(string key, CancellationToken ct)
        {
            var item = await _ctx.LoadAsync<IdempotencyRecord>(key, ct);
            if (item?.Response is null) return default;
            return JsonSerializer.Deserialize<T>(item.Response);
        }
    }
}
