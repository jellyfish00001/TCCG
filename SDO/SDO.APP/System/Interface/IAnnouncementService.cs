using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IAnnouncementService
    {
        Task<IList<AnnouncementModel>> GetAnnouncement(AnnouncementQryModel model);
        Task<IList<AnnouncementModel>> GetDisplayAnnouncement(string annType);
        Task<AnnouncementModel> GetAnnouncementById(string sId);
        Task<RtnResultModel> InsertAnnouncement(AnnouncementModel model);
        Task<RtnResultModel> DeleteAnnouncement(int sId);
        Task<RtnResultModel> UpdateAnnouncement(AnnouncementModel model);
        Task<(byte[] ms, string contentType, string fileName)> GetAttachment(string sId);
        Task<SetParamModel[]> GetAnnType();

        #region 最新公告(原系統)
        /// <summary>
        /// 取得公告
        /// </summary>
        /// <returns></returns>
        Task<List<AnnouncementModel>> GetScAnnouncement();
        #endregion 最新公告(原系統)
    }
}
