using SDO.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace SDO.Services
{
    public interface IDimRoleService
    {
        Task<RtnResultModel> Create(DimRoleMdfModel role);
        Task<RtnResultModel> Delete(string roleId);
        Task<IList<DimRoleModel>> Read();
        Task<DimRoleModel> ReadById(string roleId);
        Task<IList<DimRoleModel>> ReadByUser(string userId);
        Task<RtnResultModel> Update(DimRoleMdfModel role);
    }
}