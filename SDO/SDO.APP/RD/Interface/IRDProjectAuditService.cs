using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SDO.Services
{
    public interface IRDProjectAuditService
    {
        /// <summary>
        /// 取得審核紀錄清單
        /// </summary>
        /// <param name="model">審查資料檔 Model</param>
        /// <returns></returns>
        Task<List<RDAuditModel>> GetRDAuditList(string MAIN_NO);

        /// <summary>
        /// 儲存審核紀錄
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task SaveRDAudit(RDAuditModel model);

        /// <summary>
        /// 更改審核狀態，以及審核前要儲存對應的章節資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task ChangeReviewStatus(RDAuditStatusModel model);

        /// <summary>
        /// 產生審查紀錄編號
        /// planReviewType + 民國年度(3碼) + 流水號(4碼)例：A1113010001
        /// </summary>
        /// <param name="planYear">送審年度</param>
        /// <param name="planMonth">送審月份</param>
        /// <param name="planReviewType">審核類別</param>
        /// <returns></returns>
        public string GenRDAuditId(string planYear, string planMonth, string planReviewType);
    }
}
