using SDO.CryptSet;
using SDO.Dac;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class IPCMailSetService : MailSetService, IIPCMailSetService
    {
        private readonly IIPCMailSetDac dac;
        public IPCMailSetService(IMailSetDac mailSetDac, IUserProfile userProfile, ISetParamService setParam, IMailLogService mailLog, IEncryptService encrypt, IIPCMailSetDac dac) : base(mailSetDac, userProfile, setParam, mailLog, encrypt)
        {
            this.dac = dac;
        }

        public IPCMailSetService(IMailSetDac mailSetDac, ISetParamService setParam, IMailLogService mailLog, IEncryptService encrypt, IIPCMailSetDac dac) : base(mailSetDac, setParam, mailLog, encrypt)
        {
            this.dac = dac;
        }

        public IPCMailSetService(IMailSetDac mailSetDac, ISetParamService setParam, IMailLogService mailLog, IIPCMailSetDac dac) : base(mailSetDac, setParam, mailLog)
        {
            this.dac = dac;
        }

        /// <summary>
        /// 取得所有郵件範本
        /// </summary>
        /// <returns></returns>
        public async Task<IList<IPCMailSetModel>> GetMailTemplate()
        {
            return await dac.GetMailTemplate();
        }

        /// <summary>
        /// 取得郵件範本 by ID
        /// </summary>
        /// <param name="MAIL_ID"></param>
        /// <returns></returns>
        public async Task<IPCMailSetModel> GetMailTemplateById(string MAIL_ID)
        {
            return await dac.GetMailTemplateById(MAIL_ID) ?? new IPCMailSetModel();
        }

        /// <summary>
        /// 儲存郵件範本資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public RtnResultModel SaveMailTemplate(MailSetModel model)
        {
            dac.BeginTransaction();
            dac.UpdateMailTemplate(model);
            dac.Commit();
            return ChangeResult(true, "存檔成功");
        }
    }
}
