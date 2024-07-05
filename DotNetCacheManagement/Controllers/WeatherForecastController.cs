using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DotNetCacheManagement.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IMemoryCache _memoryCache;
        public WeatherForecastController(ILogger<WeatherForecastController> logger, IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        [ResponseCache(Duration = 10)]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }


        [HttpGet(Name = "GetWeatherForecastMemoryCaching")]
        public ActionResult<WeatherForecastsDto> GetWeatherForecastMemoryCaching()
        {
            //Cache Key
            const string cacheKey = "weatherResult";

            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<WeatherForecast> forecast))
            {
                forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                }).Take(2)
                .ToArray();

                var cacheOptions = new MemoryCacheEntryOptions()
                {
                    // Set the absolute expiration time for the cached data (20 seconds from now)
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20),
                    // Set the sliding expiration for the cached data (remove if not accessed for 10 seconds)
                    SlidingExpiration = TimeSpan.FromSeconds(10)
                };

                _memoryCache.Set(cacheKey, forecast, cacheOptions);
            }



            var weather2 = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).Take(2)
            .ToArray();

            var result = new WeatherForecastsDto
            {
                Weather1 = forecast,
                Weather2 = weather2
            };

            return Ok(result);
        }


    }
}
