using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Playground.Shared.Services;
using Playground.Shared.Models;

namespace client_sandbox.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly WeatherForecastService forecastService = new WeatherForecastService();

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return forecastService.GetForecasts();
        }

        [HttpGet("{count}")]
        public IEnumerable<WeatherForecast> Get(int count)
        {
            return forecastService.GetForecasts(count);
        }
    }
}
