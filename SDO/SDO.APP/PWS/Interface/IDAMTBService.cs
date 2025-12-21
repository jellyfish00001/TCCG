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
    public interface IDAMTBService
    {
        /// <summary>
        /// 存經費需求細項
        /// </summary>
        /// <returns></returns>
        Task<bool> SaveDAMTB(DAMTBIDModel model);


        /// <summary>
        /// 取得經費需求細項
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        Task<DAMTBIDModel> GetDAMTB(string PLANNO);
    }
}
