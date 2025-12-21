using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class UploadFileDac : Dac, IUploadFileDac
    {
        public UploadFileDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        public async Task<UploadFileModel> GetByFileSeqNo(int fileSeqNo)
        {
            string sql = @"
                SELECT
                    [FILE_SEQ_NO]
                    ,[USER_ID]
                    ,[FILE_UID]
                    ,[FILE_NAME]
                    ,[FILE_EXTENSION]
                    ,[CRT_USER]
                    ,[CRT_IP]
                    ,[CRT_DATE]
                FROM UPLOAD_FILE_DATA (NOLOCK)
                WHERE FILE_SEQ_NO = ?FILE_SEQ_NO?";
            return (await ExecuteQueryAsync<UploadFileModel>(sql, new { FILE_SEQ_NO = fileSeqNo })).FirstOrDefault();
        }

        public async Task<int> Insert(UploadFileModel model)
        {
            string sql = @"
                INSERT INTO [dbo].[UPLOAD_FILE_DATA]
                    ([USER_ID]
                    ,[FILE_UID]
                    ,[FILE_NAME]
                    ,[FILE_EXTENSION]
                    ,[CRT_USER]
                    ,[CRT_IP]
                    ,[CRT_DATE])
                OUTPUT Inserted.FILE_SEQ_NO
                VALUES
                    (?USER_ID?
                    ,?FILE_UID?
                    ,?FILE_NAME?
                    ,?FILE_EXTENSION?
                    ,?CRT_USER?
                    ,?CRT_IP?
                    ,DATEADD(HH,8,GETUTCDATE()) )";
            return (await ExecuteQueryAsync<int>(sql, model)).FirstOrDefault();
        }

        public void Delete(string fileSeqNo)
        {
            string sql = @"
                DELETE UPLOAD_FILE_DATA 
                WHERE FILE_SEQ_NO = ?fileSeqNo?";
            ExecuteCommand(sql, new { fileSeqNo });
        }
    }
}
