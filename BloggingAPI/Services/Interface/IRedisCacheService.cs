namespace BloggingAPI.Services.Interface
{
    public interface IRedisCacheService
    {
        Task<T> GetCachedDataAsync<T>(string key);
        Task RemoveCacheAsync(string key);
        Task SetCachedDataAsync<T>(string key, T data, TimeSpan cacheDuration);
    }
}
