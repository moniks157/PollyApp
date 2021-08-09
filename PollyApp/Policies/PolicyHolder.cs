using Polly;
using Polly.CircuitBreaker;
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
        public readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
        
        public PolicyHolder()
        {
            _circuitBreakerPolicy = Policy
               .Handle<Exception>()
               .CircuitBreakerAsync(1, TimeSpan.FromSeconds(30),
               onBreak: (ex, t) =>
               {
                   Console.WriteLine("Circut Broken!");
                   Console.WriteLine($"Circuit State: {_circuitBreakerPolicy.CircuitState}");
               },
               onReset: () =>
               {
                   Console.WriteLine("Circut Reset!");
               });
        }
    }
}
