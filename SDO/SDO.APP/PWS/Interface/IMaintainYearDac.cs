using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IMaintainYearDac
    {

        /// <summary>
        /// 取計畫年度
        /// </summary>
        /// <returns></returns>
        Task<List<MaintainYearModel>> GetMaintainYear();

        /// <summary>
        /// 更新截止日期
        /// </summary>
        Task SetMaintainYear(List<MaintainYearModel> model);
    }
}
