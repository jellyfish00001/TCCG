using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface ITrackoService
    {
        /// <summary>
        /// 取得本日到期或待辦案件資訊
        /// </summary>
        /// <returns></returns>
        Task<RtnTrackoModel> GetTrackoData();
    }
}
