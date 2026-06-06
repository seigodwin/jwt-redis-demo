
using System.Text.Json;
using JwtDemo.Services.Caching.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace JwtDemo.Services.Caching.Implimentations
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDistributedCache _redisCacheService;
        public RedisCacheService(IDistributedCache redisCacheService)
        {
            _redisCacheService = redisCacheService;
        }
        public async Task<T?> GetAsync<T>(string key)
        {
            if(string.IsNullOrEmpty(key)) return default;

            var data = await _redisCacheService.GetStringAsync(key);
            
            if(string.IsNullOrEmpty(data))
            {
                return default;
            }
            try
            {
                return JsonSerializer.Deserialize<T>(data);
            }
            catch (JsonException)
            {
                await _redisCacheService.RemoveAsync(key);
                return default;
            }
        }

        public async Task RemoveAsync(string key)
        {
            if(string.IsNullOrEmpty(key)) return;
            await _redisCacheService.RemoveAsync(key);
        }

        public async Task SetAsync<T>(string key, T value, 
        TimeSpan? absoluteExpiry = null, TimeSpan? slidingExpiry = null)
        {
            if(value is null || string.IsNullOrEmpty(key)) return;

            var options = new DistributedCacheEntryOptions();
            if (absoluteExpiry.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = absoluteExpiry.Value;
            }

            if (slidingExpiry.HasValue)
            {
                options.SlidingExpiration = slidingExpiry;
            }
           
            var jsonData = JsonSerializer.Serialize(value);
            await _redisCacheService.SetStringAsync(key, jsonData, options);
    
        }
    }
}
