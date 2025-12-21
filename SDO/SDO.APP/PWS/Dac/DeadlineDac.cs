using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Base.Utils.Models;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class DeadlineDac : Dac , IDeadlineDac
    {
        public DeadlineDac(
            IConnectionControlCenter connectionControlCenter, 
            IHttpContextAccessor httpContextAccessor, 
            ISqlTrace trace, 
            IUserProfile profile, 
            IParameterAdaptor ParameterAdaptor, 
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }

        /// <summary>
        /// 取計畫年度
        /// </summary>
        /// <returns></returns>
        public async Task<List<DeadlineModel>> GetPlanYear()
        {
            string sql = @"
                           SELECT DISTINCT PLANYEAR
                             FROM PWSSDYEARSET
                            ORDER BY PLANYEAR DESC; 
                          ";

            var result = (await ExecuteQueryAsync<DeadlineModel>(sql, null, PWSDBKey)).ToList();
            return result;
        }

        /// <summary>
        /// 更新機關截止日期
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task SetOrgDeadLine(DeadlineModel model)
        {
            string sql = $@"
                            UPDATE PWSSDASSIGNMENT
                            SET HANDDATEEND = @OrgEndTime,
                                MDF_DATE = {DTNow},
                                MDF_USER = @MDF_USER
                            WHERE PLANYEAR = @PLANYEAR
                            AND TRIM(OU_NAME) NOT LIKE '%區公所'; 
                          ";

            await ExecuteCommandAsync(sql, model, PWSDBKey);
        }

        /// <summary>
        /// 更新區公所截止日期
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task SetDisDeadLine(DeadlineModel model)
        {
            string sql = $@"
                            UPDATE PWSSDASSIGNMENT
                            SET HANDDATEEND = @DistrictHallEndTime,
                                MDF_DATE = {DTNow},
                                MDF_USER = @MDF_USER
                            WHERE PLANYEAR = @PLANYEAR
                            AND TRIM(OU_NAME) LIKE '%區公所'; 
                          ";

            await ExecuteCommandAsync(sql, model, PWSDBKey);
        }

        /// <summary>
        /// 是否有年度
        /// </summary>
        /// <param name="PLANYEAR"></param>
        /// <returns></returns>
        public async Task<bool> IsDeadlineYear(string PLANYEAR)
        {
            string sql = @"
                            SELECT 
                                   PLANYEAR
                              FROM PWSSDASSIGNMENT (NOLOCK)
                              WHERE PLANYEAR = @PLANYEAR 
                           ";
            var result = await ExecuteQueryFirstOrDefaultAsync<string>(sql, new { PLANYEAR }, PWSDBKey);
            return result != null;
        }

        /// <summary>
        /// 新增年度機關
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task AddOrgData(DeadlineModel model)
        {
            string sql = $@"
                          INSERT INTO 
                                      PWSSDASSIGNMENT
                                      (PLANYEAR, OU_ID, OU_NAME)
                               SELECT
                                      @PLANYEAR AS PLANYEAR,
                                      M1.OU_ID AS OU_ID,
                                      M1.OU_NAME AS OU_NAME
                                 FROM {SC30_M}.SCORG_UNITM (NOLOCK) M1
                           INNER JOIN {SC30_M}.GPREL_GRP_GRPM (NOLOCK) M2 
                                   on M1.OU_ID=M2.T_GROUP_ID AND M2.T_GROUP_KIND='SC_OU'
                                WHERE M1.OU_KIND=1
                                      AND M1.OU_NAME <> '-'
                                      AND M1.OU_NAME <>'桃園市政府'
                                      AND LEN(M1.OU_ID) > 7
                                      AND M1.IS_ENABLE = 'Y'
                             GROUP BY M1.OU_ID, M1.OU_NAME, M1.OU_SORT_ORDER
                             ORDER BY M1.OU_SORT_ORDER;

                          ";

            await ExecuteCommandAsync(sql, model, PWSDBKey);
        }

        /// <summary>
        /// 取機關年度截止日
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<DeadlineModel> GetOrgDeadline( string PLANYEAR)
        {
            string sql = @"
                           SELECT DISTINCT HANDDATEEND AS OrgEndTime
                           FROM PWSSDASSIGNMENT
                           WHERE PLANYEAR = @PLANYEAR
                           AND TRIM(OU_NAME) NOT LIKE '%區公所';
                         ";
            var result = await ExecuteQueryFirstOrDefaultAsync<DeadlineModel>(sql, new { PLANYEAR }, PWSDBKey);
            return result;
        }

        /// <summary>
        /// 取區公所年度截止日
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<DeadlineModel> GetDisDeadline(string PLANYEAR)
        {
            string sql = @"
                           SELECT DISTINCT HANDDATEEND AS DistrictHallEndTime
                           FROM PWSSDASSIGNMENT
                           WHERE PLANYEAR = @PLANYEAR
                           AND TRIM(OU_NAME) LIKE '%區公所';
                         ";
            var result = await ExecuteQueryFirstOrDefaultAsync<DeadlineModel>(sql, new { PLANYEAR }, PWSDBKey);
            return result;
        }
    }
}
