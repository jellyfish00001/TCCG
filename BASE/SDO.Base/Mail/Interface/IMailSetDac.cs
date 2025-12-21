using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IMailSetDac : IDac
    {
        Task Delete(string mailId);
        Task DeleteRecipient(string mailId);
        Task Insert(MailSetMdfModel mail);
        Task InsertRecipient(IEnumerable<RecipientModel> models);
        Task<IList<MailSetModel>> Read(string userId);
        Task<MailSetModel> ReadById(string mailId);
        MailSetModel ReadByIdSync(string mailId);
        Task<IList<RecipientModel>> ReadRecipientById(string mailId);
        Task Update(MailSetMdfModel mail);
    }
}