using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollyApp.Presentation.Policies
{
    public class PolicyHolder
    {
        public AsyncRetryPolicy<HttpResponseMessage> HttpRetryPolicy { get; private set; }

        public PolicyHolder()
        {
            HttpRetryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt));
        }
    }
}
