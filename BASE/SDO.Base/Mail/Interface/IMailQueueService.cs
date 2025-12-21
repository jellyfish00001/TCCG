using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IMailQueueService
    {
        Task<IList<MailQueueMdfModel>> GetMailQueue();
        Task<MailQueueMdfModel> GetMailQueueById(string queueId);
        Task<RtnResultModel> InsertMailQueue(MailQueueMdfModel model);
        Task<RtnResultModel> UpdateMailQueue(MailQueueMdfModel model);
        Task<RtnResultModel> DeleteMailQueue(string queueId);
        Task<MailQueueMdfModel> LoadMailTemplate(string mailId);
    }
}
