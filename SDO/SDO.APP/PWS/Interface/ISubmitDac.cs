using Microsoft.CodeAnalysis;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface ISubmitDac : IDac
    {
        /// <summary>
        /// 檢查當期執行情形是否已送出
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<bool> CheckProjectFillIsSend(string PLANNO);

        /// <summary>
        /// 計畫送出
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        Task ProjectFillSubmit(string PLANNO);

        /// <summary>
        /// 檢查計畫經費是否一致
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        Task<bool> CheckProjectMoney(string PLANNO);

        /// <summary>
        /// 檢查基本計畫是否存在
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        Task<List<string>> ChkPlanBasicAValid(string PLANNO);
    }


}
