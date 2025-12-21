using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class IPCMailSetDac : Dac, IIPCMailSetDac
    {
        public IPCMailSetDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 取得所有郵件範本
        /// </summary>
        /// <returns></returns>
        public async Task<IList<IPCMailSetModel>> GetMailTemplate()
        {
            string sql = @"SELECT MAIL_ID,
                                MAIL_NAME,
                                MAIL_SUBJECT,
                                MAIL_TYPE,
                                DEL_FLG
                            FROM MAIL_TEMPLATE (NOLOCK)";
            return await ExecuteQueryAsync<IPCMailSetModel>(sql);
        }

        /// <summary>
        /// 取得郵件範本 by ID
        /// </summary>
        /// <param name="MAIL_ID"></param>
        /// <returns></returns>
        public async Task<IPCMailSetModel> GetMailTemplateById(string MAIL_ID)
        {
            string sql = @"SELECT MAIL_ID,
                                MAIL_NAME,
                                MAIL_SUBJECT,
                                MAIL_CONTENT,
                                MAIL_MEMO,
                                DEL_FLG
                            FROM MAIL_TEMPLATE (NOLOCK)
                            WHERE MAIL_ID = @MAIL_ID";
            return (await ExecuteQueryAsync<IPCMailSetModel>(sql, new { MAIL_ID })).FirstOrDefault();
        }

        /// <summary>
        /// 更新郵件範本
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void UpdateMailTemplate(MailSetModel model)
        {
            string sql = $@"UPDATE MAIL_TEMPLATE
                           SET MAIL_NAME = @MAIL_NAME,
                               MAIL_SUBJECT = @MAIL_SUBJECT,
                               MAIL_CONTENT = @MAIL_CONTENT,
                               DEL_FLG = @DEL_FLG,
                               MDF_USER = @MDF_USER,
                               MDF_DATE = {DTNow}
                           WHERE MAIL_ID = @MAIL_ID";
            ExecuteCommand(sql, model);
        }
    }
}
