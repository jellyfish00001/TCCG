using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IProjectBasicService
    {
        /// <summary>
        /// 變更委託研究刪除與撤銷
        /// 執行類別 D 刪除 R 撤銷 L1 送出鎖定L2 解除鎖定</summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> SaveRDBasicStatus(ProjectBasicStatusModel model);

        /// <summary>
        /// 取得委託研究的基本資料
        /// </summary>
        /// <param name="PLAN_NO">計畫編號</param>
        /// <returns></returns>
        Task<ResearchBasicModel> GetRDResearchBasic(string PLAN_NO);

        /// <summary>
        /// 儲存委託研究的基本資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<string> SaveRDResearchBasic(ResearchBasicModel model);

        /// <summary>
        /// 產生計畫編號
        /// 年度(3碼)+流水號(5碼)例：10400001
        /// </summary>
        string GenPlanNo(string planYear);
    }
}
