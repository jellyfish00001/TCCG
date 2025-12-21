using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    ///  SCAPPLICATIONM 相關DBIO
    /// </summary>
    public interface ISCAppDac
    {
        /// <summary>
        /// 取得登入使用者可用系統
        /// </summary>
        /// <returns></returns>
        Task<IList<SCAppModel>> GetUserApp();
        /// <summary>
        /// 以帳號取得使用者可使用系統
        /// </summary>
        /// <param name="UserId">使用者Id</param>
        /// <returns></returns>
        Task<IList<SCAppModel>> GetUserApp(string UserId);
    }
}

