using SDO.Dac;
using SDO.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SDO.Services
{
    public class ApiLogService : Service, IApiLogService
    {
        private readonly IApiLogDac dac;

        public ApiLogService(IApiLogDac dac)
        {
            this.dac = dac;
        }

        public async Task<IList<ApiTraceModel>> GetApiLog(GridBasicQryModel model)
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

                return await dac.ReadByDate(model);
            }
        }
    }
}
