using SDO.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IAnnouncementDac : IDac
    {
        Task Delete(int sId);
        Task Insert(AnnouncementModel model);
        Task<string[]> GetOrgIdList(string ORG_ID);
        Task<IList<AnnouncementModel>> Read(AnnouncementQryModel model);
        Task<AnnouncementModel> ReadById(string sId);
        Task<IList<AnnouncementModel>> ReadDisplay(string annType);
        Task Update(AnnouncementModel model);
        Task<bool> IsUserSysAdmin(string USER_ID);
        Task<SetParamModel[]> GetAnnType();

        #region 最新公告(原系統)
        /// <summary>
        /// 取得公告
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns></returns>
        Task<List<AnnouncementModel>> GetScAnnouncement(DateTime date);
        #endregion 最新公告(原系統)
    }
}