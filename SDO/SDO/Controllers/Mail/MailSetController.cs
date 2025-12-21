using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Models;
using SDO.Services;

namespace SDO.Controllers
{
    [Authorize(Roles = "handUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class MailSetController : ControllerBase
    {
        private readonly IIPCMailSetService mailSetService;

        public MailSetController(IIPCMailSetService mailSetService)
        {
            this.mailSetService = mailSetService;
        }
        /// <summary>
        /// 取得所有郵件範本
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IList<MailSetModel>> Read()
        {
            return await mailSetService.Read();
        }
        /// <summary>
        /// 查詢郵件範本信息
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        [HttpGet("{mailId}")]
        public async Task<MailSetModel> ReadById(string mailId)
        {
            return await mailSetService.ReadById(mailId);
        }
        /// <summary>
        /// 查詢郵件範本收件人
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        [HttpGet("[action]/{mailId}")]
        public async Task<IList<RecipientModel>> ReadRecipientById(string mailId)
        {
            return await mailSetService.ReadRecipientById(mailId);
        }
        /// <summary>
        /// 新增郵件範本
        /// </summary>
        /// <param name="mail"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<RtnResultModel> Create(MailSetMdfModel mail){
            return await mailSetService.Create(mail);
        }
        /// <summary>
        /// 更新郵件範本
        /// </summary>
        /// <param name="mail"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<RtnResultModel> Update(MailSetMdfModel mail)
        {
            return await mailSetService.Update(mail);
        }
        /// <summary>
        /// 刪除郵件範本
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        [HttpDelete("{mailId}")]
        public async Task<RtnResultModel> Delete(string mailId)
        {
            return await mailSetService.Delete(mailId);
        }
        /// <summary>
        /// 更新寄，收件者資料
        /// </summary>
        /// <param name="updateRecipient"></param>
        /// <returns></returns>
        [HttpPut("[action]")]
        public async Task<IRtnResult> UpdateRecipient(RecipientMdfModel updateRecipient)
        {
            return await mailSetService.UpdateRecipient(updateRecipient);
        }
        /// <summary>
        /// 取得所有郵件範本
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<IList<IPCMailSetModel>> GetMailTemplate()
        {
            return await mailSetService.GetMailTemplate();
        }
        /// <summary>
        /// 取得郵件範本 by ID
        /// </summary>
        /// <param name="MAIL_ID"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<IPCMailSetModel> GetMailTemplateById([FromBody] string MAIL_ID)
        {
            return await mailSetService.GetMailTemplateById(MAIL_ID);
        }
        /// <summary>
        /// 儲存郵件範本資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveMailTemplate(MailSetModel model)
        {
            return mailSetService.SaveMailTemplate(model);
        }
    }
}