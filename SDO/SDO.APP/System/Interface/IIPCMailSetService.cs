using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IIPCMailSetService : IMailSetService
    {
        /// <summary>
        /// 取得所有郵件範本
        /// </summary>
        /// <returns></returns>
        Task<IList<IPCMailSetModel>> GetMailTemplate();

        /// <summary>
        /// 取得郵件範本by ID
        /// </summary>
        /// <param name="MAIL_ID"></param>
        /// <returns></returns>
        Task<IPCMailSetModel> GetMailTemplateById(string MAIL_ID);

        /// <summary>
        /// 儲存郵件範本資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        RtnResultModel SaveMailTemplate(MailSetModel model);
    }
}
