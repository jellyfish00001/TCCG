using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Models;
using SDO.Services;
using SDO.Utils;

namespace SDO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class AnnouncementController : ControllerBase
    {
        private readonly IAnnouncementService announcementService;
        public AnnouncementController(IAnnouncementService announcementService)
        {
            this.announcementService = announcementService;
        }

        /// <summary>
        /// 查詢公告
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<IList<AnnouncementModel>> GetAnnouncement(AnnouncementQryModel model)
        {
            return await announcementService.GetAnnouncement(model);
        }

        /// <summary>
        /// 登入畫面公告
        /// </summary>
        /// <param name="annType">參數從url傳遞[公告類別(WAPL:00, APL:99)]</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IList<AnnouncementModel>> GetDisplayAnnouncement([FromQuery] string annType)
        {
            return await announcementService.GetDisplayAnnouncement(annType);
        }

        /// <summary>
        /// 查詢單筆公告
        /// </summary>
        /// <param name="sId"></param>
        /// <returns></returns>
        [HttpGet("{sId}")]
        public async Task<AnnouncementModel> GetAnnouncementById(string sId)
        {
            return await announcementService.GetAnnouncementById(sId);
        }

        /// <summary>
        /// 新增公告
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<RtnResultModel> InsertAnnouncement(AnnouncementModel model)
        {
            return await announcementService.InsertAnnouncement(model);
        }

        /// <summary>
        /// 更新公告
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<RtnResultModel> UpdateAnnouncement(AnnouncementModel model)
        {
            return await announcementService.UpdateAnnouncement(model);
        }

        /// <summary>
        /// 刪除公告
        /// </summary>
        /// <param name="sId"></param>
        /// <returns></returns>
        [HttpDelete("{sId}")]
        public async Task<RtnResultModel> DeleteAnnouncement(int sId)
        {
            return await announcementService.DeleteAnnouncement(sId);
        }

        /// <summary>
        /// 下載附件
        /// </summary>
        /// <param name="sId"></param>
        /// <returns></returns>
        [HttpGet("[action]/{sId}")]
        [AllowAnonymous]
        public async Task<FileContentResult> GetAttachment(string sId)
        {
            (byte[] ms, string contentType, string fileName) = await announcementService.GetAttachment(sId);
            return File(ms, contentType, fileName);
        }

        /// <summary>
        /// 取得公告類別清單
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<SetParamModel[]> GetAnnType()
        {
            return await announcementService.GetAnnType();
        }

        #region 最新公告(原系統)
        /// <summary>
        /// 取得公告
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<AnnouncementModel>> GetScAnnouncement()
        {
            return await announcementService.GetScAnnouncement();
        }
        #endregion 最新公告(原系統)
    }
}