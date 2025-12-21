using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IListExecDac : IDac
    {
        /// <summary>
        /// 查詢先期計畫資料
        /// </summary>
        /// <param name="model"></param>
        Task<List<ListExecModel>> GetPWSProjectList(ListExecQueryModel model);

        /// <summary>
        /// 查詢機關是否截止
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        Task<bool> GetOrgDeadline(string OU_ID);

        /// <summary>
        /// 刪除計畫主檔
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        Task DeletePWSSDPLANMAIN(List<string> PLANNO);
        /// <summary>
        /// 刪除跨年度預算
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        Task DeletePROJECT_BUDGET_SOURCE_G(List<string> PLANNO);
        /// <summary>
        /// 刪除歷年執行情形
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        Task DeletePWSSDHISTORYEXE(List<string> PLANNO);
        /// <summary>
        /// 刪除近三年相關研究
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        Task DeletePWSSDTREEYEARPLAN(List<string> PLANNO);
        /// <summary>
        /// 刪除經費需求細項
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        Task DeletePWSSDPLANFUND(List<string> PLANNO);
        /// <summary>
        /// 刪除小組審核紀錄
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        Task DeletePWSSDVIEW(List<string> PLANNO);
        /// <summary>
        /// 刪除檔案上傳
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        Task DeletePROJECT_ATTACHMENT(List<string> PLANNO);


    }


}
