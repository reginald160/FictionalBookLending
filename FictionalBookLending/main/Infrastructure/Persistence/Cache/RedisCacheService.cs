using StackExchange.Redis;
using System.Text.Json;

namespace FictionalBookLending.src.Infrastructure.Persistence.Cache
{
    public sealed class RedisCacheService : ICacheService, IDisposable
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = redis.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty)
                return default;
            return JsonSerializer.Deserialize<T>(value!);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
        {
            var serialized = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, serialized, expiry);
        }

        public async Task RemoveAsync(string key, CancellationToken ct = default)
        {
            await _db.KeyDeleteAsync(key);
        }

        public void Dispose() => _redis?.Dispose();
    }
}
