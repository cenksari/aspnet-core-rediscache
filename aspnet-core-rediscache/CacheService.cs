namespace AzureRedisCache;

using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Service responsible for managing application caching, providing methods to store, retrieve,
/// and remove data in a distributed cache with support for automatic serialization and expiration.
/// 
/// Service must register as a singleton.
/// </summary>
/// <param name="distributedCache">The distributed cache instance used to store and retrieve cached values</param>
public class CacheService(IDistributedCache distributedCache) : ICacheService
{
	/// <summary>
	/// Gets or sets cache value.
	/// </summary>
	/// <typeparam name="T">Generic type</typeparam>
	/// <param name="mins">Minutes to keep cache</param>
	/// <param name="key">Cache key</param>
	/// <param name="factory">Cache generator function</param>
	/// <param name="cancellationToken">Cancellation token</param>
	public async Task<T?> GetOrCreateAsync<T>(
		int mins,
		string key,
		Func<CancellationToken, Task<T>> factory,
		CancellationToken cancellationToken
	)
	{
		// Validate parameters.
		ArgumentNullException.ThrowIfNull(key);

		ArgumentNullException.ThrowIfNull(factory);

		// Try to get cached value.
		if (await GetCacheAsync<T>(key, cancellationToken) is { } cached) return cached;

		// Generate data and set cache.
		T? value = await factory(cancellationToken);

		// Set cache only if data is not null.
		return value is null ? default : await SetCacheAsync(key, value, mins, cancellationToken);
	}

	/// <summary>
	/// Gets cache value.
	/// </summary>
	/// <typeparam name="T">Generic type</typeparam>
	/// <param name="key">Cache key</param>
	/// <param name="cancellationToken">Cancellation token</param>
	public async Task<T?> GetCacheAsync<T>(string key, CancellationToken cancellationToken)
	{
		// Validate parameters.
		ArgumentNullException.ThrowIfNull(key);

		// Get cached string value.
		string? cachedResponse = await distributedCache.GetStringAsync(key, cancellationToken);

		// Return deserialized cached value or default if not found.
		return string.IsNullOrWhiteSpace(cachedResponse) ? default : Deserialize<T>(cachedResponse);
	}

	/// <summary>
	/// Sets cache value.
	/// </summary>
	/// <typeparam name="T">Generic type</typeparam>
	/// <param name="key">Cache key</param>
	/// <param name="value">Cache value</param>
	/// <param name="mins">Minutes to keep cache</param>
	/// <param name="cancellationToken">Cancellation token</param>
	public async Task<T?> SetCacheAsync<T>(string key, T value, int mins, CancellationToken cancellationToken)
	{
		// Validate parameters.
		ArgumentNullException.ThrowIfNull(key);

		ArgumentNullException.ThrowIfNull(value);

		// Serialize value to JSON string.
		string serializedValue = Serialize(value);

		// Create cache entry options with sliding expiration.
		DistributedCacheEntryOptions cacheOptions = CreateCacheEntryOptions(mins);

		// Set cache value.
		await distributedCache.SetStringAsync(key, serializedValue, cacheOptions, cancellationToken);

		// Return the original value.
		return value;
	}

	/// <summary>
	/// Removes cache value.
	/// </summary>
	/// <param name="key">Cache key</param>
	/// <param name="cancellationToken">Cancellation token</param>
	public async Task RemoveCacheAsync(string key, CancellationToken cancellationToken) =>
		await distributedCache.RemoveAsync(key, cancellationToken);

	/// <summary>
	/// Serializes an object to JSON string.
	/// </summary>
	/// <typeparam name="T">Generic type</typeparam>
	/// <param name="value">Object to serialize</param>
	private string Serialize<T>(T value) => JsonSerializer.Serialize(value, SerializerOptions);

	/// <summary>
	/// Deserializes JSON string to object.
	/// </summary>
	/// <typeparam name="T">Generic type</typeparam>
	/// <param name="json">JSON string</param>
	private T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, SerializerOptions);

	/// <summary>
	/// Creates cache entry options with sliding expiration.
	/// </summary>
	/// <param name="minutes">Minutes to keep cache</param>
	private static DistributedCacheEntryOptions CreateCacheEntryOptions(int minutes) => new()
	{
		SlidingExpiration = TimeSpan.FromMinutes(minutes)
	};

	/// <summary>
	/// Creates json serializer options.
	/// </summary>
	private readonly JsonSerializerOptions SerializerOptions = new()
	{
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};
}