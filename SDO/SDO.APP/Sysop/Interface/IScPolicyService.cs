using SDO.Models;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IScPolicyService
    {
        /// <summary>
        /// 取得帳戶原則
        /// </summary>
        Task<ScPolicyModel> GetPolicy(string policyId, string policyCompId);

        /// <summary>
        /// 更新SC POLICY
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<RtnResultModel> UpdatePolicy(ScPolicyModel model);
    }
}