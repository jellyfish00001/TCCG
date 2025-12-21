using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IDAMTBDac : IDac
    {
        /// <summary>
        /// 刪除經費需求事項
        /// </summary>
        /// <param name="model"></param>
        Task DeleteDAMTB(string PLANNO);

        /// <summary>
        /// 新增經費需求事項
        /// </summary>
        /// <param name="model"></param>
        Task SaveDAMTB(List<DAMTBModel> models);


        /// <summary>
        /// 取經費需求事項
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        Task<List<DAMTBModel>> GetDAMTB(string PLANNO);

        /// <summary>
        /// 取經費需求事項總計
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        Task<int> GetDAMTBTOTAL(string PLANNO);
    }


}
