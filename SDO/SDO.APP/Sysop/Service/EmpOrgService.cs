using SDO.Dac;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SDO.Services
{
    public class EmpOrgService : Service, IEmpOrgService
    {
        private readonly IEmpOrgDac dac;
        private readonly IUserProfile userProfile;

        public EmpOrgService(IEmpOrgDac dac, IUserProfile userProfile)
        {
            this.dac = dac;
            this.userProfile = userProfile;
        }

        /// <summary>
        /// 建立EmpOrg
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> CreateEmpOrg(EmpOrgModel model)
        {
            #region 檢查 EMP_ORG 存不存在
            EmpOrgModel check = await dac.ReadForCheck(model.ORG_ID);
            if (check != default(EmpOrgModel) && !check.DEL_FLG)
            {
                return ChangeResult(ResultType.Fail | ResultType.Insert, HtmlEncode(model.ORG_ID));
            }
            #endregion

            model.CRT_USER = userProfile.GetLoginUser().USER_ID;
            model.MDF_USER = userProfile.GetLoginUser().USER_ID;

            dac.BeginTransaction();
            #region 不存在 EMP_ORG 執行新增
            await ((check != default) ? dac.Update(model) : dac.Insert(model));
            #endregion
            // 更新單位人員對應
            await UpdateMapOrgUser(model);
            dac.Commit();

            return ChangeResult(ResultType.Success | ResultType.Insert);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> UpdateEmpOrg(EmpOrgModel model)
        {
            model.CRT_USER = userProfile.GetLoginUser().USER_ID;
            model.MDF_USER = userProfile.GetLoginUser().USER_ID;
            dac.BeginTransaction();
            await dac.Update(model);
            // 更新單位人員對應
            await UpdateMapOrgUser(model);
            dac.Commit();

            return ChangeResult(ResultType.Success | ResultType.Update);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orgIds"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> DeleteEmpOrg(string orgIds)
        {
            dac.BeginTransaction();
            string mdfUser = userProfile.GetLoginUser().USER_ID;
            string[] orgIdList = orgIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            IList obj = new ArrayList();
            foreach (string orgId in orgIdList)
            {
                ((ArrayList)obj).Add(new
                {
                    ORG_ID = orgId,
                    MDF_USER = mdfUser
                });
            }
            // 更新del_flg為已刪除
            await dac.Delete(obj);
            // 刪除單位人員對應
            await dac.DeleteMap(orgIdList);
            dac.Commit();

            return ChangeResult(ResultType.Success | ResultType.Delete);
        }

        /// <summary>
        /// 取得全部的EmpOrgs
        /// </summary>
        /// <returns></returns>
        public async Task<IList<EmpOrgModel>> ReadAllOrgs()
        {
            return await dac.ReadAllOrgs();
        }

        /// <summary>
        /// 取得所有機關
        /// 系統管理員:所有機關
        /// 機關使用者:所屬機關
        /// </summary>
        /// <returns></returns>
        public async Task<IList<EmpOrgModel>> GetOrgList()
        {
            var orgId = userProfile.GetLoginUser().ORG_ID;
            List<EmpOrgModel> result = new List<EmpOrgModel>();
            //機關使用者只回傳自己的機關
            if (!orgId.Equals("MOEA"))
            {
                var orgData = await dac.ReadByOrgId(orgId);
                result.Add(orgData);
                return result;
            }
            return await dac.ReadAllOrgs();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public async Task<EmpOrgModel> ReadChildOrgs(string orgId)
        {
            return await dac.ReadChildOrgs(orgId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public async Task<IList<EmpOrgModel>> ReadByParentOrg(string orgId)
        {
            orgId = HtmlEncode(orgId);
            return await dac.ReadByParentOrg(orgId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public async Task<EmpOrgModel> ReadByOrgId(string orgId)
        {
            return await dac.ReadByOrgId(orgId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public async Task<IList<EmpOrgModel>> ReadByOrgIds(string[] orgIds)
        {
            // orgId編碼
            orgIds = orgIds.Select(p => HtmlEncode(p)).ToArray();
            return await dac.ReadByOrgIds(orgIds);
        }

        public async Task<EmpOrgModel> ReadByUserId(string userId)
        {
            return await dac.ReadByUserId(userId);
        }

        /// <summary>
        /// 更新單位人員對應
        /// </summary>
        /// <param name="model"></param>
        private async Task UpdateMapOrgUser(EmpOrgModel model)
        {
            Task deleteTask = dac.DeleteMap(new string[] { model.ORG_ID });

            if (model.USERS?.Length != null && model.USERS.Length > 0)
            {
                MapOrgUserModel[] obj = new MapOrgUserModel[model.USERS.Length];
                Parallel.For(0, model.USERS.Length, i =>
                {
                    obj[i] = new MapOrgUserModel()
                    {
                        USER_ID = model.USERS[i],
                        ORG_ID = model.ORG_ID
                    };
                });

                await deleteTask;
                await dac.InsertMap(obj);
            }
            await deleteTask;
        }

        /// <summary>
        /// 取得全部的EmpOrgs 組合成 DropDownTree結構(前端使用)
        /// </summary>
        /// <returns></returns>
        public async Task<EmpOrgInfoModel[]> GetAllOrgs()
        {
            var data = await dac.ReadAllOrgs();
            var result = (from org in data
                          where string.IsNullOrEmpty(org.PARENT_ID)
                          select new EmpOrgInfoModel
                          {
                              ORG_ID = org.ORG_ID,
                              ORG_DISPLAY = org.ORG_DISPLAY,
                              items = GetOrgInfoModel(org.ORG_ID, data)
                          }).ToArray();
            return result;
        }

        private EmpOrgInfoModel[] GetOrgInfoModel(string orgId, IList<EmpOrgModel> empOrgs)
        {
            var orgs = empOrgs.Where(org => org.PARENT_ID == orgId).ToList();

            var result = new List<EmpOrgInfoModel>();
            foreach (var org in orgs)
            {
                result.Add(new EmpOrgInfoModel
                {
                    ORG_ID = org.ORG_ID,
                    ORG_DISPLAY = org.ORG_DISPLAY,
                    items = GetOrgInfoModel(org.ORG_ID, empOrgs)
                });
            }
            return result.ToArray();
        }
    }
}
