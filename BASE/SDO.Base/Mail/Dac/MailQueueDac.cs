using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class MailQueueDac : Dac, IMailQueueDac
    {
        public MailQueueDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        public MailQueueDac(IConnectionControlCenter connectionControlCenter,
                  ISqlTrace trace,
                  IParameterAdaptor parameterAdaptor, 
                  IUserData userData,
                  IConfiguration configuration) : base(connectionControlCenter, trace, parameterAdaptor, userData, configuration)
        {
        }

        public async Task<IList<MailQueueQryResultModel>> Read()
        {
            string sql = @"
                SELECT  
                    QUEUE_ID,
	                MAIL_SUBJECT,
                    MAIL_FROM,
	                SEND_FLG,
                    SEND_TIME,
                    CRT_DATE
                FROM MAIL_QUEUE";
            return await ExecuteQueryAsync<MailQueueQryResultModel>(sql);
        }

        public async Task<MailQueueQryResultModel> ReadById(string queueId)
        {
            string sql = @"
                SELECT  
                    QUEUE_ID,
                    MAIL_SUBJECT,
                    MAIL_CONTENT,
                    MAIL_FROM,
                    MAIL_TO,
                    SEND_FLG
                FROM MAIL_QUEUE
                WHERE QUEUE_ID = ?QUEUE_ID?";
            return (await ExecuteQueryAsync<MailQueueQryResultModel>(sql, new { QUEUE_ID = queueId })).FirstOrDefault();
        }

        public async Task<MailQueueQryResultModel> ReadMailTemplateById(string mailId)
        {
            // MAIL_TYPE = 0 : 寄件者
            string sql = @"
                SELECT 
                    mt.MAIL_SUBJECT,
                    mt.MAIL_CONTENT, 
                    '{""MAIL_NAME"":""' + mr.MAIL_TITLE + '"",""MAIL_ADDR"":""' + mr.MAIL_ADDRESS + '""}' AS MAIL_FROM
                FROM MAIL_TEMPLATE mt
                    LEFT JOIN MAIL_RECIPIENT mr
                    ON mt.MAIL_ID = mr.MAIL_ID AND mr.MAIL_TYPE = '0'
                WHERE mt.MAIL_ID = ?MAIL_ID? AND mt.DEL_FLG = 0";
            return (await ExecuteQueryAsync<MailQueueQryResultModel>(sql, new { MAIL_ID = mailId })).FirstOrDefault();
        }

        public async Task Insert(MailQueueQryResultModel model)
        {
            string sql = @"
                INSERT INTO	dbo.MAIL_QUEUE
                (
	                MAIL_SUBJECT, 
                    MAIL_CONTENT, 
                    CRT_DATE, 
                    CRT_USER, 
                    MDF_DATE, 
                    MDF_USER, 
                    MAIL_FROM, 
                    MAIL_TO, 
                    SEND_FLG, 
                    MAIL_CC, 
                    MAIL_BCC
                ) VALUES (
	                ?MAIL_SUBJECT?, 
                    ?MAIL_CONTENT?, 
                    DATEADD(HH,8,GETUTCDATE()) , 
                    ?CRT_USER?, 
                    DATEADD(HH,8,GETUTCDATE()) , 
                    ?MDF_USER?, 
                    ?MAIL_FROM?, 
                    ?MAIL_TO?, 
                    0, 
                    '',
                    '' )";
            await ExecuteCommandAsync(sql, model);
        }

        public async Task Update(MailQueueQryResultModel model)
        {
            string sql = @"
                UPDATE	dbo.MAIL_QUEUE
                SET MAIL_FROM = ?MAIL_FROM?,
                    MAIL_TO = ?MAIL_TO?,
                    MAIL_SUBJECT = ?MAIL_SUBJECT?,
                    MAIL_CONTENT = ?MAIL_CONTENT?,
                    MDF_DATE = DATEADD(HH,8,GETUTCDATE()) ,
                    MDF_USER = ?MDF_USER?
                WHERE QUEUE_ID = ?QUEUE_ID?";
            await ExecuteCommandAsync(sql, model);
        }

        public async Task Delete(string queueId)
        {
            string sql = @"
                DELETE FROM dbo.MAIL_QUEUE
                WHERE QUEUE_ID = ?QUEUE_ID?";
            await ExecuteCommandAsync(sql, new { QUEUE_ID = queueId });
        }
    }
}
