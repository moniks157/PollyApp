using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PollyApp.Services.Interfaces;
using System.Threading.Tasks;

namespace PollyApp.Presentation.Controllers
{
    [ApiController]
    [Route("holidays")]
    public class HolidaysController : ControllerBase
    {
        private readonly ILogger<HolidaysController> _logger;
        private readonly IHolidaysService _holidaysService;
        public HolidaysController(ILogger<HolidaysController> logger, IHolidaysService holidaysService)
        {
            _logger = logger;
            _holidaysService = holidaysService;
        }

        [HttpGet("retry")]
        public async Task<IActionResult> GetReauthorise()
        {
            var result = await _holidaysService.GetHolidaysRetry();
            return Ok(result);
        }

        [HttpGet("circutBreaker")]
        public async Task<IActionResult> GetWaitAndRetry()
        {
            var result = await _holidaysService.GetHolidaysCircutBreaker();
            return Ok(result);
        }        
    }
}
