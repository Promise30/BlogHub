using BloggingAPI.Services.Interface;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BloggingAPI.Services.Implementation
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDistributedCache? _cache;
        private readonly JsonSerializerOptions _jsonOptions;
        public RedisCacheService(IDistributedCache? cache)
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
        public T GetCachedData<T>(string key)
        {
            var jsonData = _cache.GetString(key);
            if(jsonData is null)
                return default(T);
            return JsonSerializer.Deserialize<T>(jsonData, _jsonOptions)!;
        }

        public void SetCachedData<T>(string key, T data, TimeSpan cacheDuration)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheDuration,
                SlidingExpiration = TimeSpan.FromMinutes(1)
            };
            // Use ReferenceHandler.Preserve to handle cycles

            var jsonDate = JsonSerializer.Serialize(data, _jsonOptions);
            _cache.SetString(key, jsonDate, options);    
            //var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, _jsonOptions));
            //_cache.SetAsync(key, bytes, options);
        }
        public void RemoveCache(string key) => _cache.Remove(key);
    }
}
