using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IProjectExecutionDac : IDac
    {
        /// <summary>
        /// 取得執行情形填報清單
        /// </summary>
        Task<List<ResPolicyListQueryModel>> GetRDResPolicyList(ResPolicyListQueryModel model);

        /// <summary>
        /// 取得執行情形填報明細
        /// </summary>
        Task<ResPolicyIndexModel> GetRDResPolicyIndex(int SEQ);

        /// <summary>
        /// 修改執行情形
        /// </summary>
        Task<bool> SaveRDResPolicyIndex(ResPolicyIndexModel model);
    }
}
