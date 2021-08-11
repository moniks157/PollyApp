using Microsoft.Extensions.Options;
using Polly.Retry;
using PollyApp.Constants;
using PollyApp.Policies.Interfaces;
using PollyApp.Repositories.Interfaces;
using PollyApp.Services.Interfaces;
using PollyApp.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PollyApp.Services
{
    public class HolidaysService : IHolidaysService
    {
        private readonly IRetryPolicyMaker _retryPolicyMaker;
        private readonly IHolidaysReository _holidaysRepository;
        private readonly ApiAuthorisationSettings _apiAuthorisationSettings;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        private HttpClient httpClient;
        private string requestEndpoint;

        public HolidaysService(IHolidaysReository holidaysReository, IRetryPolicyMaker retryPolicyMaker, IOptions<ApiAuthorisationSettings> apiAuthorisationSettings)
        {
            _holidaysRepository = holidaysReository;
            _retryPolicyMaker = retryPolicyMaker;
            _apiAuthorisationSettings = apiAuthorisationSettings.Value;
            httpClient = GetHttpClient();
            _retryPolicy = _retryPolicyMaker.CreateRetryPolicy(() => CreateEndpoint(_apiAuthorisationSettings.HolidaysApiKey));
        }

        public async Task<string> GetHolidaysCircutBreaker()
        {
            var result = await _holidaysRepository.GetHolidaysCircutBreaker();
            return result;
        }

        public async Task<string> GetHolidaysRetry()
        {
            CreateEndpoint(_apiAuthorisationSettings.WrongApiKey);
            var response = await _retryPolicy.ExecuteAsync(() => httpClient.GetAsync(requestEndpoint));
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        private HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(@"https://holidayapi.com/v1/");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }

        private void CreateEndpoint(string apiKey)
        {
            requestEndpoint = $"{HolidaysUriConstants.HOLIDAYS}?{apiKey}&{HolidaysUriConstants.COUNTRY_PL}&{HolidaysUriConstants.YEAR_2021}&{HolidaysUriConstants.PRETTY}";
        }
    }
}
