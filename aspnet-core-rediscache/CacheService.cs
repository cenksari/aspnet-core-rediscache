﻿namespace AzureRedisCache
{
	using Microsoft.Extensions.Caching.Distributed;
	using System;
	using System.Text.Json;
	using System.Text.Json.Serialization;
	using System.Threading.Tasks;

	/// <summary>
	/// Cache service.
	/// </summary>
	public class CacheService : ICacheService
	{
		public readonly IDistributedCache Cache;

		public CacheService(IDistributedCache cache) => Cache = cache;

		private readonly JsonSerializerOptions options = new()
		{
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};

		/// <summary>
		/// Get cache value.
		/// </summary>
		/// <param name="key">Cache key</param>
		public async Task<T> GetCacheAsync<T>(string key) where T : class
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
		public async Task<bool> SetCacheAsync<T>(string key, T value, int minutes) where T : class
		{
			string response = JsonSerializer.Serialize(value, options);

			await Cache.SetStringAsync(key, response, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(minutes) });

			return true;
		}

		/// <summary>
		/// Remove cache value.
		/// </summary>
		/// <param name="key">Cache key</param>
		public async Task<bool> RemoveCacheAsync(string key)
		{
			await Cache.RemoveAsync(key);

			return true;
		}
	}
}