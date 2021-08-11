using Polly;
using Polly.Retry;
using PollyApp.Policies.Interfaces;
using System;
using System.Net;
using System.Net.Http;

namespace PollyApp.Policies
{
    public class RetryPolicyMaker : IRetryPolicyMaker
    {
        public AsyncRetryPolicy<HttpResponseMessage> CreateRetryPolicy(Action authorise)
        {
            var result = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt), (httpResponseMessage, timeSpan, context) =>
                {
                    if (httpResponseMessage.Result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        authorise();
                    }
                });

            return result;
        }
    }
}
