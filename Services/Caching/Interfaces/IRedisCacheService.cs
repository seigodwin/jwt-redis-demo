
namespace JwtDemo.Services.Caching.Interfaces
{
    public interface IRedisCacheService
    {
        Task<bool> IsRateLimited(string key, int limit, TimeSpan window);
    }
}