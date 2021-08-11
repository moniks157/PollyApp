using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PollyApp.Services.Interfaces
{
    public interface IHolidaysService
    {
        Task<string> GetHolidaysRetry();
        Task<string> GetHolidaysCircutBreaker();
    }
}
