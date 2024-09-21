using BloggingAPI.Services.Interface;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BloggingAPI.Services.Implementation
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDistributedCache _cache;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
            _jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true,
                AllowTrailingCommas = true,
                PropertyNamingPolicy = null,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<T> GetCachedDataAsync<T>(string key)
        {
            var jsonData = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(jsonData))
                return default(T);
            return JsonSerializer.Deserialize<T>(jsonData, _jsonOptions);
        }

        public async Task SetCachedDataAsync<T>(string key, T data, TimeSpan cacheDuration)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheDuration,
                SlidingExpiration = TimeSpan.FromMinutes(1)
            };
            var jsonData = JsonSerializer.Serialize(data, _jsonOptions);
            await _cache.SetStringAsync(key, jsonData, options);
        }

        public async Task RemoveCacheAsync(string key) => await _cache.RemoveAsync(key);
    }
}
