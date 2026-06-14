
namespace JwtDemo.Services.Caching.Interfaces
{
    public interface IDistributedRedisCacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, 
        TimeSpan? absoluteExpiry = null, TimeSpan? slidingExpiry = null);
        Task RemoveAsync(string key);
    }
}