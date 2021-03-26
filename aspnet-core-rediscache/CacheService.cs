namespace AzureRedisCache
{
	using Microsoft.Extensions.Caching.Distributed;
	using System;
	using System.Text.Json;
	using System.Threading.Tasks;

	/// <summary>
	/// Cache service.
	/// </summary>
	public class CacheService : ICacheService
	{
		public readonly IDistributedCache Cache;

		public CacheService(IDistributedCache cache) => Cache = cache;

		/// <summary>
		/// Get cache value.
		/// </summary>
		/// <param name="key">Cache key</param>
		public async Task<T> GetCache<T>(string key) where T : class
		{
			string cachedResponse = await Cache.GetStringAsync(key);

			return cachedResponse == null ? null : JsonSerializer.Deserialize<T>(cachedResponse);
		}

		/// <summary>
		/// Set cache value.
		/// </summary>
		/// <param name="key">Cache key</param>
		/// <param name="value">Cache value</param>
		/// <param name="minutes">Minutes to keep</param>
		public async Task<bool> SetCache<T>(string key, T value, int minutes) where T : class
		{
			string response = JsonSerializer.Serialize(value);

			await Cache.SetStringAsync(key, response, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(minutes) });

			return true;
		}

		/// <summary>
		/// Remove cache value.
		/// </summary>
		/// <param name="key">Cache key</param>
		public async Task<bool> RemoveCache(string key)
		{
			await Cache.RemoveAsync(key);

			return true;
		}
	}
}