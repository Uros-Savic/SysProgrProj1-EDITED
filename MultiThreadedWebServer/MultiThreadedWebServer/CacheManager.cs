using System;
using System.Runtime.Caching;

namespace MultiThreadedWebServer
{
    internal class CacheManager
    {
        private static readonly ObjectCache Cache = MemoryCache.Default;

        public static byte[] Get(string key)
        {
            return Cache[key] as byte[];
        }

        public static void Set(string key, byte[] data, double cacheDurationInSeconds)
        {
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(cacheDurationInSeconds),
                RemovedCallback = CacheItemRemovedCallback
            };
            Cache.Set(key, data, policy);
        }

        public static bool Contains(string key)
        {
            return Cache.Contains(key);
        }

        private static void CacheItemRemovedCallback(CacheEntryRemovedArguments arguments)
        {
            if (arguments.RemovedReason == CacheEntryRemovedReason.Expired)
            {
                Console.WriteLine($"Cache item '{arguments.CacheItem.Key}' has expired.");
            }
        }
    }
}
