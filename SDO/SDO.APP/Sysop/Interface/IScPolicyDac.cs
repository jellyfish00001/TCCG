using SDO.Models;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IScPolicyDac : IDac
    {
        Task<ScPolicyModel> CheckExist(string policyId, string policyCompId);
        Task<ScPolicyModel> GetPolicy(string policyId, string policyCompId);
        Task Insert(ScPolicyModel model);
        Task Update(ScPolicyModel model);
        /// <summary>
        /// 檢查密碼原則是否與前次一致
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="UID"></param>
        /// <param name="Code"></param>
        /// <returns></returns>
        Task<bool> CheckPassSame(string UID, string Code);
    }
}