
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Models;
using SDO.Services;

namespace SDO.Controllers
{
    [Authorize(Roles = "handUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class EmpOrgController : ControllerBase
    {
        private readonly IEmpOrgService empOrgService;
        private readonly IEmpUserService empUserService;

        public EmpOrgController(IEmpOrgService empOrgService, IEmpUserService empUserService)
        {
            this.empOrgService = empOrgService;
            this.empUserService = empUserService;
        }

        /// <summary>
        /// Get all EMP_ORGs
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        [HttpGet("{orgId?}")]
        public async Task<IList<EmpOrgModel>> GetEmpOrg(string orgId = "")
        {
            // 前端呼叫這支長出 OrgTree
            return await empOrgService.ReadByParentOrg(orgId);
        }

        /// <summary>
        /// Get all ORGs
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IList<EmpOrgModel>> GetOrgs()
        {
            return await empOrgService.ReadAllOrgs();
        }

        /// <summary>
        /// 取得所有機關
        /// 系統管理員:所有機關
        /// 機關使用者:所屬機關
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IList<EmpOrgModel>> GetOrgList()
        {
            return await empOrgService.GetOrgList();
        }

        /// <summary>
        /// 取得OrgByOrgId
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]/{orgId}")]
        public async Task<EmpOrgModel> GetOrgByOrgId(string orgId)
        {
            return await empOrgService.ReadByOrgId(orgId);
        }

        /// <summary>
        /// Get Multi ORGs
        /// </summary>
        /// <param name="orgIds"></param>
        /// <returns></returns>
        /// 這裡的orgId是多選, 避免串url參數超過長度限制, 改用POST
        /// GET的參數串在url, 不能傳array
        [HttpPost("[action]")]
        public async Task<IList<EmpOrgModel>> GetMultiOrgs([FromBody]string[] orgIds)
        {
            return await empOrgService.ReadByOrgIds(orgIds);
        }

        /// <summary>
        /// Get all Org's Users
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        [HttpGet("[action]/{orgId?}")]
        public async Task<IList<UserDataModel>> GetOrgUsers(string orgId = "")
        {
            return await empUserService.GetUserByOrg(orgId);
        }

        /// <summary>
        /// Get Parent By orgId
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        [HttpGet("[action]/{orgId}")]
        public async Task<EmpOrgModel> GetParentOrg(string orgId)
        {
            return await empOrgService.ReadChildOrgs(orgId);
        }

        /// <summary>
        /// 取得沒有所屬機關的user
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IList<UserDataModel>> GetEmpUsers()
        {
            IList<UserDataModel> users = await empUserService.Read(new EmpUserReadModel());
            IDictionary<string, bool> notEmpOrgUser = new Dictionary<string, bool>();
            foreach(UserDataModel user in users)
            {
                notEmpOrgUser.Add(user.USER_ID, string.IsNullOrEmpty((await empOrgService.ReadByUserId(user.USER_ID)).ORG_ID));
            }
            return users.Where(user => notEmpOrgUser[user.USER_ID]).OrderBy(user => user.USER_NAME).ToList();
        }

        /// <summary>
        /// 新增一筆 EMP_ORG
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<RtnResultModel> CreateEmpOrg(EmpOrgMdfModel model)
        {
            return await empOrgService.CreateEmpOrg(model);
        }

        /// <summary>
        /// 修改 EMP_ORG
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<RtnResultModel> UpdateEmpOrg(EmpOrgMdfModel model)
        {
            return await empOrgService.UpdateEmpOrg(model);
        }

        /// <summary>
        /// 刪除EMP_ORG
        /// </summary>
        /// <param name="orgIds"></param>
        /// <returns></returns>
        [HttpDelete("{orgIds}")]
        public async Task<RtnResultModel> DeleteEmpOrg(string orgIds)
        {
            return await empOrgService.DeleteEmpOrg(orgIds);
        }

        /// <summary>
        /// 取得全部的 EmpOrgs 組合成 DropDownTree結構(前端使用)
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<EmpOrgInfoModel[]> GetAllOrgs()
        {
            return await empOrgService.GetAllOrgs();
        }
    }
}