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
    public class CalendarDac : Dac, ICalendarDac
    {
        public CalendarDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        public async Task<IList<CalendarModel>> Read(string userId, string orgId)
        {
            string sql = @"
                SELECT 
                    CALENDAR_ID,
                    CONVERT(VARCHAR(10), CALENDAR_START_DATE, 126) AS CALENDAR_START_DATE,
	                CONVERT(VARCHAR(10), CALENDAR_END_DATE, 126) AS CALENDAR_END_DATE,
	                CALENDAR_TYPE,
	                CALENDAR_ORG,
	                CALENDAR_TITLE,
	                CALENDAR_CONTENT
                FROM CALENDAR (NOLOCK)
                WHERE CALENDAR_TYPE = 0 OR (CALENDAR_TYPE = 1 AND CALENDAR_ORG = ?ORG_ID?) OR CRT_USER = ?USER_ID?";
            return await ExecuteQueryAsync<CalendarModel>(sql, new { USER_ID = userId, ORG_ID = orgId });
        }

        public async Task<CalendarDetailModel> ReadById(int calendarId, string userId)
        {
            string sql = @"
                SELECT 
                    CALENDAR_ID,
                    CONVERT(VARCHAR(10), CALENDAR_START_DATE, 126) AS CALENDAR_START_DATE,
	                CONVERT(VARCHAR(10), CALENDAR_END_DATE, 126) AS CALENDAR_END_DATE,
	                CALENDAR_TYPE,
	                CALENDAR_ORG,
	                CALENDAR_TITLE,
	                CALENDAR_CONTENT,
                    ORG_NAME,
	                iif(CLD.CRT_USER = ?USER_ID?,1,0) as EDITABLE
                FROM CALENDAR (NOLOCK) AS CLD LEFT JOIN EMP_ORG (NOLOCK) AS ORG ON CLD.CALENDAR_ORG = ORG.ORG_ID
                WHERE CALENDAR_ID = ?CALENDAR_ID?";
            return (await ExecuteQueryAsync<CalendarDetailModel>(sql, new { CALENDAR_ID = calendarId, USER_ID = userId })).SingleOrDefault();
        }

        public async Task Insert(CalendarModel calendar)
        {
            string sql = $@"
                INSERT INTO CALENDAR(
	                CALENDAR_START_DATE,
	                CALENDAR_END_DATE,
	                CALENDAR_TYPE,
	                CALENDAR_ORG,
	                CALENDAR_TITLE,
	                CALENDAR_CONTENT,
	                CRT_DATE,
	                CRT_USER,
	                MDF_DATE,
	                MDF_USER)
                VALUES(?CALENDAR_START_DATE?,
	                ?CALENDAR_END_DATE?,
	                ?CALENDAR_TYPE?,
	                ?CALENDAR_ORG?,
	                ?CALENDAR_TITLE?,
	                ?CALENDAR_CONTENT?,
	                {DTNow},
	                ?CRT_USER?,
	                {DTNow},
	                ?MDF_USER?)";
            await ExecuteCommandAsync(sql, calendar);
        }

        public async Task Update(CalendarModel calendar)
        {
            string sql = $@"
                UPDATE CALENDAR 
                SET CALENDAR_START_DATE = ?CALENDAR_START_DATE?,
	                CALENDAR_END_DATE  = ?CALENDAR_END_DATE?,
	                CALENDAR_TYPE = ?CALENDAR_TYPE?,
	                CALENDAR_ORG = ?CALENDAR_ORG?,
	                CALENDAR_TITLE = ?CALENDAR_TITLE?,
	                CALENDAR_CONTENT = ?CALENDAR_CONTENT?,
	                MDF_DATE = {DTNow},
	                MDF_USER = ?MDF_USER?
                WHERE CALENDAR_ID = ?CALENDAR_ID?";
            await ExecuteCommandAsync(sql, calendar);
        }

        public async Task Delete(int calendarId)
        {
            string sql = @"
                DELETE CALENDAR 
                WHERE CALENDAR_ID = ?CALENDAR_ID?";
            await ExecuteCommandAsync(sql, new CalendarModel() { CALENDAR_ID = calendarId });
        }
    }
}
