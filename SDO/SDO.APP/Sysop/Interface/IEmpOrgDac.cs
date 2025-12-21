using SDO.Models;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IEmpOrgDac : IDac
    {
        Task Delete(IEnumerable orgs) => null;
        Task DeleteMap(IEnumerable<string> orgIds) => null;
        Task Insert(EmpOrgModel empOrg) => null;
        Task InsertMap(IEnumerable<MapOrgUserModel> mapOrgUserDbEditors) => null;
        Task<IList<EmpOrgModel>> ReadAllOrgs() => null;
        Task<EmpOrgModel> ReadByOrgId(string orgId) => null;
        Task<IList<EmpOrgModel>> ReadByOrgIds(string[] orgIds) => null;
        Task<IList<EmpOrgModel>> ReadByParentOrg(string orgId)=>null;
        Task<EmpOrgModel> ReadByUserId(string userId);
        Task<EmpOrgModel> ReadChildOrgs(string orgId) => null;
        Task<EmpOrgModel> ReadForCheck(string ORG_ID) => null;
        Task Update(EmpOrgModel empOrg) => null;
    }
}