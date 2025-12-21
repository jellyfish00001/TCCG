using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IDimRightDac : IDac
    {
        Task Delete(string rightId);
        Task DeleteMap(string rightId);
        Task Insert(DimRightModel right);
        Task InsertMap(IEnumerable<MapRightFunctionModel> maps);
        Task<IList<DimRightModel>> Read();
        Task<DimRightModel> ReadById(string rightId, bool delFlg);
        Task<IList<DimRightModel>> ReadByRole(string roleId);
        Task Update(DimRightModel right);
    }
}