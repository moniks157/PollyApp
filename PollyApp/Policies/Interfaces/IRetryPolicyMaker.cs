using Polly.Retry;
using System;
using System.Net.Http;

namespace PollyApp.Policies.Interfaces
{
    public interface IRetryPolicyMaker
    {
        AsyncRetryPolicy<HttpResponseMessage> CreateRetryPolicy(Action authorise);
    }
}
