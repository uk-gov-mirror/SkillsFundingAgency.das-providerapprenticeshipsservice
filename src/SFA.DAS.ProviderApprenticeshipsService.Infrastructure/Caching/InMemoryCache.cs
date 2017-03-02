﻿using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces;

namespace SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Caching
{
    public class InMemoryCache : ICache
    {
        private readonly ICurrentDateTime _currentDateTime;

        public InMemoryCache(ICurrentDateTime currentDateTime)
        {
            if (currentDateTime == null)
                throw new ArgumentNullException(nameof(currentDateTime));

            _currentDateTime = currentDateTime;
        }

        public Task<bool> ExistsAsync(string key)
        {
            var value = MemoryCache.Default.Get(key);

            return Task.FromResult(value != null);
        }

        public Task<T> GetCustomValueAsync<T>(string key)
        {
            return Task.FromResult((T)MemoryCache.Default.Get(key));
        }

        public Task SetCustomValueAsync<T>(string key, T customType, int secondsInCache = 300)
        {
            MemoryCache.Default.Set(key, customType, new CacheItemPolicy { AbsoluteExpiration = new DateTimeOffset(_currentDateTime.Now.AddSeconds(secondsInCache)) });

            return Task.FromResult<object>(null);
        }
    }
}