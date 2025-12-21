using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IProjectPrintService
    {
        /// <summary>
        /// 取得計畫預覽資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<ProjectPrintModel> GetProjectPrint(string PROJECT_NO, string type);

        /// <summary>
        /// 計畫預覽依特定條件顯示填報資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<Dictionary<string, object>> GetProjectPrintShowData(string PROJECT_NO);
    }
}
