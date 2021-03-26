namespace AzureRedisCache
{
	using System.Threading.Tasks;

	public interface ICacheService
	{
		Task<bool> RemoveCache(string key);

		Task<T> GetCache<T>(string key) where T : class;

		Task<bool> SetCache<T>(string key, T value, int minutes) where T : class;
	}
}