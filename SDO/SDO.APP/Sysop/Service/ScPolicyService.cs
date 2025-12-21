using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using SDO.Utils;

namespace SDO.Services
{
    public class ScPolicyService : Service, IScPolicyService
    {
        private readonly IScPolicyDac dac;
        private readonly IUserProfile userProfile;

        public ScPolicyService(IScPolicyDac dac,
            IUserProfile userProfile)
        {
            this.dac = dac;
            this.userProfile = userProfile;
        }

        public async Task<ScPolicyModel> GetPolicy(string policyId, string policyCompId)
        {
            return await dac.GetPolicy(policyId, policyCompId);
        }

        /// <summary>
        /// 更新 SC POLICY
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> UpdatePolicy(ScPolicyModel model)
        {
            #region 檢查存在
            ScPolicyModel check = await dac.CheckExist(model.POLICY_ID, model.POLICY_COMP_ID);
            #endregion

            #region 執行儲存
            if (check != default(ScPolicyModel))
            {
                model.MDF_USER = userProfile.GetLoginUser().USER_ID;
                await dac.Update(model);
            }
            else
            {
                await dac.Insert(model);
            }
            #endregion
            return ChangeResult(ResultType.Success | ResultType.Update);
        }
    }
}
