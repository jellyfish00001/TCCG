using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IInnProjectDac : IDac
    {

        #region 創新提案基本資料
        /// <summary>
        /// 取得創新提案基本資料
        /// </summary>
        /// <param name="planNo"></param>
        /// <returns></returns>
        Task<InnProjectBasicModel> GetProjectInnBasic(string planNo);

        /// <summary>
        /// 取創新提案主題
        /// </summary>
        /// <param name="planNo"></param>
        /// <returns></returns>
        Task<List<InnProjectProposalTypeModel>> GetInnProjectProposalType(string planNo);

        /// <summary>
        /// 取創新提案自訂欄位
        /// </summary>
        /// <param name="planNo"></param>
        /// <returns></returns>
        Task<List<InnProjectCusFieldValueModel>> GetProjectCusFieldValue(string planNo);

        /// <summary>
        /// 取創新提案參與人
        /// </summary>
        /// <param name="planNO"></param>
        /// <returns></returns>
        Task<List<InnPartnerModel>> GetInnPartner(string planNO);


        #region AddMdf 創新提案基本資料、參與提案人、主題及涉及、自訂欄位
        /// <summary>
        /// 新增計畫基本資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns>回傳新增後，產生的列管編號</returns>
        Task InsertInnProjectBasic(InnProjectBasicModel model);

        /// <summary>
        /// 修改計畫基本資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns>回傳列管編號</returns>
        Task UpdateInnProjectBasic(InnProjectBasicModel model);

        /// <summary>
        /// 刪除參與提案人
        /// </summary>
        /// <param name="string InnPlanNO"></param>
        Task DeleteInnPartner(string InnPlanNO);

        /// <summary>
        /// 新增創新提案參與提案人
        /// </summary>
        /// <param name="model"></param>
        Task InsertInnPartner(List<InnPartnerModel> model);

        /// <summary>
        /// 刪除提案提案類別
        /// </summary>
        /// <param name="InnPlanNO"></param>
        Task DeleteProposalType(string InnPlanNO);

        /// <summary>
        /// 新增提案提案類別
        /// </summary>
        /// <param name="model"></param>
        Task InsertProjectProposalType(List<InnProjectProposalTypeModel> model);

        /// <summary>
        /// 刪除創新提案自定義欄位值
        /// </summary>
        /// <param name="InnPlanNO"></param>
        Task DeleteProjectCusFieldValue(string InnPlanNO);

        /// <summary>
        /// 新增創新提案自定義欄位值
        /// </summary>
        /// <param name="model"></param>
        Task InsertProjectCusFieldValue(List<InnProjectCusFieldValueModel> model);

        /// <summary>
        /// 刪除提案
        /// </summary>
        /// <param name="InnPlanNo"></param>
        Task DeleteInnProjectBasic(string InnPlanNo);

        #endregion
        #endregion

        /// <summary>
        /// 取得計劃編號流水號
        /// </summary>
        /// <param name="projectNoStart6Char">計劃編號前6碼</param>
        /// <returns></returns>
        string GetProjectNoSeq(string projectNoStart6Char);

        /// <summary>
        /// 抓取自取欄位名
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        Task<List<ProjectCusFieldModel>> GetInnProjectCusField(string INN_YEAR);


    }
}
