using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IMaintainYearService
    {
        /// <summary>
        /// 取計畫年度
        /// </summary>
        /// <returns></returns>
        Task<List<MaintainYearModel>> GetMaintainYear();
        /// <summary>
        /// 更新計畫年度
        /// </summary>
        /// <returns></returns>
        Task<bool> SetMaintainYear(List<MaintainYearModel> model);
    }
}
