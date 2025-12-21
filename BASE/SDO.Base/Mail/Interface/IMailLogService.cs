using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IMailLogService
    {
        Task<IList<MailLogModel>> GetMailLog(GridBasicQryModel model);
        Task<bool> SetMailLog(MailLogModel model);
        bool SetMailLogSync(MailLogModel model);
    }
}
