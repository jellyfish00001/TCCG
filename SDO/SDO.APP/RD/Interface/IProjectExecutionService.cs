using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IProjectExecutionService
    {
        /// <summary>
        /// 取得執行情形清單
        /// </summary>
        Task<List<ResPolicyListQueryModel>> GetRDResPolicyList(ResPolicyListQueryModel model);

        /// <summary>
        /// 取得執行情形明細
        /// </summary>
        Task<ResPolicyIndexModel> GetRDResPolicyIndex(int SEQ);

        /// <summary>
        /// 儲存執行情形
        /// </summary>
        Task SaveRDResPolicyIndex(ResPolicyIndexModel model);
    }
}
