using SDO.Dac;
using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class MailLogService : Service, IMailLogService
    {
        private readonly IMailLogDac dac;

        public MailLogService(IMailLogDac dac)
        {
            this.dac = dac;
        }

        /// <summary>
        /// 查詢郵寄日誌
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IList<MailLogModel>> GetMailLog(GridBasicQryModel model)
        {
            // 檢查日期範圍是否正確
            if (model.END_DATE < model.START_DATE)
            {
                return null;
            }
            else
            {
                // 轉換頁碼 --> 跳過筆數
                model.PAGE_NO *= model.PAGE_SIZE;
                return await dac.ReadMailLog(model);
            }
        }
        /// <summary>
        /// 查詢郵寄日誌
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> SetMailLog(MailLogModel model)
        {
            return await dac.WriteMailLog(model);
        }
        /// <summary>
        /// 查詢郵寄日誌
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool SetMailLogSync(MailLogModel model)
        {
            return  dac.WriteMailLogSync(model);
        }
    }
}
