using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IMailQueueDac : IDac
    {
        Task Delete(string queueId);
        Task Insert(MailQueueQryResultModel model);
        Task<IList<MailQueueQryResultModel>> Read();
        Task<MailQueueQryResultModel> ReadById(string queueId);
        Task<MailQueueQryResultModel> ReadMailTemplateById(string mailId);
        Task Update(MailQueueQryResultModel model);
    }
}