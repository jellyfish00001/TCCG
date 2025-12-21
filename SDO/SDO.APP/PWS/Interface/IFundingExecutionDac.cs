using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IFundingExecutionDac : IDac
    {

        /// <summary>
        /// 取經費需求事項總計和執行情形
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        Task<int> GetFundingExecutionTOTAL(string PLANNO);
    }


}
