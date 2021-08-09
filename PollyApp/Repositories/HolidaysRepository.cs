using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using PollyApp.Constants;
using PollyApp.Presentation.Policies;
using PollyApp.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PollyApp.Repositories
{
    public class HolidaysRepository : IHolidaysReository
    {
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly PolicyHolder _policyHolder;
        private HttpClient httpClient;
        private string requestEndpoint;

        private static int attempt = 0;
        public HolidaysRepository(PolicyHolder policyHolder)
        {
            httpClient = GetHttpClient();
            _policyHolder = policyHolder;

            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt),(httpResponseMessage, timeSpan, context) =>
                {
                    if (httpResponseMessage.Result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        requestEndpoint = HolidaysRequestEnpoint(Credentials.API_KEY);
                    }
                });
        }
        
        public async Task<string> GetHolidaysRetry()
        {
            requestEndpoint = HolidaysRequestEnpoint(Credentials.WRONG_API_KEY);
            var response = await _retryPolicy.ExecuteAsync(() => httpClient.GetAsync(requestEndpoint));
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public async Task<string> GetHolidaysCircutBreaker()
        {
            try
            {
                Console.WriteLine($"Circuit State: {_policyHolder._circuitBreakerPolicy.CircuitState}");
                var result = await _policyHolder._circuitBreakerPolicy.ExecuteAsync(() => GetHolidays());
                return result;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }

        private HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(@"https://holidayapi.com/v1/");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
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
                requestEndpoint = HolidaysRequestEnpoint(Credentials.API_KEY);
                var response = await httpClient.GetAsync(requestEndpoint);
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
        }

        private string HolidaysRequestEnpoint(string apiKey)
        {
            var requestEndpoint = $"{HolidaysUriConstants.HOLIDAYS}{apiKey}&{HolidaysUriConstants.COUNTRY_PL}&{HolidaysUriConstants.YEAR_2021}&pretty";

            return requestEndpoint;
        }
    }
}
