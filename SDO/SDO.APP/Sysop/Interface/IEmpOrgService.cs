using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IEmpOrgService
    {
        Task<RtnResultModel> CreateEmpOrg(EmpOrgModel model);

        Task<RtnResultModel> UpdateEmpOrg(EmpOrgModel model);

        Task<RtnResultModel> DeleteEmpOrg(string orgIds);

        Task<IList<EmpOrgModel>> ReadAllOrgs();

        Task<IList<EmpOrgModel>> GetOrgList();

        Task<EmpOrgModel> ReadChildOrgs(string orgId);

        Task<IList<EmpOrgModel>> ReadByParentOrg(string orgId);

        Task<EmpOrgModel> ReadByOrgId(string orgId);

        Task<IList<EmpOrgModel>> ReadByOrgIds(string[] orgIds);

        Task<EmpOrgModel> ReadByUserId(string userId);

        Task<EmpOrgInfoModel[]> GetAllOrgs();
    }
}
