﻿using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using PollyApp.Constants;
using PollyApp.Policies;
using PollyApp.Repositories.Interfaces;
using PollyApp.Settings;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PollyApp.Repositories
{
    public class HolidaysRepository : IHolidaysReository
    {
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly CircuitBreakerPolicyHolder _policyHolder;
        private readonly ApiAuthorisationSettings _apiAuthorisationSettings;
        private HttpClient httpClient;
        private string requestEndpoint;

        private static int attempt = 0;
        public HolidaysRepository(CircuitBreakerPolicyHolder policyHolder, IOptions<ApiAuthorisationSettings> apiAuthorisationSettings)
        {
            httpClient = GetHttpClient();
            _apiAuthorisationSettings = apiAuthorisationSettings.Value;
            _policyHolder = policyHolder;

            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt),(httpResponseMessage, timeSpan, context) =>
                {
                    if (httpResponseMessage.Result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        requestEndpoint = HolidaysRequestEnpoint(_apiAuthorisationSettings.HolidaysApiKey);
                    }
                });
        }
        
        public async Task<string> GetHolidaysRetry()
        {
            requestEndpoint = HolidaysRequestEnpoint(_apiAuthorisationSettings.HolidaysApiKey);
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
                requestEndpoint = HolidaysRequestEnpoint(_apiAuthorisationSettings.HolidaysApiKey);
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
