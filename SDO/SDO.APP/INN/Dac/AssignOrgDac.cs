using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
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
    public class AssignOrgDac : Dac, IAssignOrgDac
    {
        public AssignOrgDac(IConnectionControlCenter connectionControlCenter,
          IHttpContextAccessor httpContextAccessor,
          ISqlTrace trace,
          IUserProfile profile,
          IParameterAdaptor ParameterAdaptor,
          IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {

        }

        /// <summary>
        /// 取得截止時間
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        public async Task<AssignOrgModel> GetInnAssignOrg(string INN_YEAR)
        {
            string sql = @" SELECT CLOSE_DATE
							FROM INN_ASSIGN_ORG (NOLOCK)
							WHERE INN_YEAR = @INN_YEAR ";

            return await ExecuteQueryFirstOrDefaultAsync<AssignOrgModel>(sql, new { INN_YEAR }, INNDBKey);
        }

        /// <summary>
        /// 新增編輯截止時間
        /// </summary>
        /// <param name="model"></param>
        public async Task AddMdfInnAssignOrgCloseDate(AssignOrgModel model)
        {
            string sql = $@"UPDATE INN_ASSIGN_ORG 
                            SET
                               INN_YEAR = @INN_YEAR,
                               OU_ID = @OU_ID,
                               CLOSE_DATE = @CLOSE_DATE,
                               IS_PLURAL = @IS_PLURAL,
                               MDF_USER = @MDF_USER,
                               MDF_DATE = {DTNow}
                            where INN_YEAR = @INN_YEAR
                            -- 若沒資料則新增
                            IF @@ROWCOUNT = 0
                            BEGIN
                                INSERT INTO INN_ASSIGN_ORG (
                                    INN_YEAR,
                                    OU_ID,
                                    CLOSE_DATE,
                                    IS_PLURAL,
                                    CRT_USER,
                                    CRT_DATE)
                                VALUES(
                                    @INN_YEAR,
                                    @OU_ID,
                                    @CLOSE_DATE,
                                    @IS_PLURAL,
                                    @CRT_USER,
                                    {DTNow})
                            END";
            await ExecuteCommandAsync(sql, model, INNDBKey);
        }

    }
}
