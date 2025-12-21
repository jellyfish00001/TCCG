using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IAdjustNumberService
    {
        /// <summary>
        /// 取計畫資料
        /// </summary>
        /// <returns></returns>
        Task<List<AdjustNumberModel>> GetAdjustNumber(AdjustNumberQueryModel model);
        /// <summary>
        /// 計畫資料送審
        /// </summary>
        /// <returns></returns>
        Task<string> SetAdjustNumber(AdjustNumberQueryModel model);
    }
}
