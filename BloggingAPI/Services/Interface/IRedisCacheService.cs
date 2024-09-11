namespace BloggingAPI.Services.Interface
{
    public interface IRedisCacheService
    {
        T GetCachedData<T>(string key);
        void RemoveCache(string key);
        void SetCachedData<T>(string key, T data, TimeSpan cacheDuration);
    }
}
