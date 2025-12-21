using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class ReviewListDac : Dac, IReviewListDac
    {
        public ReviewListDac(
            IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 查詢先期計畫資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ReviewListModel>> GetPWSReviewList(ReviewListQueryModel model)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@"
                           SELECT 
                                  P.PLANNO,
                                  P.OU_ID,
                                  P.PLANYEAR,
                                  P.PLANNO,
                                  P.PLANKIND,
                                  P.SEND_STATUS,
                                  PK.SET_VALUE AS PLANKINDNAME,
                                  P.PLANNAME,
                                  P.CREATEUNITOUID,
                                  P.CREATEORGOUID,
                                  P.PLANORDERNUMBER,
                                  P.PLANDATETYPE,
                                  dbo.FN_GetOuName(P.CREATEUNITOUID, 3) AS UNITOUNAME,
                                  dbo.FN_GetOuName(P.OU_ID, 3) AS ORGOUNAME,
                                  PS.SET_VALUE AS SENDTYPE
                            FROM  PWSSDPLANMAIN P (NOLOCK)
                            LEFT JOIN SET_PARAM PK ON P.PLANKIND = PK.SET_TYPE AND PK.SET_ITEM = 'PLAN_KIND' 
                            LEFT JOIN SET_PARAM PS ON P.SEND_STATUS = PS.SET_TYPE AND PS.SET_ITEM = 'AUDIT_STATUS' 
                            WHERE P.IS_SEND = '1'");

            // 年分
            if (model.PLANYEAR > 0)
            {
                sql.AppendLine(" AND PLANYEAR = @PLANYEAR");
            }
            // 模糊分段查詢部分
            // 計畫編號
            sql.AppendLine(FuzzySearch(model.PLANNO, "PLANNO"));
            // 計畫名稱
            sql.AppendLine(FuzzySearch(model.PLANNAME, "PLANNAME"));
            // 計畫狀態
            if (!string.IsNullOrEmpty(model.SEND_STATUS.ToString()) )
            {
                sql.AppendLine(" AND SEND_STATUS = @SEND_STATUS");
            }
            // 計畫機關
            if (!string.IsNullOrEmpty(model.OU_ID))
            {
                sql.AppendLine(" AND OU_ID = @OU_ID");
            }
            // 計畫類別
            if (!string.IsNullOrEmpty(model.PLANKIND))
            { 
                sql.AppendLine(" AND PLANKIND = @PLANKIND");
            }

            var result = (await ExecuteQueryAsync<ReviewListModel>(sql.ToString(), model, PWSDBKey)).ToList();
            return result;
        }

        /// <summary>
        /// 退回先期計畫資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task SetPWSProject(List<ReviewListModel> model)
        {
            string sql = $@"
                            UPDATE PWSSDPLANMAIN
                            SET SEND_STATUS = '0',
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE PLANNO = @PLANNO
                        ";

            await ExecuteCommandAsync(sql, model, PWSDBKey);
        }

    }
}
