using SDO.APP.INN.Models.ProjectManage;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IInnProjectService
    {
        /// <summary>
        /// 取得提案基本資料
        /// </summary>
        /// <param name="projectNo"></param>
        /// <returns></returns>
        Task<InnProjectBasicFillModel> GetInnBasic(string projectNo);

        #region 計劃基本資料
        /// <summary>
        /// 儲存創新提案基本資料(含計畫基本資料、提案提案類別、自定義欄位、參與提案人)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<string> SaveInnBasic(InnProjectBasicFillModel model);
        #endregion


        /// <summary>
        /// 產生計劃編號
        /// </summary>
        /// <param name="planYear">民國年</param>
        /// <param name="OU_ID">執行機關的機關代碼</param>
        /// <returns></returns>
        string GenProjectNo(string planYear, string OU_ID);

    }
}
