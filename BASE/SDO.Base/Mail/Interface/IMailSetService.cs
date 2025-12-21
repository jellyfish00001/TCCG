using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using SDO.Models;

namespace SDO.Services
{
    public interface IMailSetService
    {
        Task<RtnResultModel> Create(MailSetMdfModel mail);
        Task<RtnResultModel> Delete(string mailId);
        Task<IList<MailSetModel>> Read();
        Task<MailSetModel> ReadById(string mailId);
        MailSetModel ReadByIdSync(string mailId);
        Task<IList<RecipientModel>> ReadRecipientById(string mailId);
        Task<RtnResultModel> Update(MailSetMdfModel mail);
        Task<IRtnResult> UpdateRecipient(RecipientMdfModel updateRecipient);
        /// <summary>
        /// 送信
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="RcvList"></param>
        /// <returns></returns>
        Task<bool> Send(MailSetModel mail, IList<RecipientModel> RcvList,List<Attachment> attachments = null);
        /// <summary>
        /// 透過範本寄信
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> SetTemplateSend<T>(MailTemplateSendModel<T> model);
        bool SetTemplateSendSync<T>(MailTemplateSendModel<T> model);
    }
}