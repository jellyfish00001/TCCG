using SDO.Dac;
using SDO.Dac.Models;
using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface ISysParamDac : IDac
    {
        Task<SetParamModel> GetSysParam(string setItem, string setType);
        Task<SetParamItemModel> GetSysParamItem(string setItem);
        Task<IList<SetParamItemModel>> GetSysParamItems();
        Task<GridModel<SetParamItemModel>> GetSysParamItems(int skip, int take, string orderByField, string dir);
        Task<IList<SetParamModel>> GetSysParams();
        Task<IList<SetParamModel>> GetSysParams(string setItem);
        Task<IList<SetParamModel>> GetSysParams(int fromWhere = 0);
    }
}