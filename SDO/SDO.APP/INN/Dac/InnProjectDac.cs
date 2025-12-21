using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class InnProjectDac : Dac, IInnProjectDac
    {
        private readonly IUserData user;
        public InnProjectDac(IConnectionControlCenter connectionControlCenter,
           IHttpContextAccessor httpContextAccessor,
           ISqlTrace trace,
           IUserProfile profile,
           IParameterAdaptor ParameterAdaptor,
           IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
            user = profile.GetLoginUser();
        }

        #region 取得創新提案基本資料(提案主題、參與提案人、自定義欄位)
        /// <summary>
        /// 取得創新提案基本資料
        /// </summary>
        /// <param name="projectNo"></param>
        /// <returns></returns>
        public async Task<InnProjectBasicModel> GetProjectInnBasic(string planNo)
        {

            string sql = $@"SELECT 
                                M1.INN_PLAN_ID,
                                M1.INN_YEAR,
                                M1.OU_ID,
                                M1.INN_PLAN_NO,
                                M1.INN_PLAN_NAME,
                                M1.INN_DESCRIPTION,
                                M1.SPONSOR_TYPE,
                                M1.PROPOSAL_TYPE,
                                M1.SUBJECT_TYPE,
                                M1.[GROUP],
                                M1.CUSTOM_TITLE,
                                M1.IDEA_CONTENT,
                                M1.EXPECT_BENEFIT,
                                M1.SPONSOR_ORG,
                                M1.SPONSOR_UNIT,
                                M1.SPONSOR_TITLE,
                                M1.SPONSOR_NAME,
                                M1.SPONSOR_SEX,
                                M1.SPREAD_IDEA_YN,
                                M1.REJECT_YN,
                                M1.REJECT_TYPE,
                                M1.ORIGINATE_YN,
                                M1.CONTACT_NAME,
                                M1.CONTACT_TEL,
                                M1.CONTACT_EMAIL,
                                M2.IS_PLURAL,
	                            M3.PROPOSAL_TYPE_ID AS PROPOSAL_TYPE 
                            FROM 
                                INN_BASIC M1(NOLOCK)
                            INNER JOIN 
	                                INN_ASSIGN_ORG M2 (NOLOCK)
                                ON M1.INN_YEAR = M2.INN_YEAR
                            LEFT JOIN 
                                    INN_PROJECT_PROPOSALTYPE M3 (NOLOCK)
                                ON M1.INN_PLAN_NO = M3.INN_PLAN_NO
                            WHERE    M1.INN_PLAN_NO = @INN_PLAN_NO;";

            return await ExecuteQueryFirstOrDefaultAsync<InnProjectBasicModel>(sql, new { INN_PLAN_NO = planNo }, INNDBKey);
        }

        /// <summary>
        /// 取得自訂欄位
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        public async Task<List<ProjectCusFieldModel>> GetInnProjectCusField(string INN_YEAR)
        {
            string sql = @"SELECT CUS_FIELD_ID,
                                  IS_USE,
                                  CUS_ITEM,
                                  CUS_FIELD_NANE,
                                  YEAR
                           FROM INN_PROJECT_CUS_FIELD (NOLOCK)
						   WHERE YEAR = @INN_YEAR 
                           AND IS_USE = 1";

            return (await ExecuteQueryAsync<ProjectCusFieldModel>(sql, new { INN_YEAR }, INNDBKey)).ToList();
        }


        /// <summary>
        /// 取創新提案主題
        /// </summary>
        /// <param name="planNo"></param>
        /// <returns></returns>
        public async Task<List<InnProjectProposalTypeModel>> GetInnProjectProposalType(string planNo)
        {
            string sql = $@"SELECT
                                   PROPOSAL_TYPE_ID
                            FROM INN_PROJECT_PROPOSALTYPE (NOLOCK)
                            WHERE INN_PLAN_NO = @INN_PLAN_NO 
                                  AND PROPOSAL_KIND = 2";

            return (await ExecuteQueryAsync<InnProjectProposalTypeModel>(sql, new { INN_PLAN_NO = planNo }, INNDBKey)).ToList();
        }

        /// <summary>
        /// 取創新提案自訂欄位
        /// </summary>
        /// <param name="planNo"></param>
        /// <returns></returns>
        public async Task<List<InnProjectCusFieldValueModel>> GetProjectCusFieldValue(string planNo)
        {
            string sql = $@"SELECT ID,
                                   CUS_FIELD_ID,
                                   CUS_FIELD_VALUE
                            FROM INN_PROJECT_CUS_FIELD_VALUE (NOLOCK)
                            WHERE INN_PLAN_NO = @INN_PLAN_NO";

            return (await ExecuteQueryAsync<InnProjectCusFieldValueModel>(sql, new { INN_PLAN_NO = planNo }, INNDBKey)).ToList();
        }

        /// <summary>
        /// 取創新提案參與人
        /// </summary>
        /// <param name="planNO"></param>
        /// <returns></returns>
        public async Task<List<InnPartnerModel>> GetInnPartner(string planNO)
        {
            string sql = $@"SELECT PARTNER_ID,
                                   PARTNER_ORG,
                                   PARTNER_UNIT,
                                   PARTNER_TITLE,
                                   PARTNER_NAME
                            FROM INN_PARTNER (NOLOCK)
                            WHERE INN_PLAN_NO = @INN_PLAN_NO";

            return (await ExecuteQueryAsync<InnPartnerModel>(sql, new { INN_PLAN_NO = planNO }, INNDBKey)).ToList();
        }


        #endregion

        #region AddMdf 創新提案基本資料
        #region 創新提案基本資料
        /// <summary>
        /// 新增創新提案基本資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns>回傳新增後，產生的提案序號</returns>
        public async Task InsertInnProjectBasic(InnProjectBasicModel model)
        {
            string sql = $@"
                INSERT INTO INN_BASIC 
                    (INN_YEAR
                    ,OU_ID
                    ,INN_PLAN_NO
                    ,INN_PLAN_NAME
                    ,INN_DESCRIPTION
                    ,SPONSOR_TYPE
                    ,[GROUP]
                    ,IDEA_CONTENT
                    ,EXPECT_BENEFIT
                    ,SPONSOR_ORG
                    ,SPONSOR_UNIT
                    ,SPONSOR_TITLE
                    ,SPONSOR_NAME
                    ,SPONSOR_SEX
                    ,SPREAD_IDEA_YN
                    ,REJECT_YN
                    ,ORIGINATE_YN
                    ,CONTACT_NAME
                    ,CONTACT_ORG
                    ,CONTACT_TITLE
                    ,CONTACT_EMAIL
                    ,CONTACT_TEL
                    ,CRT_USER
                    ,CRT_DATE
                    ,MDF_USER
                    ,MDF_DATE )
                VALUES
                    (@INN_YEAR
                    ,@OU_ID
                    ,@INN_PLAN_NO
                    ,@INN_PLAN_NAME
                    ,@INN_DESCRIPTION
                    ,@SPONSOR_TYPE
                    ,@GROUP
                    ,@IDEA_CONTENT
                    ,@EXPECT_BENEFIT
                    ,@SPONSOR_ORG
                    ,@SPONSOR_UNIT
                    ,@SPONSOR_TITLE
                    ,@SPONSOR_NAME
                    ,@SPONSOR_SEX
                    ,@SPREAD_IDEA_YN
                    ,@REJECT_YN
                    ,@ORIGINATE_YN
                    ,@CONTACT_NAME
                    ,@CONTACT_ORG
                    ,@CONTACT_TITLE
                    ,@CONTACT_EMAIL
                    ,@CONTACT_TEL
                    ,@CRT_USER
                    ,{DTNow}
                    ,@MDF_USER
                    ,{DTNow} )";
            await ExecuteCommandAsync(sql, model, INNDBKey);
        }

        /// <summary>
        /// 修改創新提案基本資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns>回傳提案序號</returns>
        public async Task UpdateInnProjectBasic(InnProjectBasicModel model)
        {
            string sql = $@"
                UPDATE INN_BASIC
                SET INN_YEAR = @INN_YEAR,
                    INN_PLAN_NAME = @INN_PLAN_NAME,
                    INN_DESCRIPTION = @INN_DESCRIPTION,
                    SPONSOR_TYPE = @SPONSOR_TYPE,
                    [GROUP] = @GROUP,
                    IDEA_CONTENT = @IDEA_CONTENT,
                    EXPECT_BENEFIT = @EXPECT_BENEFIT,
                    SPONSOR_ORG = @SPONSOR_ORG,
                    SPONSOR_UNIT = @SPONSOR_UNIT,
                    SPONSOR_TITLE = @SPONSOR_TITLE,
                    SPONSOR_NAME = @SPONSOR_NAME,
                    SPONSOR_SEX = @SPONSOR_SEX,
                    SPREAD_IDEA_YN = @SPREAD_IDEA_YN,
                    REJECT_YN = @REJECT_YN,
                    ORIGINATE_YN = @ORIGINATE_YN,
                    MDF_USER = @MDF_USER,
                    MDF_DATE = {DTNow}
                WHERE INN_PLAN_NO = @INN_PLAN_NO ";
            await ExecuteCommandAsync(sql, model, INNDBKey);
        }

        #endregion

        #region 創新提案參與提案人
        /// <summary>
        /// 新增創新提案參與提案人
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task InsertInnPartner(List<InnPartnerModel> model)
        {
            string sql = $@"
                INSERT INTO INN_PARTNER
                    (INN_PLAN_NO
                    ,PARTNER_ORG
                    ,PARTNER_UNIT
                    ,PARTNER_TITLE
                    ,PARTNER_NAME
                    ,CRT_USER
                    ,CRT_DATE
                    ,MDF_USER
                    ,MDF_DATE)
                VALUES
                    (@INN_PLAN_NO
                    ,@PARTNER_ORG
                    ,@PARTNER_UNIT
                    ,@PARTNER_TITLE
                    ,@PARTNER_NAME
                    ,@CRT_USER
                    ,{DTNow}
                    ,@MDF_USER
                    ,{DTNow})";

            await ExecuteCommandAsync(sql, model, INNDBKey);
        }

        /// <summary>
        /// 刪除創新提案參與提案人
        /// </summary>
        /// <param name="InnPlanId"></param>
        public async Task DeleteInnPartner(string InnPlanNO)
        {
            string sql = @"
                DELETE FROM INN_PARTNER
                WHERE INN_PLAN_NO = @INN_PLAN_NO";
            await ExecuteCommandAsync(sql, new { INN_PLAN_NO = InnPlanNO }, INNDBKey);
        }
        #endregion

        #region 提案提案類別
        /// <summary>
        /// 新增提案提案類別
        /// </summary>
        /// <param name="model"></param>
        public async Task InsertProjectProposalType(List<InnProjectProposalTypeModel> model)
        {
            string sql = $@"
                INSERT INTO INN_PROJECT_PROPOSALTYPE
                    (INN_PLAN_NO
                    ,PROPOSAL_KIND
                    ,PROPOSAL_TYPE_ID
                    ,CRT_USER
                    ,CRT_DATE
                    ,MDF_USER
                    ,MDF_DATE)
                VALUES
                    (@INN_PLAN_NO
                    ,@PROPOSAL_KIND
                    ,@PROPOSAL_TYPE_ID
                    ,@CRT_USER
                    ,{DTNow}
                    ,@MDF_USER
                    ,{DTNow})";

            await ExecuteCommandAsync(sql, model, INNDBKey);
        }


        /// <summary>
        /// 刪除提案提案類別
        /// </summary>
        /// <param name="buildId"></param>
        public async Task DeleteProposalType(string InnPlanNO)
        {
            string sql = @"
                DELETE FROM INN_PROJECT_PROPOSALTYPE
                WHERE INN_PLAN_NO = @INN_PLAN_NO";
            await ExecuteCommandAsync(sql, new { INN_PLAN_NO = InnPlanNO }, INNDBKey);
        }
        #endregion

        #region 創新提案自定義欄位值
        /// <summary>
        /// 新增創新提案自定義欄位值
        /// </summary>
        /// <param name="model"></param>
        public async Task InsertProjectCusFieldValue(List<InnProjectCusFieldValueModel> model)
        {
            string sql = $@"
                INSERT INTO INN_PROJECT_CUS_FIELD_VALUE
                    (INN_PLAN_NO
                    ,CUS_FIELD_ID
                    ,CUS_FIELD_VALUE
                    ,CRT_USER
                    ,CRT_DATE
                    ,MDF_USER
                    ,MDF_DATE)
                VALUES
                    (@INN_PLAN_NO
                    ,@CUS_FIELD_ID
                    ,@CUS_FIELD_VALUE
                    ,@CRT_USER
                    ,{DTNow}
                    ,@MDF_USER
                    ,{DTNow})";
            await ExecuteCommandAsync(sql, model, INNDBKey);
        }

        /// <summary>
        /// 刪除創新提案自定義欄位值
        /// </summary>
        /// <param name="asstId"></param>
        public async Task DeleteProjectCusFieldValue(string InnPlanNO)
        {
            string sql = @"
                DELETE FROM INN_PROJECT_CUS_FIELD_VALUE
                WHERE INN_PLAN_NO = @INN_PLAN_NO";
            await ExecuteCommandAsync(sql, new { INN_PLAN_NO = InnPlanNO }, INNDBKey);
        }

        /// <summary>
        /// 刪除提案
        /// </summary>
        /// <param name="InnPlanNo"></param>
        public async Task DeleteInnProjectBasic(string InnPlanNo)
        {
            string sql = @"
                DELETE FROM INN_BASIC
                WHERE INN_PLAN_NO = @INN_PLAN_NO";
            await ExecuteCommandAsync(sql, new { INN_PLAN_NO = InnPlanNo }, INNDBKey);
        }

        #endregion
        #endregion



        #region 共用
        /// <summary>
        /// 取得計畫編號流水號
        /// </summary>
        /// <param name="projectNoStart6Char">計畫編號前6碼</param>
        /// <returns></returns>
        public string GetProjectNoSeq(string InnPlanNoFirst6Char)
        {
            string sql = @"SELECT ISNULL(MAX(CAST(RIGHT(INN_PLAN_NO,3) AS INT)),0)+1 AS NUM 
                           FROM INN_BASIC (noLock)
                           WHERE INN_PLAN_NO LIKE  @InnPlanNoFirst6Char + '%'";
            return ExecuteQuery<string>(sql, new { InnPlanNoFirst6Char }, INNDBKey).FirstOrDefault();
        }

        #endregion
    }
}
