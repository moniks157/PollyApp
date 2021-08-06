using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using PollyApp.Presentation.Constants;
using PollyApp.Presentation.Policies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PollyApp.Presentation.Controllers
{
    [ApiController]
    [Route("holidays")]
    public class HolidayController : ControllerBase
    {
        private readonly ILogger<HolidayController> _logger;
        private readonly PolicyHolder _policyHolder;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _reauthorisePolicy;

        private string requestEndpoint;

        public HolidayController(ILogger<HolidayController> logger, PolicyHolder policyHolder)
        {
            _logger = logger;
            _policyHolder = policyHolder;
            _reauthorisePolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .RetryAsync(3, (httpResponseMessage, i) =>
                {
                    if (httpResponseMessage.Result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        requestEndpoint = SetUpHolidaysRequestEndpoint(Credentials.API_KEY);
                    }
                });
        }

        [HttpGet]
        public async Task<IActionResult> GetReauthorise()
        {
            var httpClient = GetHttpClient();
            requestEndpoint = SetUpHolidaysRequestEndpoint(Credentials.WRONG_API_KEY);
            var response = await _reauthorisePolicy.ExecuteAsync(() => httpClient.GetAsync(requestEndpoint));
            var result = await response.Content.ReadAsStringAsync();
            return Ok(result);
        }

        private HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(@"https://holidayapi.com/v1/");
            httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }

        private string SetUpHolidaysRequestEndpoint(string api_key)
        {
            var requestEndpoint = $"{HolidaysUriConstants.HOLIDAYS}{api_key}&{HolidaysUriConstants.COUNTRY_PL}&{HolidaysUriConstants.YEAR_2021}&pretty";

            return requestEndpoint;
        }
    }
}
