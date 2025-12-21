using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IProjectBasicDac : IDac
    {
        /// <summary>
        /// 變更委託研究刪除與撤銷
        /// 執行類別 D 刪除 R 撤銷 L1 送出鎖定L2 解除鎖定
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> SaveRDBasicStatus(ProjectBasicStatusModel model);

        /// <summary>
        /// 取得 委託研究的基本資料
        /// </summary>
        Task<ResearchBasicModel> GetRDResearchBasic(string PLAN_NO);

        /// <summary>
        /// 取得 委託研究的基本資料的每季評核指標資料
        /// </summary>
        Task<List<PolicyIndexModel>> GetPolicyIndexData(string PLAN_NO);

        /// <summary>
        /// 儲存 委託研究的基本資料
        /// </summary>
        Task<bool> UpdateRDResearchBasic(ResearchBasicModel model);

        /// <summary>
        /// 新增 評核指標資料
        /// </summary>
        Task InsertPolicyIndex(List<PolicyIndexModel> model);

        /// <summary>
        /// 修改 評核指標資料
        /// </summary>
        /// <param name="model"></param>
        Task UpdatePolicyIndex(List<PolicyIndexModel> model);

        /// <summary>
        /// 刪除 評核指標資料
        /// </summary>
        /// <param name="model"></param>
        Task DeletePolicyIndex(List<int> seq);

        /// <summary>
        /// 插入委託研究的基本資料
        /// </summary>
        Task InsertRDResearchBasic(ResearchBasicModel model);

        /// <summary>
        /// 產生計畫編號
        /// 年度(3碼)+流水號(5碼)例：10400001
        /// </summary>
        string GetPlanNoSeq(string planYear);
    }
}
