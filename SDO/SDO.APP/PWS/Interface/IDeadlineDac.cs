using SDO.Base.Utils.Models;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IDeadlineDac : IDac
    {

        /// <summary>
        /// 取計畫年度
        /// </summary>
        /// <returns></returns>
        Task<List<DeadlineModel>> GetPlanYear();

        /// <summary>
        /// 更新機關截止日期
        /// </summary>
        Task SetOrgDeadLine(DeadlineModel model);

        /// <summary>
        /// 更新區公所截止日期
        /// </summary>
        Task SetDisDeadLine(DeadlineModel model);

        /// <summary>
        /// 是否有年度
        /// </summary>
        /// <param name="PLANYEAR"></param>
        /// <returns></returns>
        Task<bool> IsDeadlineYear(string PLANYEAR);

        /// <summary>
        /// 新增年度機關
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        Task AddOrgData(DeadlineModel model);

        /// <summary>
        /// 取機關得截止日
        /// </summary>
        Task<DeadlineModel> GetOrgDeadline(string year);

        /// <summary>
        /// 取區公所得截止日
        /// </summary>
        Task<DeadlineModel> GetDisDeadline(string year);
    }
}
