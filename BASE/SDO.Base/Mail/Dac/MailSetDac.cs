using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class MailSetDac : Dac, IMailSetDac
    {
        public MailSetDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        public MailSetDac(IConnectionControlCenter connectionControlCenter,
               ISqlTrace trace,
               IParameterAdaptor parameterAdaptor, 
               IUserData userData,
               IConfiguration configuration) : base(connectionControlCenter, trace, parameterAdaptor, userData, configuration)
        {
        }
        /// <summary>
        /// 查詢所有未被刪除的郵件模板
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IList<MailSetModel>> Read(string userId)
        {
            string sql = @"
                SELECT  
                    MAIL_ID,
                    MAIL_NAME,
                    MAIL_SUBJECT,
                    DEL_FLG
                FROM MAIL_TEMPLATE
                WHERE DEL_FLG=0 ";
            return await ExecuteQueryAsync<MailSetModel>(sql, new { USER_ID = userId });
        }
        /// <summary>
        /// 查詢郵件範本信息
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        public async Task<MailSetModel> ReadById(string mailId)
        {
            string sql = @"
                SELECT  
                    MAIL_ID,
	                MAIL_NAME,
	                MAIL_SUBJECT,
                    MAIL_CONTENT,
	                DEL_FLG
                FROM MAIL_TEMPLATE
                WHERE MAIL_ID = ?MAIL_ID?";
            return (await ExecuteQueryAsync<MailSetModel>(sql, new { MAIL_ID = mailId })).SingleOrDefault();
        }
        /// <summary>
        /// 查詢郵件範本信息(有回傳model)
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        public MailSetModel ReadByIdSync(string mailId)
        {
            string sql = @"
                SELECT  
                    MAIL_ID,
	                MAIL_NAME,
	                MAIL_SUBJECT,
                    MAIL_CONTENT,
	                DEL_FLG
                FROM MAIL_TEMPLATE
                WHERE MAIL_ID = ?MAIL_ID?";
            return (ExecuteQuery<MailSetModel>(sql, new { MAIL_ID = mailId })).SingleOrDefault();
        }
        /// <summary>
        /// 查詢郵件範本收件人
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        public async Task<IList<RecipientModel>> ReadRecipientById(string mailId)
        {
            string sql = @"
                SELECT	
                    MAIL_ID, 
                    MAIL_TYPE, 
                    MAIL_ROLE, 
                    MAIL_ADDRESS, 
                    MAIL_TITLE
                FROM MAIL_RECIPIENT
                WHERE MAIL_ID = ?MAIL_ID?";
            return await ExecuteQueryAsync<RecipientModel>(sql, new { MAIL_ID = mailId });
        }
        /// <summary>
        /// 新增郵件範本
        /// </summary>
        /// <param name="mail"></param>
        /// <returns></returns>
        public async Task Insert(MailSetMdfModel mail)
        {
            string sql = @"
                INSERT INTO	dbo.MAIL_TEMPLATE
                (
	                MAIL_ID, 
                    MAIL_NAME, 
                    MAIL_SUBJECT, 
                    MAIL_CONTENT, 
                    CRT_DATE, 
                    CRT_USER, 
                    MDF_DATE, 
                    MDF_USER
                )
                VALUES (
	                ?MAIL_ID?, 
                    ?MAIL_NAME?, 
                    ?MAIL_SUBJECT?, 
                    ?MAIL_CONTENT?, 
                    DATEADD(HH,8,GETUTCDATE()) , 
                    ?CRT_USER?, 
                    DATEADD(HH,8,GETUTCDATE()) , 
                    ?MDF_USER?
                )";
            await ExecuteCommandAsync(sql, mail);
        }
        /// <summary>
        /// 更新郵件範本
        /// </summary>
        /// <param name="mail"></param>
        /// <returns></returns>
        public async Task Update(MailSetMdfModel mail)
        {
            string sql = @"
                UPDATE	dbo.MAIL_TEMPLATE
                SET		MAIL_NAME = ?MAIL_NAME?, 
		                MAIL_SUBJECT = ?MAIL_SUBJECT?, 
		                MAIL_CONTENT = ?MAIL_CONTENT?,  
		                MDF_DATE = DATEADD(HH,8,GETUTCDATE()) ,
		                MDF_USER = ?MDF_USER? 
                WHERE	[MAIL_ID] = ?MAIL_ID?";
            await ExecuteCommandAsync(sql, mail);
        }
        /// <summary>
        /// 刪除郵件範本
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        public async Task Delete(string mailId)
        {
            string sql = @"
                -- 修改郵件範本設定資料 DEL_FLG = 1
                UPDATE MAIL_TEMPLATE
                SET DEL_FLG = 1
                WHERE   MAIL_ID = ?MAIL_ID?";
            await ExecuteCommandAsync(sql, new { MAIL_ID = mailId });
        }
        /// <summary>
        /// 删除收件人信息
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        public async Task DeleteRecipient(string mailId)
        {
            string sql = @"
                DELETE FROM dbo.MAIL_RECIPIENT
                WHERE MAIL_ID = ?MAIL_ID?";
            await ExecuteCommandAsync(sql, new { MAIL_ID = mailId });
        }
        /// <summary>
        /// 插入收件人信息
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task InsertRecipient(IEnumerable<RecipientModel> models)
        {
            string sql = @"
                INSERT INTO dbo.MAIL_RECIPIENT( 
                    MAIL_ID, 
                    MAIL_TYPE, 
                    MAIL_ROLE, 
                    MAIL_ADDRESS, 
                    MAIL_TITLE 
                ) 
                VALUES(
                    ?MAIL_ID?, 
                    ?MAIL_TYPE?, 
                    ?MAIL_ROLE?, 
                    ?MAIL_ADDRESS?, 
                    ?MAIL_TITLE? )";
            await ExecuteCommandAsync(sql, models);
        }
    }
}
