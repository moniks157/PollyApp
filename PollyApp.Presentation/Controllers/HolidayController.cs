using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PollyApp.Presentation.Policies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollyApp.Presentation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HolidayController : ControllerBase
    {
        private readonly ILogger<HolidayController> _logger;
        private readonly PolicyHolder _policyHolder;

        public HolidayController(ILogger<HolidayController> logger, PolicyHolder policyHolder)
        {
            _logger = logger;
            _policyHolder = policyHolder;
        }

        [HttpGet]
        public async Task<IActionResult> GetReauthorise()
        {
            var httpClient = GetHttpClient();
            return Ok();
        }

        private HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(@"http://localhost:57696/api/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            return httpClient;
        }
    }
}
