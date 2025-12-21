using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface ISqlLogDac : IDac
    {
        Task<IList<SqlTraceGridModel>> Read(GridBasicQryModel model);
    }
}