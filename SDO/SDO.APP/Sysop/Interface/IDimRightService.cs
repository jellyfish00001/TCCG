using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IDimRightService
    {
        Task<RtnResultModel> Create(DimRightModel right);
        Task<RtnResultModel> Delete(string rightId);
        Task<IList<DimRightModel>> Read();
        Task<DimRightModel> ReadById(string rightId);
        Task<IList<DimRightModel>> ReadByRole(string roleId = "");
        Task<RtnResultModel> Update(DimRightModel right);
    }
}