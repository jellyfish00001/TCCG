using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class MailLogDac : Dac, IMailLogDac
    {
        public MailLogDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }
        public MailLogDac(IConnectionControlCenter connectionControlCenter,
                  ISqlTrace trace,
                  IParameterAdaptor parameterAdaptor,
                  IUserData userData,
                  IConfiguration configuration) : base(connectionControlCenter, trace, parameterAdaptor, userData, configuration)
        {
        }


        public async Task<IList<MailLogModel>> ReadMailLog(GridBasicQryModel model)
        {
            string sql = @"
                SELECT 
                    COUNT(*) OVER() AS DATA_COUNT,
                            MAIL_SENDER,
                            MAIL_RECEIVER,
                            CC_RECEIVER,
                            BCC_RECEIVER,
                            MAIL_SUBJECT,
                            MAIL_CONTENT,
                            SEND_FLG,
                            REPLACE(CONVERT(VARCHAR(100), CRT_DATE, 120),'-','/') AS [LOG_DATE]
                FROM dbo.MAIL_LOG 
                WHERE FORMAT(CRT_DATE, 'yyyy-MM-dd') BETWEEN ?START_DATE? AND ?END_DATE?
                ORDER BY CRT_DATE DESC 
                    OFFSET ?PAGE_NO? ROWS
	                FETCH NEXT ?PAGE_SIZE? ROWS ONLY";
            return await ExecuteQueryAsync<MailLogModel>(sql, model);
        }

        public async Task<bool> WriteMailLog(MailLogModel model)
        {
            string sql = @"INSERT MAIL_LOG(
                                MAIL_SENDER,
                                MAIL_RECEIVER,
                                CC_RECEIVER,
                                BCC_RECEIVER,
                                MAIL_SUBJECT,
                                MAIL_CONTENT,
                                MAIL_ID,
                                PROJECT_NO,
                                SEND_FLG,
                                CRT_DATE)
                            VALUES(
                                ?MAIL_SENDER?,
                                ?MAIL_RECEIVER?,
                                ?CC_RECEIVER?,
                                ?BCC_RECEIVER?,
                                ?MAIL_SUBJECT?,
                                ?MAIL_CONTENT?,
                                ?MAIL_ID?,
                                ?PROJECT_NO?,
                                ?SEND_FLG?,
                                DATEADD(HH,8,GETUTCDATE()))";
            await ExecuteCommandAsync(sql, model);
            return true;
        }

        public bool WriteMailLogSync(MailLogModel model)
        {
            string sql = @"INSERT MAIL_LOG(
                                MAIL_SENDER,
                                MAIL_RECEIVER,
                                CC_RECEIVER,
                                BCC_RECEIVER,
                                MAIL_SUBJECT,
                                MAIL_CONTENT,
                                MAIL_ID,
                                PROJECT_NO,
                                SEND_FLG,
                                CRT_DATE)
                           VALUES(
                                ?MAIL_SENDER?,
                                ?MAIL_RECEIVER?,
                                ?CC_RECEIVER?,
                                ?BCC_RECEIVER?,
                                ?MAIL_SUBJECT?,
                                ?MAIL_CONTENT?,
                                ?MAIL_ID?,
                                ?PROJECT_NO?,
                                ?SEND_FLG?,
                                DATEADD(HH,8,GETUTCDATE()))";
            ExecuteCommand(sql, model);
            return true;
        }
    }
}
