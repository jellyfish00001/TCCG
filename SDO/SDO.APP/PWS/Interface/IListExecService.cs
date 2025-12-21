using Microsoft.AspNetCore.Mvc;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IListExecService
    {
        /// <summary>
        /// 查詢先期計畫資料
        /// </summary>
        /// <returns></returns>
        Task<List<ListExecModel>> GetPWSProjectList(ListExecQueryModel model);

        /// <summary>
        /// 查詢機關是否截止
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        Task<bool> GetOrgDeadline(string OU_ID);

        /// <summary>
        /// 刪除計畫
        /// </summary>
        /// <returns></returns>
        Task<bool> DeleteProjectList(List<string> PLANNO); 

    }
}
