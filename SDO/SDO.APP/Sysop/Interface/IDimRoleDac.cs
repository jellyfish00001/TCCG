using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IDimRoleDac : IDac
    {
        Task Delete(string roleId) => null;
        Task DeleteMap(string roleId) => null;
        Task Insert(DimRoleMdfModel role) => null;
        Task InsertMap(IEnumerable<MapRoleRightModel> maps) => null;
        Task<IList<DimRoleModel>> Read() => null;
        Task<DimRoleModel> ReadById(string roleId, bool filteDelFlg, bool delFlg = false) => null;
        Task<IList<DimRoleModel>> ReadByUser(string userId) => null;
        Task<List<DimRoleModel>> ReadListByUser(string userId, string apId = "IPC3") => null;
        Task Update(DimRoleMdfModel role) => null;
    }
}