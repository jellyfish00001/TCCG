using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IMailLogDac : IDac
    {
        Task<IList<MailLogModel>> ReadMailLog(GridBasicQryModel model);
        /// <summary>
        /// 寫mailLog
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> WriteMailLog(MailLogModel model);
        bool WriteMailLogSync(MailLogModel model);
    }
}