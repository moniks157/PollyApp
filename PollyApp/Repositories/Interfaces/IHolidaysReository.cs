using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PollyApp.Repositories.Interfaces
{
    public interface IHolidaysReository
    {
        Task<string> GetHolidaysRetry();
        Task<string> GetHolidaysCircutBreaker();
    }
}
