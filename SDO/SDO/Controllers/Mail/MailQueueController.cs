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
    public class MailQueueController : ControllerBase
    {
        private readonly IMailQueueService mailQueueService;
        public MailQueueController(IMailQueueService mailQueueService)
        {
            this.mailQueueService = mailQueueService;
        }

        /// <summary>
        /// 查詢所有郵件排程
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IList<MailQueueMdfModel>> GetMailQueue()
        {
            return await mailQueueService.GetMailQueue();
        }

        /// <summary>
        /// 查詢一筆郵件排程資料
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        [HttpGet("{queueId}")]
        public async Task<MailQueueMdfModel> GetMailQueueById(string queueId)
        {
            return await mailQueueService.GetMailQueueById(queueId);
        }

        [HttpGet("[action]/{mailId}")]
        public async Task<MailQueueMdfModel> LoadMailTemplate(string mailId)
        {
            return await mailQueueService.LoadMailTemplate(mailId);
        }

        /// <summary>
        /// 新增一筆郵件排程資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<RtnResultModel> InsertMailQueue(MailQueueMdfModel model)
        {
            await mailQueueService.InsertMailQueue(model);
            return  new RtnResultModel(true, "存檔成功");
        }

        /// <summary>
        /// 修改一筆郵件排程資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<RtnResultModel> UpdateMailQueue(MailQueueMdfModel model)
        {
            await mailQueueService.UpdateMailQueue(model);
            return new RtnResultModel(true, "存檔成功");
        }

        /// <summary>
        /// 刪除未寄出的郵件排程資料
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        [HttpDelete("{queueId}")]
        public async Task<RtnResultModel> DeleteMailQueue(string queueId)
        {
            await mailQueueService.DeleteMailQueue(queueId);
            return new RtnResultModel(true, "刪除成功");
        }
    }
}