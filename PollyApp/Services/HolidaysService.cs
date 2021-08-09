using PollyApp.Repositories.Interfaces;
using PollyApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PollyApp.Services
{
    public class HolidaysService : IHolidaysService
    {
        private readonly IHolidaysReository _holidaysRepository;

        public HolidaysService(IHolidaysReository holidaysReository)
        {
            _holidaysRepository = holidaysReository;
        }

        public async Task<string> GetHolidaysCircutBreaker()
        {
            var result = await _holidaysRepository.GetHolidaysCircutBreaker();
            return result;
        }

        public async Task<string> GetHolidaysRetry()
        {
            var result = await _holidaysRepository.GetHolidaysRetry();
            return result;
        }
    }
}
