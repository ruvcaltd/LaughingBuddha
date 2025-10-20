using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LAF.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        [ProducesResponseType(typeof(IEnumerable<WeatherForecast>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IEnumerable<WeatherForecast> Get()
        {
            // Debug logging
            _logger.LogInformation("Authorization header: {AuthHeader}",
                Request.Headers.Authorization.ToString());

            _logger.LogInformation("User Identity Name: {Name}",
                User.Identity?.Name ?? "null");

            _logger.LogInformation("User Identity IsAuthenticated: {IsAuthenticated}",
                User.Identity?.IsAuthenticated ?? false);

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            _logger.LogInformation($"Weather forecast requested by user: {userEmail}");

            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            _logger.LogInformation("All claims: {@Claims}", claims);

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
