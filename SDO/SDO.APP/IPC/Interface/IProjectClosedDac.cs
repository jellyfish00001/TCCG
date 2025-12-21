using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IProjectClosedDac : IDac
    {
        /// <summary>
        /// 取得計畫結案資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<ProjectFillCloseModel> GetProjectFillClose(string PROJECT_NO);

        /// <summary>
        /// 新增計畫實際經費支用
        /// </summary>
        /// <param name="model"></param>
        void AddMdfProjectPayment(ProjectFillCloseModel model);

        /// <summary>
        /// 更新計畫基本資料結案審核結果
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectCloseRvwResult(ProjectFillCloseModel model);

        int GetWorkEnd(string PROJECT_NO);

        /// <summary>
        /// 清除驗收檢核點
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="SEQ"></param>
        void ClearWorkEnd(string PROJECT_NO, int SEQ);
    }
}
