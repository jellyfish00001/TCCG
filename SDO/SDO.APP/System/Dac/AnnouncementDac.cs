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
    public class AnnouncementDac : Dac, IAnnouncementDac
    {
        public AnnouncementDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        public async Task<string[]> GetOrgIdList(string ORG_ID)
        {
            var sql = @"DECLARE @ORG_LEVEL CHAR(1) 
                        SET @ORG_LEVEL = (SELECT ORG_LEVEL FROM EMP_ORG WHERE ORG_ID = ?ORG_ID?)
                        SELECT ORG_ID FROM EMP_ORG (NOLOCK)
                        WHERE (@ORG_LEVEL = '2' AND ORG_ID = ?ORG_ID?)
	                        OR (@ORG_LEVEL = '1' AND (ORG_ID = ?ORG_ID? OR PARENT_ID = ?ORG_ID?))";
            return (await ExecuteQueryAsync<string>(sql, new { ORG_ID = ORG_ID })).ToArray();
        }

        public async Task<IList<AnnouncementModel>> Read(AnnouncementQryModel model)
        {
            string sql = @"SELECT 
	                           A.SID, 
	                           A.TITLE, 
	                           A.COMMENT, 
	                           A.EFFECTIVE_DATE, 
	                           A.EXPIRE_DATE, 
	                           A.ATTACH_NAME, 
	                           A.OFF_DOC,
                               A.MDF_DATE,
                               A.MDF_USER
                           FROM ANNOUNCEMENT A (NOLOCK)
                           WHERE A.DEL_FLG = 0 
                           ORDER BY A.EFFECTIVE_DATE DESC";
            return await ExecuteQueryAsync<AnnouncementModel>(sql, model);
        }

        /// <summary>
        /// 查詢期間內公告
        /// 依時間反排序
        /// </summary>
        /// <param name="annType"></param>
        /// <returns></returns>
        public async Task<IList<AnnouncementModel>> ReadDisplay(string annType)
        {
            string sql = $@"
                SELECT SID, TITLE, COMMENT, EFFECTIVE_DATE, EXPIRE_DATE, ATTACH_NAME
                FROM dbo.ANNOUNCEMENT (NOLOCK)
                WHERE DEL_FLG = 0 
                AND CONVERT(VARCHAR(10), {DTNow} , 111) BETWEEN EFFECTIVE_DATE AND CONVERT(VARCHAR(10), EXPIRE_DATE, 111) 
                ORDER BY EFFECTIVE_DATE DESC";
            return await ExecuteQueryAsync<AnnouncementModel>(sql, new { ANN_TYPE = annType });
        }

        /// <summary>
        /// 查詢公告內容
        /// </summary>
        /// <param name="sId"></param>
        /// <returns></returns>
        public async Task<AnnouncementModel> ReadById(string sId)
        {
            string sql = @"
                SELECT SID, TITLE, COMMENT, EFFECTIVE_DATE, EXPIRE_DATE, ATTACH_NAME, OFF_DOC
                FROM dbo.ANNOUNCEMENT (NOLOCK)
                WHERE SID = @SID 
                AND DEL_FLG = 0";
            return (await ExecuteQueryAsync<AnnouncementModel>(sql, new { SID = sId })).FirstOrDefault();
        }

        /// <summary>
        /// 新增公告
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task Insert(AnnouncementModel model)
        {
            string sql = $@"
                INSERT INTO ANNOUNCEMENT ( 
                        TITLE,
                        COMMENT,
                        EFFECTIVE_DATE,
                        EXPIRE_DATE,
                        ATTACH_NAME,
                        CRT_DATE,
                        CRT_USER,
                        MDF_DATE,
                        MDF_USER,
                        OFF_DOC)
                SELECT 
                        ?TITLE?,
                        ?COMMENT?,
                        ?EFFECTIVE_DATE?,
                        ?EXPIRE_DATE?,
                        ?ATTACH_NAME?,
                        {DTNow},
                        ?CRT_USER?,
                        {DTNow},
                        ?MDF_USER?,
                        ?OFF_DOC?
                        FROM  VW_USER_ORG (NOLOCK)
                        WHERE USER_ID = ?USER_ID?";
            await ExecuteCommandAsync(sql, model);
        }

        /// <summary>
        /// 更新公告
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task Update(AnnouncementModel model)
        {
            string sql = $@"
                UPDATE dbo.ANNOUNCEMENT 
                SET TITLE = ?TITLE?,
                    COMMENT = ?COMMENT?,
                    EFFECTIVE_DATE = ?EFFECTIVE_DATE?,
                    EXPIRE_DATE = ?EXPIRE_DATE?,
                    ATTACH_NAME = ?ATTACH_NAME?,
                    OFF_DOC = ?OFF_DOC?,
                    MDF_DATE = {DTNow},
                    MDF_USER = ?MDF_USER?
                WHERE SID = ?SID?";
            await ExecuteCommandAsync(sql, model);
        }

        /// <summary>
        /// 刪除公告
        /// </summary>
        /// <param name="sId"></param>
        /// <returns></returns>
        public async Task Delete(int sId)
        {
            string sql = $@"
                UPDATE dbo.ANNOUNCEMENT 
                SET DEL_FLG = 1, 
                    MDF_DATE = {DTNow}, 
                    MDF_USER = ?MDF_USER?
                WHERE SID = ?SID?";
            await ExecuteCommandAsync(sql, new { SID = sId, MDF_USER = UserId });
        }

        public async Task<bool> IsUserSysAdmin(string USER_ID)
        {
            string sql = @"SELECT ROLE_ID 
                           FROM MAP_USER_ROLE (NOLOCK) 
                           WHERE ROLE_ID = 'SysAdmin' 
                           AND USER_ID = ?USER_ID?";
            string result = (await ExecuteQueryAsync<string>(sql, new { USER_ID = USER_ID })).FirstOrDefault();
            return !string.IsNullOrEmpty(result);
        }

        public async Task<SetParamModel[]> GetAnnType()
        {
            string sql = @"SELECT SET_TYPE, 
                                  SET_VALUE 
                           FROM SET_PARAM (NOLOCK) 
                           WHERE SET_ITEM = 'AnnouceType'";
            return (await ExecuteQueryAsync<SetParamModel>(sql)).ToArray();
        }

        #region 最新公告(原系統)
        /// <summary>
        /// 取得公告
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns></returns>
        public async Task<List<AnnouncementModel>> GetScAnnouncement(DateTime date)
        {
            string sql = @"select 
	                            SDATE EFFECTIVE_DATE,
	                            EDATE EXPIRE_DATE,
                                ACMT_TITLE TITLE,
                                ACMT_DESC COMMENT
                            from SCANNOUNCEMENTM
                            where @date between SDATE and EDATE 
	                            and SOURCE = 'Y'
                            order by ACMT_CLASS asc, 
	                            ACMT_TIME desc";
            return (await ExecuteQueryAsync<AnnouncementModel>(sql, param: new { date }, DB: SCDBKey)).ToList();
        }
        #endregion 最新公告(原系統)

    }
}
