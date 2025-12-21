using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IApiLogDac : IDac
    {
        Task<IList<ApiTraceModel>> ReadByDate(GridBasicQryModel model);
    }
}