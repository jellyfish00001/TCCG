using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDO.Models;

namespace SDO.Dac
{
    public interface IProjectExtensionDac : IDac
    {
        /// <summary>
        /// 取得展延紀錄清單
        /// </summary>
        /// <param name="PLAN_NO">計畫編號</param>
        /// <returns></returns>
        Task<List<ProjectExtensionModel>> GetRDExtensionList(string PLAN_NO);

        /// <summary>
        /// 取得展延紀錄明細
        /// </summary>
        /// <param name="EXTENSION_NO">展延編號</param>
        /// <returns></returns>
        Task<ProjectExtensionModel> GetRDExtension(string EXTENSION_NO);

        /// <summary>
        /// 取得展延紀錄明細的評核指標資料
        /// （流水號、評核類別、評核項目、預期完成期程、調整預期完成日期）
        /// </summary>
        /// <param name="PLAN_NO">計畫編號</param>
        /// <returns></returns>
        Task<List<ExtensionPolicyIndexModel>> GetPolicyIndexData(string PLAN_NO);

        /// <summary>
        /// 儲存展延紀錄
        /// </summary>
        /// <param name="model">展延紀錄 Model</param>
        /// <returns></returns>
        Task SaveRDExtension(ProjectExtensionModel model);

        /// <summary>
        /// 建立展延紀錄(Main)
        /// </summary>
        /// <param name="model">展延紀錄 Model</param>
        Task<int> InsertRDExtension(ProjectExtensionModel model);

        /// <summary>
        /// 展延紀錄評核指標 轉檔 RD_RES_POLICY_INDEX => RD_RES_POLICY_INDEX_ADJ
        /// </summary>
        /// <param name="policySEQs">展延紀錄序號</param>
        /// <param name="EXTENSION_NO">展延編號</param>
        Task TransferRDExtensionPolicyIndexAdj(List<int> policySEQs, string EXTENSION_NO, int EXTENSION_ID);


        

        /// <summary>
        /// 取得評核指標調整表所需的評核指標資料
        /// </summary>
        /// <param name="EXTENSION_NO">展延編號</param>
        /// <returns></returns>
        Task<List<ExtensionPolicyIndexModel>> GetExtensionPolicyIndexData(string EXTENSION_NO);

        /// <summary>
        /// 新增 評核指標調整表的資料
        /// </summary>
        /// <param name="model"></param>
        /// <remarks> STATUS 狀態: 1:未送審、2:送審、3:審核退回、4:審核通過</remarks>
        Task InsertPolicyIndex(List<ExtensionPolicyIndexModel> model);

        /// <summary>
        /// 修改 評核指標調整表的資料
        /// </summary>
        /// <param name="model"></param>
        Task UpdatePolicyIndex(List<ExtensionPolicyIndexModel> model);

        /// <summary>
        /// 刪除 評核指標調整表的資料
        /// </summary>
        /// <param name="POLICY_INDEX_ADJ_IDs"></param>
        Task DeletePolicyIndex(List<ExtensionPolicyIndexModel> model);


        /// <summary>
        /// 取得展延編號流水號
        /// 年度(3碼)+流水號(5碼)例：111300001
        /// </summary>
        /// <param name="planYear">計畫年度</param>
        string GetExtensionNoSeq(string planYear);
    }
}
