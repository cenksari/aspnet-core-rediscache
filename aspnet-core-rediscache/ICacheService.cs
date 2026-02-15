namespace AzureRedisCache;

using System;
using System.Threading;
using System.Threading.Tasks;

public interface ICacheService
{
	Task RemoveCacheAsync(string key, CancellationToken cancellationToken);

	Task<T?> GetCacheAsync<T>(string key, CancellationToken cancellationToken);

	Task<T?> SetCacheAsync<T>(string key, T value, int mins, CancellationToken cancellationToken);

	Task<T?> GetOrCreateAsync<T>(int mins, string key, Func<CancellationToken, Task<T>> factory, CancellationToken cancellationToken);
}