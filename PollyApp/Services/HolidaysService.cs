using Microsoft.Extensions.Options;
using Polly.Retry;
using PollyApp.Constants;
using PollyApp.Policies;
using PollyApp.Policies.Interfaces;
using PollyApp.Services.Interfaces;
using PollyApp.Settings;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PollyApp.Services
{
    public class HolidaysService : IHolidaysService
    {
        private readonly IRetryPolicyMaker _retryPolicyMaker;
        private readonly ApiAuthorisationSettings _apiAuthorisationSettings;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly CircuitBreakerPolicyHolder _policyHolder;

        private HttpClient httpClient;
        private string requestEndpoint;

        private static int attempt = 0;

        public HolidaysService(IRetryPolicyMaker retryPolicyMaker, IOptions<ApiAuthorisationSettings> apiAuthorisationSettings, CircuitBreakerPolicyHolder policyHolder)
        {
            _retryPolicyMaker = retryPolicyMaker;
            _apiAuthorisationSettings = apiAuthorisationSettings.Value;
            httpClient = GetHttpClient();
            _policyHolder = policyHolder;
            _retryPolicy = _retryPolicyMaker.CreateRetryPolicy(() => CreateEndpoint(_apiAuthorisationSettings.HolidaysApiKey));
        }

        public async Task<string> GetHolidaysCircutBreaker()
        {
            try
            {
                Console.WriteLine($"Circuit State: {_policyHolder._circuitBreakerPolicy.CircuitState}");
                var result = await _policyHolder._circuitBreakerPolicy.ExecuteAsync(() => GetHolidays());
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> GetHolidaysRetry()
        {
            CreateEndpoint(_apiAuthorisationSettings.WrongApiKey);
            var response = await _retryPolicy.ExecuteAsync(() => httpClient.GetAsync(requestEndpoint));
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        private async Task<string> GetHolidays()
        {
            if (attempt % 2 == 0)
            {
                attempt++;
                throw new Exception();
            }
            else
            {
                attempt++;
                CreateEndpoint(_apiAuthorisationSettings.HolidaysApiKey);
                var response = await httpClient.GetAsync(requestEndpoint);
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
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
