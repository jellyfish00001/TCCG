using SDO.Dac;
using SDO.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Utils
{
    public class ApiTrace : IApiTrace
    {
        private readonly ITraceDac traceDac;

        public ApiTrace(ITraceDac traceDac)
        {
            this.traceDac = traceDac;
        }

        public async Task AddTrace(ApiTraceModel apiTrace)
        {
            string sql = @"
                INSERT INTO [dbo].[API_TRACE]
                    ([ip]
                    ,[request_header]
                    ,[request_body]
                    ,[request_url]
                    ,[request_type]
                    ,[response_header]
                    ,[response_body]
                    ,[response_code]
                    ,[log_date]
                    ,user_id
                    ,request_desc
                    )
                VALUES
                    (?IP?
                    ,?REQUEST_HEADER?
                    ,?REQUEST_BODY?
                    ,?REQUEST_URL?
                    ,?REQUEST_TYPE?
                    ,?RESPONSE_HEADER?
                    ,?RESPONSE_BODY?
                    ,?RESPONSE_CODE?
                    ,DATEADD(HH,8,GETUTCDATE())
                    ,?user_id?
                    ,?request_desc? )";
            await traceDac.ExecuteCommandAsync(sql, apiTrace);
        }

        public async Task<string> MD5Encode(string userPd)
        {
            string sql = @"SELECT REPLACE(sys.fn_varbintohexstr(HASHBYTES('MD5', ?USER_PD?)), '0x', '')";
            return (await traceDac.ExecuteQueryAsync<string>(sql, new { USER_PD = userPd })).SingleOrDefault();
        }
    }
}
