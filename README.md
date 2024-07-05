## ASP.NET Core Caching with Memory and Response Caching

This code demonstrates using both in-memory caching (`IMemoryCache`) and response caching in an ASP.NET Core application.

**Memory Caching**

* Stores data in the server's memory for faster retrieval.
* Suitable for data that changes infrequently and is expensive to generate.
* Ideal for single servers or server farms with session affinity (requests from a client always go to the same server).

**Response Caching**

* Instructs the client's browser to cache the response locally.
* Reduces server load and improves performance for subsequent requests.
* Useful for static content or content that doesn't change frequently.

**Code Breakdown**

```csharp
#region MemoryCache
// Adds the in-memory caching service to the application.
// Data can be stored and retrieved for faster access within the application.
builder.Services.AddMemoryCache();
#endregion

#region ResponseCaching
// Enables response caching for the entire application.
// instructs the middleware to check the cache for responses before processing the request.
app.UseResponseCaching();
#endregion

private readonly IMemoryCache _memoryCache;

public WeatherForecastController(ILogger<WeatherForecastController> logger, IMemoryCache memoryCache)
{
  _memoryCache = memoryCache;
  // Injects the IMemoryCache service for interacting with the cache through the constructor.
}

[HttpGet(Name = "GetWeatherForecast")]
[ResponseCache(Duration = 10)] // Cache response for 10 seconds
public IEnumerable<WeatherForecast> Get()
{
  // Generate weather forecast data
}

[HttpGet(Name = "GetWeatherForecastMemoryCaching")]
public ActionResult<WeatherForecastsDto> GetWeatherForecastMemoryCaching()
{
  const string cacheKey = "weatherResult";

  // Check if cached data exists using the cache key.
  if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<WeatherForecast> forecast))
  {
    // Generate weather forecast data and cache it for 20 seconds with a 10-second sliding expiration
    forecast = GenerateWeatherForecastData(2); // Get only 2 forecasts

    var cacheOptions = new MemoryCacheEntryOptions()
    {
      // Set the absolute expiration time for the cached data (20 seconds from now)
      AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20),
      // Set the sliding expiration for the cached data (remove if not accessed for 10 seconds)
      SlidingExpiration = TimeSpan.FromSeconds(10)
    };

    _memoryCache.Set(  // Use code with caution. This can potentially lead to memory exhaustion if not properly managed.
                        cacheKey, forecast, cacheOptions);
  }

  // Generate additional data and combine with cached data
  var weather2 = GenerateWeatherForecastData(5); // Get 5 forecasts (only 2 used)
  var result = new WeatherForecastsDto { Weather1 = forecast, Weather2 = weather2.Take(2) };

  return Ok(result);
}

private IEnumerable<WeatherForecast> GenerateWeatherForecastData(int count)
{
  // Logic to generate weather forecast data
}
