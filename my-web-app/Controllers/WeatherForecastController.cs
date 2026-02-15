using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace my_web_app.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool",
            "Mild", "Warm", "Balmy", "Hot",
            "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        // Existing endpoint
        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            Console.WriteLine(DateTime.UtcNow + " this is pushpraj code");

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        // Existing endpoint
        [HttpGet]
        public string GetName(string name)
        {
            return "your name is new code" + name;
        }

        // ✅ NEW: CI/CD version visibility endpoint
        [HttpGet]
        public string GetVersion()
        {
            var version = Environment.GetEnvironmentVariable("APP_VERSION") ?? "unknown";
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown";

            return $"AppVersion={version}, Environment={env}, TimeUTC={DateTime.UtcNow}";
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "healthy",
                timeUtc = DateTime.UtcNow
            });
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult UnHealth()
        {
            throw new NotImplementedException();
            return Ok(new
            {
                status = "healthy",
                timeUtc = DateTime.UtcNow
            });
        }
    }
}
