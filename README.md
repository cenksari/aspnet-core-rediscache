# Azure Redis Cache helpers
Azure Redis Cache helpers for your dotnet core projects.

### How to install

1. Add `ICacheService.cs` and `CacheService.cs` to your project.
2. Install nuget package `Microsoft.Extensions.Caching.Redis` package.
3. Configure your `Startup.cs` file : Add connection string `services.AddDistributedRedisCache(options => options.Configuration = Configuration["RedisConnectionString"]);`
4. Configure your `Startup.cs` file : Add `services.AddSingleton<ICacheService, CacheService>();`

### How to use

**Example controller:**

```csharp
private readonly ICacheService CacheService;

public HomeController(ICacheService cacheService) => CacheService = cacheService;

public async Task<IActionResult> OrganizeCache()
{
  // Cache name
  const string cacheName = "TestKey";
  
  // Get cache value
  Test test = await CacheService.GetCacheAsync<Test>(cacheName);
  
  // Cache value is null
  if (test == null)
  {
    // Create new value
    Test testValue = new()
    {
      Id = 1,
      Name = "Cenk"
    };
  
    // Set new value
    test = await CacheService.SetCacheAsync(cacheName, testValue, 10);
  }
  
  ViewBag.Result = test;

  return View();
}
```

**Example model:**

```csharp
public record Test
{
  public int Id { get; init; }
  public string Name { get; init; }
}
```

**Removing cache key**

Simply call

```csharp
await Cache.RemoveCacheAsync("cacheName");
```
