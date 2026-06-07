
using JwtDemo.Services.Caching.Interfaces;
using StackExchange.Redis;

namespace JwtDemo.Services.Caching.Implimentations
{
    public class RedisCacheService : IRedisCacheService
    {
        IDatabase _cache;
        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _cache = redis.GetDatabase();
        }
        public async Task<bool> IsRateLimited(string key, int limit, TimeSpan window)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"Key cannot be null or empty space{nameof(key)}");
            }

            var attempt = await _cache.StringIncrementAsync(key);

            if(attempt == 1)
            {
                await _cache.KeyExpireAsync(key, window);
            }

            if(attempt > limit)
            {
                return true;
            }

        return false;
        }
    }
}