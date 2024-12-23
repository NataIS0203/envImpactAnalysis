using Microsoft.Extensions.Caching.Memory;

namespace Durable.Utilities
{
    public static class ServiceUtilities
    {

        public static object GetCachedObject(IMemoryCache memoryCache, string cacheKey)
        {
            return memoryCache.TryGetValue(cacheKey, out object? cacheObject) ? cacheObject : null;
        }

        public static void SetCachedObject(IMemoryCache memoryCache, string cacheKey, object cacheObject, TimeSpan? cacheExpiration = null)
        {
            memoryCache.Set(cacheKey, cacheObject, cacheExpiration ?? TimeSpan.FromMinutes(15));
        }

    }
}
