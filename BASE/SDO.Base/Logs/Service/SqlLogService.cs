using SDO.Dac;
using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class SqlLogService : Service, ISqlLogService
    {
        private readonly ISqlLogDac dac;
        public SqlLogService(ISqlLogDac dac)
        {
            this.dac = dac;
        }

        public async Task<IList<SqlTraceGridModel>> ReadSqlLog(GridBasicQryModel model)
        {
            // 檢查日期範圍是否正確
            if (model.END_DATE < model.START_DATE)
            {
                return null;
            }
            else
            {
                // 頁碼 --> 跳過筆數
                model.PAGE_NO *= model.PAGE_SIZE;

                return await dac.Read(model);
            }
        }
    }
}
