using SDO.Dac;
using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class MailQueueService : Service, IMailQueueService
    {
        private readonly IMailQueueDac dac;
        public MailQueueService(IMailQueueDac dac)
        {
            this.dac = dac;
        }

        /// <summary>
        /// 刪除未寄出的郵件排程資料
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> DeleteMailQueue(string queueId)
        {
            #region 檢查是否郵件排程資料已寄出
            MailQueueQryResultModel check = await dac.ReadById(queueId);
            // 預防信件不存在查詢回傳null
            if (check == null || check.SEND_FLG == 1)
            {
                return ChangeResult(false, i18N.Message.R19);
            }
            #endregion
            await dac.Delete(queueId);
            return ChangeResult(ResultType.Success | ResultType.Delete);
        }

        /// <summary>
        /// 查詢所有郵件排程資料
        /// </summary>
        /// <returns></returns>
        public async Task<IList<MailQueueMdfModel>> GetMailQueue()
        {
            IList<MailQueueMdfModel> result = new List<MailQueueMdfModel>();
            foreach (MailQueueQryResultModel data in await dac.Read())
            {
                result.Add(data.Decode().ToEditView());
            }
            return result;
        }

        /// <summary>
        /// 查詢一筆郵件排程資料
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        public async Task<MailQueueMdfModel> GetMailQueueById(string queueId)
        {
            // 解碼被編碼的欄位 , Json字串轉Model避免過長
            MailQueueQryResultModel result = await dac.ReadById(queueId);

            // 避免資料不存在回傳null 加上判斷
            return result?.Decode().ToEditView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        public async Task<MailQueueMdfModel> LoadMailTemplate(string mailId)
        {
            MailQueueQryResultModel data = await dac.ReadMailTemplateById(mailId);

            return data?.Decode().ToEditView();
        }

        /// <summary>
        /// 新增一筆郵件排程資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> InsertMailQueue(MailQueueMdfModel model)
        {
            // Model parse, 轉換MailAddress成JsonStr
            MailQueueQryResultModel param = model.ToQueryData();

            await dac.Insert(param);
            return ChangeResult(ResultType.Success | ResultType.Insert);
        }

        /// <summary>
        /// 修改郵件排程資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> UpdateMailQueue(MailQueueMdfModel model)
        {
            #region 檢查郵件排程資料是否已寄出 send_flg = 1
            MailQueueQryResultModel check = await dac.ReadById(model.QUEUE_ID);
            // 避免查詢回傳null 加上判斷
            if (check == null || check.SEND_FLG == 1)
            {
                return ChangeResult(false, i18N.Message.R18);
            }
            #endregion
            // Model parse, 轉換MailAddress成JsonStr
            MailQueueQryResultModel param = model.ToQueryData();

            await dac.Update(param);
            return ChangeResult(ResultType.Success | ResultType.Update);
        }
    }
}
