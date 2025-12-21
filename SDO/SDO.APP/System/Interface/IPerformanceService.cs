using SDO.Models;
using System.Collections.Generic;

namespace SDO.Services
{
    public interface IPerformanceService
    {
        IList<PerformanceModel> GetPerformance();
    }
}