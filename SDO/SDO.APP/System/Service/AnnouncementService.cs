using Ionic.Zip;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDO.Utils;

namespace SDO.Services
{
    public class AnnouncementService : Service, IAnnouncementService
    {
        private readonly IAnnouncementDac dac;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ISysParam sysParam;
        private readonly IUploadFileService uploadFileService;
        private readonly IUserProfile userProfile;

        public AnnouncementService(IAnnouncementDac dac, 
            IWebHostEnvironment webHostEnvironment, 
            ISysParam sysParam, 
            IUploadFileService uploadFileService,
            IUserProfile userProfile)
        {
            this.dac = dac;
            this.webHostEnvironment = webHostEnvironment;
            this.sysParam = sysParam;
            this.uploadFileService = uploadFileService;
            this.userProfile = userProfile;
        }

        /// <summary>
        /// 表格查詢
        /// </summary>
        /// <returns></returns>
        public async Task<IList<AnnouncementModel>> GetAnnouncement(AnnouncementQryModel model)
        {
            var orgId = userProfile.GetLoginUser().ORG_ID;
            model.USER_ID = userProfile.GetLoginUser().USER_ID;
            model.ORG_IDs = await dac.GetOrgIdList(orgId);
            return await dac.Read(model);
        }

        /// <summary>
        /// 查詢公告
        /// </summary>
        /// <returns></returns>
        public async Task<IList<AnnouncementModel>> GetDisplayAnnouncement(string annType)
        {
            return await dac.ReadDisplay(annType);
        }

        /// <summary>
        /// 查詢單筆公告
        /// </summary>
        /// <param name="sId"></param>
        /// <returns></returns>
        public async Task<AnnouncementModel> GetAnnouncementById(string sId)
        {
            return await dac.ReadById(sId);
        }

        /// <summary>
        /// 新增公告
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> InsertAnnouncement(AnnouncementModel model)
        {
            model.COMMENT ??= string.Empty;
            model.ORG_ID = userProfile.GetLoginUser().ORG_ID;
            model.USER_ID = userProfile.GetLoginUser().USER_ID;
            model = CheckAnnouncement(model);
            await dac.Insert(model);
            return ChangeResult(ResultType.Success | ResultType.Insert);
        }

        /// <summary>
        /// 更新公告
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> UpdateAnnouncement(AnnouncementModel model)
        {
            model.COMMENT ??= string.Empty;
            model = CheckAnnouncement(model);
            await dac.Update(model);
            return ChangeResult(ResultType.Success | ResultType.Update);
        }

        private AnnouncementModel CheckAnnouncement(AnnouncementModel model)
        {
            bool isUrlLink = model.IS_URL_LINK == "Y";
            model.COMMENT = isUrlLink ? string.Empty : model.COMMENT;
            model.URL_LINK = isUrlLink ? model.URL_LINK : null;
            return model;
        }

        /// <summary>
        /// 刪除公告
        /// </summary>
        /// <param name="sId"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> DeleteAnnouncement(int sId)
        {
            await dac.Delete(sId);
            return ChangeResult(ResultType.Success | ResultType.Delete);
        }

        /// <summary>
        /// 下載附件
        /// </summary>
        /// <param name="sId"></param>
        /// <returns></returns>
        public async Task<(byte[] ms, string contentType, string fileName)> GetAttachment(string sId)
        {
            AnnouncementModel model = await GetAnnouncementById(sId);
            using (MemoryStream ms = new MemoryStream()) {
                if (model != default(AnnouncementModel) && model.ATTACH_NAME.Trim().Length > 0)
                {
                    using (ZipFile zip = new ZipFile(Encoding.UTF8))
                    {
                        // ASP.NET CORE使用IWebHostEnvironment取得網站的root Path
                        string savePath = (await sysParam.GetSysParam("SystemConfig", "UploadFolder")).SET_VALUE.Replace("~", webHostEnvironment.ContentRootPath);
                        foreach (string attribute in model.ATTACH_NAME.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            //取得檔案資訊(binary,full filname,context)
                            var downFile = await uploadFileService.Download(attribute);
                            if (!string.IsNullOrEmpty(downFile.fileName))
                            {
                                zip.AddEntry(downFile.fileName, downFile.bytes);
                            }
                        }
                        if (zip.Count == 0)
                        {
                            zip.AddEntry(i18N.Message.R09, "");
                        }
                        zip.Save(ms);
                    }
                }
                string fileName = string.Format("{0}.zip", model == null ? "title" : model.TITLE);
                new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string contentType);
                contentType ??= "application/octet-stream";
                return (ms.ToArray(), contentType, fileName);
            }
        }

        /// <summary>
        /// 取得公告類別清單
        /// </summary>
        /// <returns></returns>
        public async Task<SetParamModel[]> GetAnnType()
        {
            var userId = userProfile.GetLoginUser().USER_ID;
            var isSysAdmin = await dac.IsUserSysAdmin(userId);

            // 如果 此會員 role_id 有 SysAdmin，則僅顯示00和99，否則不顯示00和99，其他顯示。
            var setTypes = new List<string> { "00", "99" };
            var data = isSysAdmin 
                ? (await dac.GetAnnType()).Where(x => setTypes.Contains(x.SET_TYPE)).ToArray()
                : (await dac.GetAnnType()).Where(x => !setTypes.Contains(x.SET_TYPE)).ToArray();
            return data;
        }

        #region 最新公告(原系統)
        /// <summary>
        /// 取得公告
        /// </summary>
        /// <returns></returns>
        public async Task<List<AnnouncementModel>> GetScAnnouncement()
        {
            var dacResult = await dac.GetScAnnouncement(DateTime.Today);
            dacResult.ForEach(x =>
            {
                x.COMMENT = x.COMMENT.Replace("\r\n", "<br/>").Replace("\n", "<br/>");
            });
            return dacResult;
        }
        #endregion 最新公告(原系統)

    }
}
