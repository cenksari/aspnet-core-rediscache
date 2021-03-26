namespace AzureRedisCache
{
	using System.Threading.Tasks;

	public interface ICacheService
	{
		Task<bool> RemoveCacheAsync(string key);

		Task<T> GetCacheAsync<T>(string key) where T : class;

		Task<bool> SetCacheAsync<T>(string key, T value, int minutes) where T : class;
	}
}