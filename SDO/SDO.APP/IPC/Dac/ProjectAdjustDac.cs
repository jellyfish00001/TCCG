using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class ProjectAdjustDac : Dac, IProjectAdjustDac
    {
		public ProjectAdjustDac(IConnectionControlCenter connectionControlCenter,
           IHttpContextAccessor httpContextAccessor,
           ISqlTrace trace,
           IUserProfile profile,
           IParameterAdaptor ParameterAdaptor,
           IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
		}

        #region 調整撤銷清單

        /// <summary>
        /// 取得計畫調整撤銷清單
        /// </summary>
        /// <param name="model">篩選條件</param>
        /// <returns></returns>
        public async Task<List<ProjectAdjustListModel>> GetAdjustList(ProjectAdjustListQueryModel model)
		{
			// 若需篩選特殊加註資料，則另外Join特殊加註Table資料
			string specNoteSql = string.IsNullOrEmpty(model.SPEC_NOTE) ? "" : GetSpecNoteJoinSql();

			// 排序: 填報頁面為申請或退回的計畫靠前，再來是審核中的計畫；審核頁面反之，先審核再申請或退回的計畫
			string orderExecProj = model.IsReview == 0 ? "0" : "1";
			string orderReviewProj = model.IsReview == 0 ? "1" : "0";

			string sql = @$" SELECT a.PROJ_ADJ_ID
								,a.AW_KIND
								,a.PROJECT_NO
								,b.PROJECT_NAME
								,a.PROJECT_YEAR
								,dbo.FN_GET_PROJECT_EXS(a.PROJECT_NO, 'T') AS BUDGET
								,dbo.FN_GetSetParam('PROJECT_AW_STATUS', a.PROJECT_AW_STATUS) AS PROJECT_AW_STATUS_NAME
								,a.PROJECT_AW_STATUS
								,b.MASTER_ORGAN_C AS MASTER_ORG_C
								,b.EXEC_ORGAN_C AS EXEC_ORG_C
								,dbo.FN_GetOuName(b.MASTER_ORGAN_C, 3) AS MASTER_ORG_NAME
								,SCForExecOrg.OU_NAME AS EXEC_ORG_NAME
								,SCForExecOrg.OU_SORT_ORDER AS EXEC_ORG_ORDER
								,a.MDF_DATE
							FROM PROJECT_BASIC_ADJ(NOLOCK) a
							LEFT JOIN PROJECT_BASIC(NOLOCK) b ON a.PROJECT_NO = b.PROJECT_NO
							LEFT JOIN {SC30_M}.SCORG_UNITM SCForExecOrg ON b.EXEC_ORGAN_C = SCForExecOrg.OU_ID 
							{specNoteSql}
							WHERE b.EXEC_ORGAN_C IS NOT NULL
                                AND b.EXEC_ORGAN_C != ''
								{GetAdjustListQueryConditions(model)}
							ORDER BY CASE 
									WHEN a.PROJECT_AW_STATUS IN ('A01', 'A03', 'B01', 'B03', 'W03') THEN {orderExecProj}
									WHEN a.PROJECT_AW_STATUS IN ('A02' ,'B02' ,'W02') THEN {orderReviewProj}
									ELSE 2 END
								,a.PROJECT_NO DESC
								,a.PROJ_ADJ_ID DESC";

            return (await ExecuteQueryAsync<ProjectAdjustListModel>(sql, model, MainDBKey)).ToList();
		}

		/// <summary>
		/// 取得計畫列表查詢條件
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		private static string GetAdjustListQueryConditions(ProjectAdjustListQueryModel model)
		{
			StringBuilder sb = new();
			// 計畫年度
			if (model.PROJECT_YEAR != 0)
			{
				sb.Append(" and b.PROJECT_YEAR = @PROJECT_YEAR");
			}
			// 計畫編號
			if (!string.IsNullOrEmpty(model.PROJECT_NO))
			{
				List<string> projNameSqls = model.PROJECT_NO
					.Split(new char[] { ',', ' ', '　' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(x => $"b.PROJECT_NO like '%{x}%'")
					.ToList();

				if (projNameSqls.Any())
				{
					sb.AppendLine($"and ({string.Join(" or ", projNameSqls)})");
				}
			}
			// 計畫名稱
			if (!string.IsNullOrEmpty(model.PROJECT_NAME))
			{
				List<string> projNameSqls = model.PROJECT_NAME
					.Split(new char[] { ',', ' ', '　' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(x => $"b.PROJECT_NAME like '%{x}%'")
					.ToList();

				if (projNameSqls.Any())
				{
					sb.AppendLine($"and ({string.Join(" or ", projNameSqls)})");
				}
			}
			// 撤銷審核狀態
			if (model.PROJECT_AW_STATUS != null && model.PROJECT_AW_STATUS.Any())
			{
				sb.Append(" and a.PROJECT_AW_STATUS in @PROJECT_AW_STATUS");
			}
			// 執行方式類別
			if (!string.IsNullOrEmpty(model.CP_KIND))
			{
				sb.Append(" and b.CP_KIND = @CP_KIND");
			}
			// 執行方式名稱
			if (!string.IsNullOrEmpty(model.RUNWAY_C))
			{
				sb.Append(" and b.RUNWAY_C = @RUNWAY_C");
			}
			// 特殊加註
			if (!string.IsNullOrEmpty(model.SPEC_NOTE))
			{
				sb.Append(" and map.SET_TYPE = @SPEC_NOTE");
			}
			// 執行機關
			if (!string.IsNullOrEmpty(model.EXEC_ORGAN_C))
			{
				sb.Append(" and b.EXEC_ORGAN_C = @EXEC_ORGAN_C");
			}
			return sb.ToString();
		}

		/// <summary>
		/// 取得特殊加註Join Table Sql
		/// </summary>
		/// <returns></returns>
		private static string GetSpecNoteJoinSql()
		{
			return @" left join PROJECT_MAPPING_DATA(NOLOCK) map
                         on a.PROJECT_NO = map.PROJECT_NO and map.SET_ITEM = 'SPEC_NOTE' ";
		}

		#endregion

		/// <summary>
		/// 新增計畫調整檔 (for 計畫撤銷)
		/// </summary>
		/// <param name="model"></param>
		/// <returns>新增的調整流水號</returns>
		public int InsertProjectBasicAdjForRevoke(AdjustReasonModel model)
        {
            string sql = $@"INSERT INTO PROJECT_BASIC_ADJ (
								PROJECT_NO
								,PROJECT_NAME
								,PROJECT_YEAR
								,AW_KIND
								,APPRV_DATE
								,ADJUST_REASON
								,OTHER_REASON
								,PROJECT_AW_STATUS
								,MASTER_ORGAN_C
								,EXEC_ORGAN_C
								,CRT_USER
								,CRT_DATE
								,MDF_USER
								,MDF_DATE
								)
							SELECT @PROJECT_NO
								,@PROJECT_NAME
								,PROJECT_YEAR
								,@AW_KIND
								,@APPRV_DATE
								,@ADJUST_REASON
								,@OTHER_REASON
								,'W02'
								,MASTER_ORGAN_C
								,EXEC_ORGAN_C
								,@CRT_USER
								,{DTNow}
								,@MDF_USER
								,{DTNow}
							FROM PROJECT_BASIC
							WHERE PROJECT_NO = @PROJECT_NO
							SELECT SCOPE_IDENTITY(); ";
            return ExecuteQuery<int>(sql, model, isSetUSER: true).FirstOrDefault();
        }

        /// <summary>
        /// 修改計畫調整檔
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectBasicAdj(AdjustReasonModel model)
        {
            string sql = $@"UPDATE PROJECT_BASIC_ADJ
                            SET PROJECT_NO = @PROJECT_NO
                                ,PROJECT_NAME = @PROJECT_NAME
	                            ,AW_KIND = @AW_KIND
	                            ,APPRV_DATE = @APPRV_DATE
	                            ,ADJUST_REASON = @ADJUST_REASON
	                            ,OTHER_REASON = @OTHER_REASON
	                            ,MDF_USER = @MDF_USER
	                            ,MDF_DATE = {DTNow}
                            WHERE PROJ_ADJ_ID = @PROJ_ADJ_ID";
            ExecuteCommand(sql, model);
        }

		/// <summary>
		/// 刪除計畫調整檔 (修改 DEL_FLG = 1，並改 PROJECT_AW_STATUS)
		/// </summary>
		/// <param name="PROJ_ADJ_ID">調整檔流水號</param>
		/// <param name="PROJECT_AW_STATUS">計畫調整狀態</param>
		public void DeleteProjBasicAdj(int PROJ_ADJ_ID, string PROJECT_AW_STATUS)
		{
			string sql = $@"UPDATE PROJECT_BASIC_ADJ
							SET DEL_FLG = 1
								,PROJECT_AW_STATUS = @PROJECT_AW_STATUS
								,MDF_USER = @UserId
								,MDF_DATE = {DTNow}
							WHERE PROJ_ADJ_ID = @PROJ_ADJ_ID ";
			ExecuteCommand(sql, new { PROJ_ADJ_ID, PROJECT_AW_STATUS, UserId });
		}

		/// <summary>
		/// 修改計畫主檔狀態
		/// </summary>
		/// <param name="PROJECT_NO">列管編號</param>
		/// <param name="PROJECT_AW_STATUS">調整撤銷狀態</param>
		/// <param name="PROJECT_STATUS">計畫狀態</param>
		public void UpdateProjBasicStatuses(string PROJECT_NO, string PROJECT_AW_STATUS, string PROJECT_STATUS = null)
        {
            string sql = $@"UPDATE PROJECT_BASIC
                            SET PROJECT_AW_STATUS = @PROJECT_AW_STATUS,
								{(PROJECT_STATUS != null ? "PROJECT_STATUS = @PROJECT_STATUS," : "")}
                                MDF_USER = @UserId,
                                MDF_DATE = {DTNow}
                            WHERE PROJECT_NO = @PROJECT_NO ";
            ExecuteCommand(sql, new { PROJECT_NO, PROJECT_AW_STATUS, PROJECT_STATUS, UserId });
        }

		/// <summary>
		/// 修改計畫調整檔 調整檔狀態
		/// </summary>
		/// <param name="PROJ_ADJ_ID">調整流水號</param>
		/// <param name="PROJECT_AW_STATUS">調整撤銷狀態</param>
		public void UpdateProjBasicAdjAwSatus(int PROJ_ADJ_ID, string PROJECT_AW_STATUS)
		{
			string sql = $@"UPDATE PROJECT_BASIC_ADJ
                            SET PROJECT_AW_STATUS = @PROJECT_AW_STATUS,
                                MDF_USER = @UserId,
                                MDF_DATE = {DTNow}
                            WHERE PROJ_ADJ_ID = @PROJ_ADJ_ID ";
			ExecuteCommand(sql, new { PROJ_ADJ_ID, PROJECT_AW_STATUS, UserId });
		}

		/// <summary>
		/// 取得計畫狀態
		/// </summary>
		/// <param name="PROJECT_NO">列管編號</param>
		/// <returns>計畫狀態</returns>
		public async Task<string> GetProjectStatus(string PROJECT_NO)
		{
			string sql = @" SELECT PROJECT_STATUS
							FROM PROJECT_BASIC
							WHERE PROJECT_NO = @PROJECT_NO ";
			return await ExecuteQueryFirstOrDefaultAsync<string>(sql, new { PROJECT_NO });
		}

		/// <summary>
		/// 取得計畫調整狀態
		/// </summary>
		/// <param name="PROJECT_NO">列管編號</param>
		/// <returns>計畫調整狀態 PROJECT_AW_STATUS</returns>
		public async Task<string> GetProjectAwStatus(string PROJECT_NO)
		{
			string sql = @" SELECT PROJECT_AW_STATUS
							FROM PROJECT_BASIC
							WHERE PROJECT_NO = @PROJECT_NO ";
			return await ExecuteQueryFirstOrDefaultAsync<string>(sql, new { PROJECT_NO });
		}

		/// <summary>
		/// 修改計畫調整檔 異動紀錄檔ID
		/// </summary>
		/// <param name="PROJ_ADJ_ID">調整流水號</param>
		/// <param name="LOG_ID">異動紀錄檔ID</param>
		public void UpdateProjBasicAdjLogId(int PROJ_ADJ_ID, int LOG_ID)
		{
			string sql = $@"UPDATE PROJECT_BASIC_ADJ
                            SET LOG_ID = @LOG_ID,
                                MDF_USER = @UserId,
                                MDF_DATE = {DTNow}
                            WHERE PROJ_ADJ_ID = @PROJ_ADJ_ID ";
			ExecuteCommand(sql, new { PROJ_ADJ_ID, LOG_ID, UserId });
		}

		#region 轉檔
		/// <summary>
		/// 計畫主檔轉調整檔 PROJECT_BASIC => PROJECT_BASIC_ADJ
		/// </summary>
		/// <param name="PROJECT_NO">列管編號</param>
		/// <param name="AW_KIND">調整申請項目</param>
		/// <param name="PROJECT_AW_STATUS">計畫調整狀態</param>
		/// <returns>調整檔ID</returns>
		public int TransferProjectBasic(string PROJECT_NO, string AW_KIND, string PROJECT_AW_STATUS)
		{
			string sql = $@"INSERT INTO PROJECT_BASIC_ADJ (
								PROJECT_NO
								,AW_KIND
								,PROJECT_AW_STATUS
								,PROJECT_NAME
								,PROJECT_YEAR
								,PROJECT_KIND --建設類別
								,MASTER_ORGAN_C --主管機關
								,MASTER_UNDERTAKER_C --主管機關人員
								,EXEC_ORGAN_C --執行機關
								,EXEC_UNDERTAKER_C --執行機關人員
								,BUDGET_HOLD_ORGAN_C --代辦機關
								,BUDGET_HOLD_UNDERTAKER_C --代辦機關承辦人
								,TOWN_C --辦理地點
								,TOWN_M --辦理地點(跨區)
								,X_COORD --X坐標
								,Y_COORD --Y坐標
								,IS_POINTS_CHECK --多點位設定完畢
								,KML --多點位XML資料
								,ALL_JOB --計畫內容
								,PROJECT_BENEFIT --計畫效益
								,MEMO --備註
								,MEMO_CHK_POINT -- 檢核點備註
								,PROJECT_LOCATION --位置說明
								,REVIEWITEM --相關審查
								,CREATEDTIME --立案時間
								,CP_KIND --執行方式類別
								,RUNWAY_C_ORI -- 原執行方式
								,RUNWAY_C -- 執行方式
								,CONTROL_DATE1 -- 計畫開始日期
								,CRT_USER
								,CRT_DATE
								,MDF_USER
								,MDF_DATE
								)
							SELECT PROJECT_NO
								,@AW_KIND
								,@PROJECT_AW_STATUS
								,PROJECT_NAME
								,PROJECT_YEAR
								,PROJECT_KIND --建設類別
								,MASTER_ORGAN_C --主管機關
								,MASTER_UNDERTAKER_C --主管機關人員
								,EXEC_ORGAN_C --執行機關
								,EXEC_UNDERTAKER_C --執行機關人員
								,BUDGET_HOLD_ORGAN_C --代辦機關
								,BUDGET_HOLD_UNDERTAKER_C --代辦機關承辦人
								,TOWN_C --辦理地點
								,TOWN_M --辦理地點(跨區)
								,X_COORD --X坐標
								,Y_COORD --Y坐標
								,IS_POINTS_CHECK --多點位設定完畢
								,KML --多點位XML資料
								,ALL_JOB --計畫內容
								,PROJECT_BENEFIT --計畫效益
								,MEMO --備註
								,MEMO_CHK_POINT -- 檢核點備註
								,PROJECT_LOCATION --位置說明
								,REVIEWITEM --相關審查
								,CREATEDTIME --立案時間
								,CP_KIND --執行方式類別
								,RUNWAY_C -- 執行方式
								,RUNWAY_C -- 執行方式
								,(	SELECT CONTROL_DATE1
									FROM PROJECT_CONTROL_EXECUTE
									WHERE PROJECT_NO = @PROJECT_NO
								) AS CONTROL_DATE1 -- 計畫開始日期
								,@UserId
								,{DTNow}
								,@UserId
								,{DTNow}
							FROM PROJECT_BASIC
							WHERE PROJECT_NO = @PROJECT_NO
                            SELECT SCOPE_IDENTITY()";
			return ExecuteQueryFirstOrDefault<int>(sql, new { PROJECT_NO, AW_KIND, PROJECT_AW_STATUS, UserId });
		}

		/// <summary>
		/// 協辦機關/人員主檔轉調整檔 PROJECT_ASST_ORG => PROJECT_ASST_ORG_ADJ
		/// </summary>
		/// <param name="PROJECT_NO">列管編號</param>
		/// <param name="PROJ_ADJ_ID">調整檔ID</param>
		public void TransferAsstOrg(string PROJECT_NO, int PROJ_ADJ_ID)
		{
			string sql = $@"INSERT INTO PROJECT_ASST_ORG_ADJ (
								PROJ_ADJ_ID
								,PROJECT_NO
								,ASST_ORG
								,ASST_USER
								,CRT_USER
								,CRT_DATE
								,MDF_USER
								,MDF_DATE
								)
							SELECT @PROJ_ADJ_ID -- 此次調整的 PROJECT_BASIC_ADJ.PROJ_ADJ_ID
								,PROJECT_NO
								,ASSISTANT_ORGAN_C
								,ASSISTANT_UNDERTAKER_C
								,@UserId
								,{DTNow}
								,@UserId
								,{DTNow}
							FROM PROJECT_ASST_ORG
							WHERE PROJECT_NO = @PROJECT_NO";
			ExecuteCommand(sql, new { PROJECT_NO, PROJ_ADJ_ID, UserId });
		}

		/// <summary>
		/// 建設類別主檔轉調整檔 PROJECT_BUILD_KIND => PROJECT_BUILD_KIND_ADJ
		/// </summary>
		/// <param name="PROJECT_NO">列管編號</param>
		/// <param name="PROJ_ADJ_ID">調整檔ID</param>
		public void TransferBuildKind(string PROJECT_NO, int PROJ_ADJ_ID)
		{
			string sql = $@"INSERT INTO PROJECT_BUILD_KIND_ADJ (
								PROJ_ADJ_ID
								,PROJECT_NO
								,BUILD_KIND_TYPE
								,BUILD_KIND
								,CRT_USER
								,CRT_DATE
								,MDF_USER
								,MDF_DATE
								)
							SELECT @PROJ_ADJ_ID -- 此次調整的 PROJECT_BASIC_ADJ.PROJ_ADJ_ID
								,PROJECT_NO
								,BUILD_KIND_TYPE
								,BUILD_KIND
								,@UserId
								,{DTNow}
								,@UserId
								,{DTNow}
							FROM PROJECT_BUILD_KIND(NOLOCK)
							WHERE PROJECT_NO = @PROJECT_NO";
			ExecuteCommand(sql, new { PROJECT_NO, PROJ_ADJ_ID, UserId });
		}

		/// <summary>
		/// 自訂的計畫檢核點日期主檔轉調整檔 PROJECT_CHECKITEM => PROJECT_CHECKITEM_ADJ
		/// </summary>
		/// <param name="PROJECT_NO">列管編號</param>
		/// <param name="PROJ_ADJ_ID">調整檔ID</param>
		public void TransferCheckItem(string PROJECT_NO, int PROJ_ADJ_ID)
		{
			string sql = $@"INSERT INTO PROJECT_CHECKITEM_ADJ (
								PROJ_ADJ_ID
								,PROJECT_NO
								,CHECKITEM_SEQ
								,CHECKITEM_NAME
								,PROGRESS
								,ORI_ESTIMATED_ENDDATE
								,ESTIMATED_STARTDATE
								,ESTIMATED_ENDDATE
								,ACTUAL_ENDDATE
								,IS_DELAY
								,CRT_USER
								,CRT_DATE
								,MDF_USER
								,MDF_DATE)
							SELECT @PROJ_ADJ_ID -- 此次調整的 PROJECT_BASIC_ADJ.PROJ_ADJ_ID
								,PROJECT_NO
								,CHECKITEM_SEQ
								,CHECKITEM_NAME
								,PROGRESS
								,ESTIMATED_ENDDATE
								,ESTIMATED_STARTDATE
								,ESTIMATED_ENDDATE
								,ACTUAL_ENDDATE
								,IS_DELAY
								,@UserId
								,CRT_DATE
								,@UserId
								,MDF_DATE
							FROM PROJECT_CHECKITEM
							WHERE PROJECT_NO = @PROJECT_NO";
			ExecuteCommand(sql, new { PROJECT_NO, PROJ_ADJ_ID, UserId });
		}

        #endregion

        #region 調整基本資料

        /// <summary>
        /// 取得主辦申請調整基本資料原因
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <returns></returns>
        public async Task<AdjustReasonViewModel> GetExecReasonBasic(int PROJ_ADJ_ID)
		{
			string sql = @"SELECT PROJ_ADJ_ID
								,PROJECT_NO
								,PROJECT_NAME
								,AW_KIND
								,APPRV_DATE
								,ADJUST_REASON
								,OTHER_REASON
								,REVIEW_COMMENTS
								,REVIEW_RESULT
							FROM PROJECT_BASIC_ADJ (NOLOCK)
							WHERE DEL_FLG != 1
								AND PROJ_ADJ_ID = @PROJ_ADJ_ID ";
			return await ExecuteQueryFirstOrDefaultAsync<AdjustReasonViewModel>(sql, new { PROJ_ADJ_ID });
		}

		/// <summary>
		/// 取得計畫基本資料調整
		/// </summary>
		/// <param name="PROJECT_NO">列管編號</param>
		/// <param name="PROJ_ADJ_ID">調整檔流水號</param>
		/// <returns></returns>
		public async Task<ProjectBasicAdjustModel> GetProjectBasicAdj(string PROJECT_NO, int PROJ_ADJ_ID)
		{
			string sql = @" SELECT M1.PROJ_ADJ_ID
								,M1.PROJECT_NO
								,M1.PROJECT_NAME
								,M1.PROJECT_YEAR
								,M1.MASTER_ORGAN_C
								,M1.MASTER_UNDERTAKER_C
								,M1.EXEC_ORGAN_C
								,M1.EXEC_UNDERTAKER_C
								,M1.BUDGET_HOLD_ORGAN_C
								,M1.BUDGET_HOLD_UNDERTAKER_C
								,M1.REVIEWITEM
								,M1.TOWN_C
								,M1.TOWN_M
								,M1.PROJECT_LOCATION
								,M1.X_COORD
								,M1.Y_COORD
								,M1.ALL_JOB
								,M1.PROJECT_BENEFIT
								,M1.MEMO
								,M1.CREATEDTIME
								,STUFF((
										SELECT '、' + M2.SET_VALUE
										FROM PROJECT_MAPPING_DATA(NOLOCK) M1
										INNER JOIN SET_PARAM(NOLOCK) M2 ON M1.SET_TYPE = M2.SET_TYPE
											AND M2.SET_ITEM = 'SPEC_NOTE'
										WHERE PROJECT_NO = @PROJECT_NO
										FOR XML PATH('')
										), 1, 1, '') AS SPEC_NOTE
								,M2.REVIEW_COMMENTS
								,M1.SCORE_A
							FROM PROJECT_BASIC_ADJ(NOLOCK) M1
							LEFT JOIN (
								SELECT TOP 1 PROJECT_NO
									,REVIEW_COMMENTS
								FROM PROJECT_AUDIT(NOLOCK)
								WHERE PROJECT_NO = @PROJECT_NO
									AND PLAN_REVIEW_TYPE = 'P1'
								ORDER BY LOG_ID DESC
								) M2 ON M1.PROJECT_NO = M2.PROJECT_NO
							WHERE M1.PROJ_ADJ_ID = @PROJ_ADJ_ID ";
			return await ExecuteQueryFirstOrDefaultAsync<ProjectBasicAdjustModel>(sql, new { PROJECT_NO, PROJ_ADJ_ID });
		}

		/// <summary>
		/// 取得計畫建設類別調整
		/// </summary>
		/// <param name="PROJ_ADJ_ID">調整檔流水號</param>
		/// <returns></returns>
		public async Task<List<ProjectBuildKindAdjustModel>> GetProjectBuildKindAdj(int PROJ_ADJ_ID)
		{
			string sql = $@" SELECT ROW_NUMBER() OVER(ORDER BY PROJ_ADJ_ID) AS BUILD_ID, -- for 前端用
									PROJ_ADJ_ID,
									PROJECT_NO,
									BUILD_KIND_TYPE,
									BUILD_KIND
							FROM PROJECT_BUILD_KIND_ADJ (NOLOCK)
							WHERE PROJ_ADJ_ID = @PROJ_ADJ_ID ";
			return (await ExecuteQueryAsync<ProjectBuildKindAdjustModel>(sql, new { PROJ_ADJ_ID })).ToList();
		}

		/// <summary>
		/// 取得調整計畫協辦機關
		/// </summary>
		/// <param name="PROJ_ADJ_ID">調整檔流水號</param>
		/// <returns></returns>
		public async Task<List<ProjectAsstOrgAdjustModel>> GetAdjustAsstOrg(int PROJ_ADJ_ID)
		{
			string sql = $@"SELECT P.ASST_ID,
									P.PROJ_ADJ_ID,
									P.PROJECT_NO,
									P.ASST_ORG AS ASSISTANT_ORGAN_C,
									P.ASST_USER AS ASSISTANT_UNDERTAKER_C,
									S.USR_NAME AS ASSISTANT_UNDERTAKER_C_NAME
							FROM PROJECT_ASST_ORG_ADJ P
							LEFT JOIN {SC30_M}.SCUSERM S on S.USR_ID = P.ASST_USER
							WHERE P.PROJ_ADJ_ID = @PROJ_ADJ_ID";
			return (await ExecuteQueryAsync<ProjectAsstOrgAdjustModel>(sql, new { PROJ_ADJ_ID })).ToList();
		}

		/// <summary>
		/// 修改計畫調整檔 基本資料
		/// </summary>
		/// <param name="model"></param>
		public void UpdateProjectBasicDataAdj(ProjectBasicAdjustModel model)
		{
			string sql = $@"UPDATE PROJECT_BASIC_ADJ
							SET PROJECT_NO = @PROJECT_NO,
								PROJECT_NAME = @PROJECT_NAME,
								PROJECT_YEAR = @PROJECT_YEAR,
								MASTER_ORGAN_C = @MASTER_ORGAN_C,
								MASTER_UNDERTAKER_C = @MASTER_UNDERTAKER_C,
								EXEC_ORGAN_C = @EXEC_ORGAN_C,
								EXEC_UNDERTAKER_C = @EXEC_UNDERTAKER_C,
								BUDGET_HOLD_ORGAN_C = @BUDGET_HOLD_ORGAN_C,
								BUDGET_HOLD_UNDERTAKER_C = @BUDGET_HOLD_UNDERTAKER_C,
								REVIEWITEM = @REVIEWITEM,
								TOWN_C = @TOWN_C,
								TOWN_M = @TOWN_M,
								PROJECT_LOCATION = @PROJECT_LOCATION,
								X_COORD = @X_COORD,
								Y_COORD = @Y_COORD,
								ALL_JOB = @ALL_JOB,
								PROJECT_BENEFIT = @PROJECT_BENEFIT,
								MEMO = @MEMO,
								MDF_USER = @MDF_USER,
								MDF_DATE = {DTNow}
							WHERE PROJ_ADJ_ID = @PROJ_ADJ_ID ";
			ExecuteCommand(sql, model);
		}
		#region 計畫建設類別
		/// <summary>
		/// 新增計畫建設類別
		/// </summary>
		/// <param name="model"></param>
		/// <param name="PROJECT_NO">列管編號</param>
		/// <param name="PROJ_ADJ_ID">調整流水編號</param>
		public void InsertProjectBuildKindAdj(List<ProjectBuildKindAdjustModel> model, string PROJECT_NO, int PROJ_ADJ_ID)
		{
			string sql = $@"INSERT INTO PROJECT_BUILD_KIND_ADJ (
								PROJ_ADJ_ID
								,PROJECT_NO
								,BUILD_KIND_TYPE
								,BUILD_KIND
								,CRT_USER
								,CRT_DATE
								,MDF_USER
								,MDF_DATE
							) VALUES (
								@PROJ_ADJ_ID
								,@PROJECT_NO
								,@BUILD_KIND_TYPE
								,@BUILD_KIND
								,@USER
								,{DTNow}
								,@USER
								,{DTNow}) ";
			object param = model.Select(x =>
			{
				return new
				{
					PROJ_ADJ_ID = PROJ_ADJ_ID,
					PROJECT_NO = PROJECT_NO,
					BUILD_KIND_TYPE = x.BUILD_KIND_TYPE,
					BUILD_KIND = x.BUILD_KIND,
					USER = UserId
				};
			}).ToList();

			ExecuteCommand(sql, param);
		}

		/// <summary>
		/// 刪除計畫建設類別
		/// </summary>
		/// <param name="PROJ_ADJ_ID">調整流水編號</param>
		public void DeleteProjectBuildKindAdj(int PROJ_ADJ_ID)
		{
			string sql = @" DELETE PROJECT_BUILD_KIND_ADJ
							WHERE PROJ_ADJ_ID = @PROJ_ADJ_ID";
			ExecuteCommand(sql, new { PROJ_ADJ_ID });
		}
		#endregion
		#region 計畫協辦機關

		/// <summary>
		/// 新增計畫協辦機關調整
		/// </summary>
		/// <param name="model"></param>
		public void InsertAdjustAsstOrg(ProjectAsstOrgAdjustModel model)
		{
			string sql = $@"INSERT INTO PROJECT_ASST_ORG_ADJ (
	                            PROJ_ADJ_ID
	                            ,PROJECT_NO
	                            ,ASST_ORG
	                            ,ASST_USER
	                            ,CRT_USER
	                            ,CRT_DATE
	                            ,MDF_USER
	                            ,MDF_DATE
                            ) VALUES (
	                            @PROJ_ADJ_ID
	                            ,@PROJECT_NO
	                            ,@ASSISTANT_ORGAN_C
	                            ,@ASSISTANT_UNDERTAKER_C
	                            ,@CRT_USER
	                            ,{DTNow}
	                            ,@MDF_USER
	                            ,{DTNow}
                            ) ";
			ExecuteCommand(sql, model);
		}

		/// <summary>
		/// 修改計畫協辦機關
		/// </summary>
		/// <param name="model"></param>
		public void UpdateAdjustProjectAsstOrg(ProjectAsstOrgAdjustModel model)
		{
			string sql = $@"UPDATE PROJECT_ASST_ORG_ADJ
							SET ASST_ORG = @ASSISTANT_ORGAN_C,
								ASST_USER = @ASSISTANT_UNDERTAKER_C,
								MDF_USER = @MDF_USER,
								MDF_DATE = {DTNow}
							WHERE ASST_ID = @ASST_ID";
			ExecuteCommand(sql, model);
		}

		/// <summary>
		/// 刪除計畫協辦機關調整
		/// </summary>
		/// <param name="ASST_ID">計畫協關機關流水編號</param>
		public void DeleteAdjustAsstOrg(int ASST_ID)
		{
			string sql = @" DELETE FROM PROJECT_ASST_ORG_ADJ
							WHERE ASST_ID = @ASST_ID";
			ExecuteCommand(sql, new { ASST_ID });
		}

		#endregion
		#endregion

		#region 調整期程

		/// <summary>
		/// 取得主辦申請調整期程原因
		/// </summary>
		/// <param name="PROJ_ADJ_ID">調整檔流水號</param>
		/// <returns></returns>
		public async Task<AdjustReasonScheduleViewModel> GetExecReasonSchedule(int PROJ_ADJ_ID)
		{
			string sql = @"SELECT PROJ_ADJ_ID
								,a.PROJECT_NO
								,a.PROJECT_NAME
								,a.AW_KIND
								,a.APPRV_DATE
								,a.ADJUST_REASON
								,a.OTHER_REASON
								,dbo.FN_GET_PROJECT_EXS(a.PROJECT_NO, 'T') AS BUDGET
								,b.PROCUREMENT_AMT
								,b.TENDER_AWARDING_AMT
								,a.TENDER_PROJ
								,a.TENDER_DESIGN
								,a.TENDER_SUPV
								,a.TENDER_CONST
								,a.IS_BUDGET_CENTRAL
								,a.NO_OD_REASON
								,a.IS_EFFECT_BUDGET
								,a.EFFECT_BUDGET_MEMO
								,a.CUR_EXECUTION
								,a.REVIEW_COMMENTS
								,a.REVIEW_RESULT
								,STUFF((
									SELECT '、' + MM2.SET_VALUE 
									FROM PROJECT_MAPPING_DATA (NOLOCK) MM1
									INNER JOIN SET_PARAM (NOLOCK) MM2 
										ON MM1.SET_TYPE = MM2.SET_TYPE AND MM2.SET_ITEM = 'SPEC_NOTE'
									WHERE PROJECT_NO = a.PROJECT_NO
									FOR XML PATH('')
								), 1, 1, '') AS SPEC_NOTE --特殊加註
							FROM PROJECT_BASIC_ADJ a(NOLOCK)
							LEFT JOIN PROJECT_BASIC b(NOLOCK) ON a.PROJECT_NO = b.PROJECT_NO
							WHERE a.DEL_FLG != 1
								AND a.PROJ_ADJ_ID = @PROJ_ADJ_ID ";
			return await ExecuteQueryFirstOrDefaultAsync<AdjustReasonScheduleViewModel>(sql, new { PROJ_ADJ_ID });
		}
		
		/// <summary>
		/// 修改計畫調整檔 for 期程調整
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public void UpdateProjectBasicAdjForSchedule(AdjustReasonModel model)
		{
			string sql = $@"UPDATE PROJECT_BASIC_ADJ
							SET APPRV_DATE = @APPRV_DATE
								,ADJUST_REASON = @ADJUST_REASON
								,OTHER_REASON = @OTHER_REASON
								,TENDER_PROJ = @TENDER_PROJ
								,TENDER_DESIGN = @TENDER_DESIGN
								,TENDER_SUPV = @TENDER_SUPV
								,TENDER_CONST = @TENDER_CONST
								,IS_BUDGET_CENTRAL = @IS_BUDGET_CENTRAL
								,NO_OD_REASON = @NO_OD_REASON
								,IS_EFFECT_BUDGET = @IS_EFFECT_BUDGET
								,EFFECT_BUDGET_MEMO = @EFFECT_BUDGET_MEMO
								,CUR_EXECUTION = @CUR_EXECUTION
							WHERE PROJ_ADJ_ID = @PROJ_ADJ_ID";
			ExecuteCommand(sql, model);
		}

		/// <summary>
		/// 取得計劃檢核點設定資料
		/// </summary>
		/// <param name="PROJ_ADJ_ID">調整檔流水號</param>
		/// <returns></returns>
		public async Task<AdjustCheckPointModel> GetAdjustCheckPoint(int PROJ_ADJ_ID)
		{
			string sql = @" SELECT a.PROJ_ADJ_ID
								,a.PROJECT_NO
								,a.PROJECT_NAME
								,a.CONTROL_DATE1
								,a.CP_KIND
								,a.RUNWAY_C
								,a.RUNWAY_C as OLD_RUNWAY_C
								,a.MEMO_CHK_POINT
								,a.SCHE_TYPE
							FROM PROJECT_BASIC_ADJ a(NOLOCK)
							WHERE a.PROJ_ADJ_ID = @PROJ_ADJ_ID ";
			return await ExecuteQueryFirstOrDefaultAsync<AdjustCheckPointModel>(sql, new { PROJ_ADJ_ID });
		}

		/// <summary>
		/// 取得計劃自訂檢核點資料
		/// </summary>
		/// <param name="PROJ_ADJ_ID">調整檔流水號</param>
		/// <returns></returns>
		public async Task<List<AdjustCusCheckPointModel>> GetAdjustCusCheckPoint(int PROJ_ADJ_ID)
		{
			string sql = @" SELECT a.SEQ
								,a.PROJ_ADJ_ID
								,a.PROJECT_NO
								,a.CHECKITEM_SEQ
								,a.CHECKITEM_NAME
								,a.PROGRESS
								,a.ORI_ESTIMATED_ENDDATE
								,a.ESTIMATED_STARTDATE
								,a.ESTIMATED_ENDDATE
								,a.ACTUAL_ENDDATE
								,params.CTRL_POINT
								,a.MDF_DATE
							FROM PROJECT_CHECKITEM_ADJ(NOLOCK) a
							LEFT JOIN CODE_CHECKPOINT_ITEM(NOLOCK) params ON a.CHECKITEM_SEQ = params.SEQ
							WHERE a.PROJ_ADJ_ID = @PROJ_ADJ_ID
							ORDER BY PROGRESS, ESTIMATED_ENDDATE";
			return (await ExecuteQueryAsync<AdjustCusCheckPointModel>(sql, new { PROJ_ADJ_ID })).ToList();
		}

		/// <summary>
		/// 更新檢核點調整 - 計畫調整檔
		/// </summary>
		/// <param name="model"></param>
		public void UpdateAdjustCheckPoint(AdjustCheckPointModel model)
		{
			string sql = $@"UPDATE PROJECT_BASIC_ADJ
							SET CP_KIND = @CP_KIND
								,CONTROL_DATE1 = @CONTROL_DATE1
								,PROJECT_LAST_DATE = @PROJECT_LAST_DATE
								,RUNWAY_C = @RUNWAY_C
								,SCHE_TYPE = @SCHE_TYPE
								,MEMO_CHK_POINT = @MEMO_CHK_POINT
								,MDF_USER = @MDF_USER
								,MDF_DATE = {DTNow}
							WHERE PROJ_ADJ_ID = @PROJ_ADJ_ID ";
			ExecuteCommand(sql, model);
		}

		/// <summary>
		/// 新增檢核點設定資料
		/// </summary>
		/// <param name="models"></param>
		public void InsertAdjustCustomChkItem(List<AdjustCusCheckPointModel> models)
		{
			string sql = $@"INSERT INTO PROJECT_CHECKITEM_ADJ (
	                            PROJ_ADJ_ID
	                            ,PROJECT_NO
	                            ,CHECKITEM_SEQ
	                            ,CHECKITEM_NAME
	                            ,PROGRESS
								,ORI_ESTIMATED_ENDDATE
	                            ,ESTIMATED_STARTDATE
	                            ,ESTIMATED_ENDDATE
								,ACTUAL_ENDDATE
	                            ,CRT_USER
	                            ,CRT_DATE
	                            ,MDF_USER
	                            ,MDF_DATE
                            ) VALUES (
	                            @PROJ_ADJ_ID
	                            ,@PROJECT_NO
	                            ,@CHECKITEM_SEQ
	                            ,@CHECKITEM_NAME
	                            ,@PROGRESS
								,@ORI_ESTIMATED_ENDDATE
	                            ,@ESTIMATED_STARTDATE
	                            ,@ESTIMATED_ENDDATE
								,@ACTUAL_ENDDATE
	                            ,@CRT_USER
	                            ,{DTNow}
	                            ,@MDF_USER
	                            ,{DTNow}
	                        )";
			ExecuteCommand(sql, models);
		}

		/// <summary>
		/// 更新檢核點設定資料
		/// </summary>
		/// <param name="models"></param>
		public void UpdateAdjustCustomChkItem(List<AdjustCusCheckPointModel> models)
		{
			string sql = $@"UPDATE PROJECT_CHECKITEM_ADJ
							SET
								CHECKITEM_NAME = @CHECKITEM_NAME,
	                            PROGRESS = @PROGRESS,
								ESTIMATED_STARTDATE = @ESTIMATED_STARTDATE,
								ESTIMATED_ENDDATE = @ESTIMATED_ENDDATE,
								MDF_USER = @MDF_USER,
	                            MDF_DATE = {DTNow}
							WHERE SEQ = @SEQ";
			ExecuteCommand(sql, models);
		}

		/// <summary>
		/// 刪除檢核點設定資料
		/// </summary>
		/// <param name="SEQs"></param>
		public void DeleteAdjustCustomChkItem(List<int> SEQs)
		{
			string sql = @" DELETE PROJECT_CHECKITEM_ADJ
							WHERE SEQ in @SEQs ";
			ExecuteCommand(sql, new { SEQs });
		}
		
		/// <summary>
		/// 刪除計畫所有檢核點
		/// </summary>
		/// <param name="PROJ_ADJ_ID"></param>
		public void DeleteProjectAdjChkItem(int PROJ_ADJ_ID)
		{
			string sql = @"DELETE PROJECT_CHECKITEM_ADJ 
						   WHERE PROJ_ADJ_ID = @PROJ_ADJ_ID";
			ExecuteCommand(sql,new {PROJ_ADJ_ID });
		}

		#endregion

		/// <summary>
		/// 取得主辦調整檢核結果
		/// </summary>
		/// <param name="PROJ_ADJ_ID">調整流水編號</param>
		/// <returns></returns>
		public async Task<AdjustCheckModel> GetAdjustScheChk(int PROJ_ADJ_ID)
		{
			string sql = @" SELECT TOP 1 a.PROJECT_NAME -- 回傳前端用
								,a.SCHE_TYPE -- 回傳前端用
								,a.ADJUST_REASON
								,CASE WHEN bChkpt.ACTUAL_ENDDATE IS NOT NULL THEN '13'
									WHEN aChkpt.ACTUAL_ENDDATE IS NOT NULL THEN '23'
									END AS FILE_KIND -- 需上傳的檔案FILE_KIND
								,count(att.FILE_KIND) OVER (PARTITION BY a.PROJ_ADJ_ID) AS CNT_FILE -- 已上傳的檔案個數
							FROM PROJECT_BASIC_ADJ a(NOLOCK)
							LEFT JOIN (
								SELECT main.PROJ_ADJ_ID, main.ACTUAL_ENDDATE
								FROM PROJECT_CHECKITEM_ADJ(NOLOCK) main
								LEFT JOIN CODE_CHECKPOINT_ITEM(NOLOCK) item ON main.CHECKITEM_SEQ = item.SEQ
								WHERE item.CTRL_POINT = 'B'
								) bChkpt ON bChkpt.PROJ_ADJ_ID = a.PROJ_ADJ_ID --竣工檢核點
							LEFT JOIN (
								SELECT main.PROJ_ADJ_ID, ACTUAL_ENDDATE
								FROM PROJECT_CHECKITEM_ADJ(NOLOCK) main
								LEFT JOIN CODE_CHECKPOINT_ITEM(NOLOCK) item ON main.CHECKITEM_SEQ = item.SEQ
								WHERE item.CTRL_POINT = 'A'
								) aChkpt ON aChkpt.PROJ_ADJ_ID = a.PROJ_ADJ_ID -- 開工檢核點
							LEFT JOIN PROJECT_ATTACHMENT(NOLOCK) att ON a.PROJ_ADJ_ID = att.SOURCE_ID
								AND ( att.FILE_KIND = '13' OR att.FILE_KIND = '23' )
							WHERE a.DEL_FLG != 1
								AND AW_KIND = 'AW02'
								AND a.PROJ_ADJ_ID = @PROJ_ADJ_ID ";
			return await ExecuteQueryFirstOrDefaultAsync<AdjustCheckModel>(sql, new { PROJ_ADJ_ID });
		}

        #region 管考審核

        /// <summary>
        /// 管考取得主辦調整原因
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <returns></returns>
        public async Task<AdjustAuditModel> GetExecReasonByAudit(int PROJ_ADJ_ID)
		{
			string sql = @" SELECT a.PROJ_ADJ_ID
								,a.PROJECT_NO
								,a.PROJECT_NAME
								,a.AW_KIND
								,SCForExecOrg.OU_NAME AS EXEC_UNIT
								,a.APPRV_DATE
								,a.ADJUST_REASON
								,a.OTHER_REASON
								,a.PROJECT_AW_STATUS
								,a.REVIEW_COMMENTS
								,a.REVIEW_RESULT
								,a.IS_DELAY_APPLY
								,a.DELAY_COMMENTS
								,a.LOG_ID
							FROM PROJECT_BASIC_ADJ a(NOLOCK)
							LEFT JOIN {0}.SCORG_UNITM AS SCForExecOrg ON a.EXEC_ORGAN_C = SCForExecOrg.OU_ID
							WHERE a.DEL_FLG != 1
								AND a.PROJ_ADJ_ID = @PROJ_ADJ_ID ";
			sql = string.Format(sql, SC30_M);
			return await ExecuteQueryFirstOrDefaultAsync<AdjustAuditModel>(sql, new { PROJ_ADJ_ID });
		}

		/// <summary>
		/// 儲存管考審核調整撤銷結果
		/// </summary>
		/// <param name="model"></param>
		public void UpdateProjectBasicAdjByAudit(AdjustAuditModel model)
		{
			string sql = $@"UPDATE PROJECT_BASIC_ADJ
							SET REVIEW_COMMENTS = @REVIEW_COMMENTS
								,REVIEW_RESULT = @REVIEW_RESULT
								,APPRV_DATE = @APPRV_DATE
								,IS_DELAY_APPLY = @IS_DELAY_APPLY
								,DELAY_COMMENTS = @DELAY_COMMENTS
								,MDF_USER = @MDF_USER
								,MDF_DATE = {DTNow}
							WHERE PROJ_ADJ_ID = @PROJ_ADJ_ID ";
			ExecuteCommand(sql, model);
		}

        #region 回寫主檔

        #region 調整基本資料

        /// <summary>
        /// 計畫調整檔回寫主檔 PROJECT_BASIC_ADJ => PROJECT_BASIC 
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        public void TransferProjectBasicByAudit(int PROJ_ADJ_ID)
		{
			string sql = $@"UPDATE a
							SET PROJECT_NAME = b.PROJECT_NAME
								,PROJECT_YEAR = b.PROJECT_YEAR
								,PROJECT_KIND = b.PROJECT_KIND --建設類別
								,MASTER_ORGAN_C = b.MASTER_ORGAN_C --主管機關
								,MASTER_UNDERTAKER_C = b.MASTER_UNDERTAKER_C --主管機關人員
								,EXEC_ORGAN_C = b.EXEC_ORGAN_C --執行機關
								,EXEC_UNDERTAKER_C = b.EXEC_UNDERTAKER_C --執行機關人員
								,BUDGET_HOLD_ORGAN_C = b.BUDGET_HOLD_ORGAN_C --代辦機關
								,BUDGET_HOLD_UNDERTAKER_C = b.BUDGET_HOLD_UNDERTAKER_C --代辦機關承辦人
								,TOWN_C = b.TOWN_C --辦理地點
								,TOWN_M = b.TOWN_M --辦理地點(跨區)
								,X_COORD = b.X_COORD --X坐標
								,Y_COORD = b.Y_COORD --Y坐標
								,IS_POINTS_CHECK = b.IS_POINTS_CHECK --多點位設定完畢
								,KML = b.KML --多點位XML資料
								,ALL_JOB = b.ALL_JOB --計畫內容
								,PROJECT_BENEFIT = b.PROJECT_BENEFIT --計畫效益
								,MEMO = b.MEMO -- 備註
								,PROJECT_LOCATION = b.PROJECT_LOCATION --位置說明
								,REVIEWITEM = b.REVIEWITEM --相關審查
								,CREATEDTIME = b.CREATEDTIME --立案時間
								,MDF_USER = @UserId
								,MDF_DATE = {DTNow}
							FROM PROJECT_BASIC a
							INNER JOIN PROJECT_BASIC_ADJ b ON a.PROJECT_NO = b.PROJECT_NO
							WHERE b.PROJ_ADJ_ID = @PROJ_ADJ_ID";
			ExecuteCommand(sql, new { PROJ_ADJ_ID, UserId });
		}

		/// <summary>
		/// 刪除協辦機關/人員 依據PROJECT_NO
		/// </summary>
		/// <param name="PROJECT_NO">列管編號</param>
		public void DeleteAsstOrgByProjectNo(string PROJECT_NO)
		{
			string sql = $@"DELETE PROJECT_ASST_ORG
							WHERE PROJECT_NO = @PROJECT_NO ";
			ExecuteCommand(sql, new { PROJECT_NO });
		}

		/// <summary>
		/// 協辦機關/人員調整 從調整檔新增到主檔
		/// </summary>
		/// <param name="PROJ_ADJ_ID">調整流水號</param>
		public void InsertAsstOrgSelectAdjust(int PROJ_ADJ_ID)
		{
			string sql = $@"INSERT PROJECT_ASST_ORG (
								PROJECT_NO
								,ASSISTANT_ORGAN_C
								,ASSISTANT_UNDERTAKER_C
								,CRT_DATE
								,CRT_USER
								,MDF_DATE
								,MDF_USER
								)
							SELECT PROJECT_NO
								,ASST_ORG
								,ASST_USER
								,{DTNow}
								,@UserId
								,{DTNow}
								,@UserId
							FROM PROJECT_ASST_ORG_ADJ
							WHERE PROJ_ADJ_ID =  @PROJ_ADJ_ID ";
			ExecuteCommand(sql, new { PROJ_ADJ_ID, UserId });
		}

		/// <summary>
		/// 經費來源調整檔轉主檔 PROJECT_BUDGET_SOURCE_G 刪除原資料、修改調整資料的PROJECT_NO
		/// </summary>
		/// <param name="IDENTITY_FIELDs">欲刪除的流水編號</param>
		/// <param name="PROJECT_NO">列管編號</param>
		/// <param name="originProjectNo">調整時用的列管編號</param>
		public void TransferBudgetSourceGByAudit(List<int> IDENTITY_FIELDs, string PROJECT_NO, string originProjectNo)
		{
			string sql = $@"-- 先刪除原本的經費來源
							DELETE PROJECT_BUDGET_SOURCE_G 
							WHERE IDENTITY_FIELD in @IDENTITY_FIELDs
							-- 再修改調整後的經費來源
							UPDATE PROJECT_BUDGET_SOURCE_G
							SET PROJECT_NO = @PROJECT_NO
								,MDF_USER = @UserId
								,MDF_DATE = {DTNow}
							WHERE PROJECT_NO = @originProjectNo ";
			ExecuteCommand(sql, new { IDENTITY_FIELDs, PROJECT_NO, originProjectNo, UserId });
		}

		/// <summary>
		/// 修改經費來源檔案 PROJECT_ATTACHMENT 刪除原資料、修改調整資料
		/// </summary>
		/// <param name="IDENTITY_FIELDs">欲刪除的流水編號</param>
		/// <param name="model">檔案model(修改成的FILE_KIND、PROJECT_NO)</param>
		/// <param name="adjustProjectNo">調整時的列管編號</param>
		public void TransferBudgetSourceAttByAudit(List<int> IDENTITY_FIELDs, ProjectAttachmentModel model, string adjustProjectNo)
        {
			string sql = $@"-- 先刪除原本的經費來源檔案
							DELETE PROJECT_ATTACHMENT 
							WHERE IDENTITY_FIELD in @IDENTITY_FIELDs
							-- 再修改調整後的經費來源檔案
							UPDATE PROJECT_ATTACHMENT
							SET FILE_KIND = @FILE_KIND
								,FILE_PATH = CONCAT ('/', @PROJECT_NO)
								,MDF_USER = @UserId
								,MDF_DATE = {DTNow}
							WHERE FILE_PATH = CONCAT('/', @adjustProjectNo) ";
			object param = new
			{
				IDENTITY_FIELDs = IDENTITY_FIELDs,
				FILE_KIND = model.FILE_KIND,
				PROJECT_NO = model.PROJECT_NO,
				adjustProjectNo = adjustProjectNo,
				UserId = UserId
			};
			ExecuteCommand(sql, param);
		}
		/// <summary>
		/// 建設類別調整檔轉主檔 PROJECT_BUILD_KIND_ADJ => PROJECT_BUILD_KIND
		/// </summary>
		/// <param name="PROJ_ADJ_ID">調整流水號</param>
		public void TransferBuildKindByAudit(int PROJ_ADJ_ID)
		{
			string sql = $@"INSERT INTO PROJECT_BUILD_KIND (
								PROJECT_NO
								,BUILD_KIND_TYPE
								,BUILD_KIND
								,CRT_USER
								,CRT_DATE
								,MDF_USER
								,MDF_DATE
								)
							SELECT PROJECT_NO
								,BUILD_KIND_TYPE
								,BUILD_KIND
								,@UserId
								,{DTNow}
								,@UserId
								,{DTNow}
							FROM PROJECT_BUILD_KIND_ADJ
							WHERE PROJ_ADJ_ID = @PROJ_ADJ_ID ";
			ExecuteCommand(sql, new { PROJ_ADJ_ID, UserId });
		}

		#endregion

		#region 調整期程

		/// <summary>
		/// 計畫調整檔回寫主檔(for 期程調整) PROJECT_BASIC_ADJ => PROJECT_BASIC 
		/// </summary>
		/// <param name="PROJ_ADJ_ID">調整流水號</param>
		public void TransferProjectBasicByAuditForSchedule(int PROJ_ADJ_ID)
		{
			string sql = $@"UPDATE a
							SET CP_KIND = b.CP_KIND
								,RUNWAY_C = b.RUNWAY_C
								,PROJECT_LAST_DATE = b.PROJECT_LAST_DATE
								,MEMO_CHK_POINT = b.MEMO_CHK_POINT
								,MDF_USER = @UserId
								,MDF_DATE = {DTNow}
							FROM PROJECT_BASIC a
							INNER JOIN PROJECT_BASIC_ADJ b ON a.PROJECT_NO = b.PROJECT_NO
							WHERE b.PROJ_ADJ_ID = @PROJ_ADJ_ID";
			ExecuteCommand(sql, new { PROJ_ADJ_ID, UserId });
		}

		/// <summary>
		/// 計畫開始日期回寫主檔 PROJECT_BASIC_ADJ.CONTROL_DATE1 => PROJECT_CONTROL_EXECUTE.CONTROL_DATE1
		/// </summary>
		/// <param name="PROJ_ADJ_ID">調整流水號</param>
		public void TransferControlDate1ByAudit(int PROJ_ADJ_ID)
		{
			string sql = $@"UPDATE a
							SET a.CONTROL_DATE1 = b.CONTROL_DATE1
								,MDF_USER = @UserId
								,MDF_DATE = {DTNow}
							FROM PROJECT_CONTROL_EXECUTE a
							INNER JOIN PROJECT_BASIC_ADJ b ON a.PROJECT_NO = b.PROJECT_NO
							WHERE b.PROJ_ADJ_ID = @PROJ_ADJ_ID";
			ExecuteCommand(sql, new { PROJ_ADJ_ID, UserId });
		}

		/// <summary>
		/// 取得檢核點調整後寫回資料
		/// </summary>
		/// <param name="PROJ_ADJ_ID"></param>
		/// <returns></returns>
		public async Task<List<ProjectCusCheckpointModel>> GetProjectCheckItemTransData(int PROJ_ADJ_ID)
		{
			string sql = @" SELECT PCA.PROJECT_NO
								,PCA.CHECKITEM_SEQ
								,PCA.CHECKITEM_NAME
								,PCA.PROGRESS
								,PCA.ESTIMATED_STARTDATE
								,PCA.ESTIMATED_ENDDATE
								,PC.ACTUAL_ENDDATE
								,PCA.IS_DELAY
								,PCA.CRT_DATE
								,CASE WHEN PCA.MDF_DATE >= PC.MDF_DATE 
									THEN PCA.MDF_DATE 
									ELSE PC.MDF_DATE 
								 END as MDF_DATE
							FROM PROJECT_CHECKITEM_ADJ PCA (NOLOCK)
							LEFT JOIN PROJECT_CHECKITEM PC (NOLOCK) 
								ON PCA.PROJECT_NO = PC.PROJECT_NO 
								AND PCA.CHECKITEM_SEQ = PC.CHECKITEM_SEQ 
							WHERE PCA.PROJ_ADJ_ID = @PROJ_ADJ_ID";

			return (await ExecuteQueryAsync<ProjectCusCheckpointModel>(sql, new { PROJ_ADJ_ID })).ToList();
        }

		/// <summary>
		/// 自訂的計畫檢核點日期調整寫回主檔 
		/// </summary>
		/// <param name="models"></param>
		public void TransferCheckItemByAudit(List<ProjectCusCheckpointModel> models)
		{
			string sql = @"INSERT INTO PROJECT_CHECKITEM (
								PROJECT_NO
								,CHECKITEM_SEQ
								,CHECKITEM_NAME
								,PROGRESS
								,ESTIMATED_STARTDATE
								,ESTIMATED_ENDDATE
								,ACTUAL_ENDDATE
								,IS_DELAY
								,CRT_USER
								,CRT_DATE
								,MDF_USER
								,MDF_DATE)
							VALUES( @PROJECT_NO
								,@CHECKITEM_SEQ
								,@CHECKITEM_NAME
								,@PROGRESS
								,@ESTIMATED_STARTDATE
								,@ESTIMATED_ENDDATE
								,@ACTUAL_ENDDATE
								,@IS_DELAY
								,@CRT_USER
								,@CRT_DATE
								,@MDF_USER
								,@MDF_DATE)";
			ExecuteCommand(sql, models);
		}

		/// <summary>
		/// 取消當期執行情形送出
		/// </summary>
		/// <param name="PROJECT_NO"></param>
		public void CancelLatestSendStatus(string PROJECT_NO)
		{
			string sql = @" update PROJECT_ENGINEERING_PROGRESS 
							set IS_SEND = 0 
							where PROJECT_NO = @PROJECT_NO
								and YEAR = (select top 1 PROJECT_YEAR from PROJECT_FILL_CYCLE (nolock) order by SEQ desc)
								and MONTH = (select top 1 PROJECT_MONTH from PROJECT_FILL_CYCLE (nolock) order by SEQ desc)";
			ExecuteCommand(sql, new { PROJECT_NO });
		}

		#endregion

		#region 撤銷

		/// <summary>
		/// 計畫調整檔回寫主檔(for 撤銷) PROJECT_BASIC_ADJ => PROJECT_BASIC 
		/// </summary>
		/// <param name="PROJ_ADJ_ID">調整流水號</param>
		public void TransferProjectBasicByAuditForRevoke(int PROJ_ADJ_ID)
		{
			string sql = $@"UPDATE a
							SET FINISH_DATE = {DTNow}
								,MDF_USER = @UserId
								,MDF_DATE = {DTNow}
							FROM PROJECT_BASIC a
							INNER JOIN PROJECT_BASIC_ADJ b ON a.PROJECT_NO = b.PROJECT_NO
							WHERE b.PROJ_ADJ_ID = @PROJ_ADJ_ID";
			ExecuteCommand(sql, new { PROJ_ADJ_ID, UserId });
		}
		#endregion 撤銷

		#endregion

		#endregion

		#region 期程調整申請表

		/// <summary>
		/// 取得期程調整申請表
		/// </summary>
		/// <param name="PROJ_ADJ_ID">調整流水號</param>
		/// <returns></returns>
		public async Task<RPTAdjustScheduleModel> GetRPTAdjustSchedule(int PROJ_ADJ_ID)
		{
			string sql = @" SELECT main.PROJECT_NO
								,main.PROJECT_NAME
								,main.EXEC_ORGAN_C
								,adjust.SCHE_TYPE
								,CASE WHEN y.SCHE_Y_CNT IS NULL THEN 0 ELSE y.SCHE_Y_CNT END AS SCHE_Y_CNT
								,CASE WHEN m.SCHE_M_CNT IS NULL THEN 0 ELSE m.SCHE_M_CNT END AS SCHE_M_CNT
								,main.CP_KIND
								,main.RUNWAY_C
								,adjust.CP_KIND AS CP_KIND_ADJ
								,adjust.RUNWAY_C AS RUNWAY_C_ADJ
								,main.PROJECT_LOCATION
								,main.ALL_JOB
								,main.PROJECT_BENEFIT
							FROM PROJECT_BASIC main(NOLOCK)
							INNER JOIN PROJECT_BASIC_ADJ adjust ON main.PROJECT_NO = adjust.PROJECT_NO
							LEFT JOIN (
								SELECT PROJECT_NO ,SCHE_TYPE ,count(SCHE_TYPE) AS SCHE_Y_CNT
								FROM PROJECT_BASIC_ADJ
								WHERE SCHE_TYPE = 'Y'
									AND DEL_FLG = 0
									AND PROJECT_AW_STATUS = 'B05'
								GROUP BY SCHE_TYPE ,PROJECT_NO
								) y ON main.PROJECT_NO = y.PROJECT_NO -- 總期程調整
							LEFT JOIN (
								SELECT PROJECT_NO ,SCHE_TYPE ,count(SCHE_TYPE) AS SCHE_M_CNT
								FROM PROJECT_BASIC_ADJ
								WHERE SCHE_TYPE = 'M'
									AND DEL_FLG = 0
									AND PROJECT_AW_STATUS = 'B05'
								GROUP BY SCHE_TYPE ,PROJECT_NO
								) m ON main.PROJECT_NO = m.PROJECT_NO -- 分月期程調整
							WHERE PROJ_ADJ_ID = @PROJ_ADJ_ID ";
			return await ExecuteQueryFirstOrDefaultAsync<RPTAdjustScheduleModel>(sql, new { PROJ_ADJ_ID });
		}

		#endregion 期程調整申請表

		/// <summary>
		/// 取得期程調整歷程資料
		/// </summary>
		/// <param name="PROJECT_NO">列管編號</param>
		/// <param name="REVIEW_RESULT">管考審核結果(Y: 審核通過、N: 審核不通過、R: 退回補正)</param>
		/// <returns>該計畫的所有期程調整歷程(含分月和總期程調整)</returns>
		public async Task<List<AdjustScheHistoryModel>> GetProjAdjScheHistory(List<string> PROJECT_NO, string REVIEW_RESULT = "Y")
		{
			string sql = @"
				SELECT 
					ROW_NUMBER() OVER (
						PARTITION BY main.PROJECT_NO, main.SCHE_TYPE 
						ORDER BY main.CRT_DATE, main.SCHE_TYPE
					) AS SEQ -- 第SEQ次總期程/分月期程調整
					,main.PROJ_ADJ_ID
					,main.PROJECT_NO
					,main.SCHE_TYPE
					,main.PROJECT_AW_STATUS
					,main.APPRV_DATE
					,(
						SELECT STUFF((
							SELECT 
								CASE WHEN A.SET_TYPE = '99' THEN ',' + B.SET_VALUE + ': ' + main.OTHER_REASON 
								ELSE ',' + B.SET_VALUE 
								END
							FROM PROJECT_MAPPING_DATA (NOLOCK) A
							INNER JOIN SET_PARAM (NOLOCK) B 
								ON B.SET_ITEM = A.SET_ITEM AND B.SET_TYPE = A.SET_TYPE
							WHERE A.SOURCE_ID = CAST(main.PROJ_ADJ_ID as nvarchar(16))
								AND A.SET_ITEM = 'ADJUST_REASON'
							FOR xml path('')
						), 1, 1, '')
					) AS REASON -- 調整原因
					,chk.ORI_ESTIMATED_ENDDATE -- 僅取總期程的原預定完成日
					,(
						CASE WHEN SCHE_TYPE = 'Y' THEN chk.ESTIMATED_ENDDATE
						ELSE (  
							SELECT max(ESTIMATED_ENDDATE)
							FROM PROJECT_CHECKITEM_ADJ(NOLOCK)
							WHERE PROJ_ADJ_ID = main.PROJ_ADJ_ID
						)
						END
					) AS ADJ_LAST_DATE -- 最後一個調整後預定完成日期
					,main.REVIEW_RESULT
					,main.REVIEW_COMMENTS
					,main.CRT_DATE
				FROM PROJECT_BASIC_ADJ(NOLOCK) main
				LEFT JOIN PROJECT_CHECKITEM_ADJ(NOLOCK) chk 
					ON chk.PROJ_ADJ_ID = main.PROJ_ADJ_ID AND chk.PROGRESS = 100
				WHERE main.AW_KIND = 'AW02'
					AND main.DEL_FLG = 0
					AND main.SCHE_TYPE IS NOT NULL
					AND main.PROJECT_AW_STATUS IN ('B03', 'B04', 'B05')
					AND main.PROJECT_NO IN @PROJECT_NO
					AND main.REVIEW_RESULT = @REVIEW_RESULT";
			return (await ExecuteQueryAsync<AdjustScheHistoryModel>(sql, new { PROJECT_NO, REVIEW_RESULT })).ToList();
		}

		/// <summary>
		/// 取得期程調整 「調整中」及「調整歷程」資料
		/// </summary>
		/// <param name="PROJECT_NO"></param>
		/// <returns>(調整中,調整歷程)</returns>
		public async Task<(List<ProjectScheOverviewModel>, List<ProjectScheOverviewModel>)> GetProjectScheAdjustList(string PROJECT_NO)
		{
			StringBuilder sql = new();
			// 調整中期程資料
			sql.AppendLine(@"select PBA.PROJ_ADJ_ID ,
								PBA.CONTROL_DATE1,
								PBA.SCHE_TYPE ,
								CP_KIND,
								CCI.CTRL_POINT ,
								PCA.ESTIMATED_ENDDATE,
								PCA.CHECKITEM_NAME,
								PBL.CRT_DATE as ITEM_DATE --調整送審日期
							from PROJECT_BASIC_ADJ PBA (nolock) 
							left join PROJECT_CHECKITEM_ADJ PCA (nolock)
								on PBA.PROJ_ADJ_ID = PCA.PROJ_ADJ_ID
							left join CODE_CHECKPOINT_ITEM CCI (nolock)
								on PCA.CHECKITEM_SEQ  = CCI.SEQ
							left join ( 
							   select TOP 1 CRT_DATE,
									 PROJECT_NO
							   from PROJECT_BASIC_LOG (nolock)
							   where PROJECT_NO = @PROJECT_NO
								   and LOG_STATUS = 'B02'
								   order by CRT_DATE desc
							   ) PBL on PBA.PROJECT_NO = PBL.PROJECT_NO

							where PBA.PROJECT_AW_STATUS = 'B02'--調整狀態：調整審核中
								and PBA.PROJECT_NO = @PROJECT_NO
							order by PBA.PROJ_ADJ_ID, PCA.PROGRESS  ");
			sql.Append(";");
			// 各次調整期程資料
			sql.AppendLine(@"select 
								PBA.PROJ_ADJ_ID,
								PBA.CONTROL_DATE1,
								PBA.CP_KIND,
								PBA.SCHE_TYPE ,
								PCA.ESTIMATED_ENDDATE,
								CCI.CTRL_POINT,
								PBA.APPRV_DATE as ITEM_DATE, --調整核准日期
								PCA.CHECKITEM_NAME
							from PROJECT_BASIC_ADJ PBA (nolock)
							left join PROJECT_CHECKITEM_ADJ PCA (nolock)
								on PBA.PROJ_ADJ_ID = PCA.PROJ_ADJ_ID 
							left join CODE_CHECKPOINT_ITEM CCI (nolock)
								on PCA.CHECKITEM_SEQ = CCI.SEQ
							where PBA.PROJECT_NO = @PROJECT_NO
								and PBA.PROJECT_AW_STATUS = 'B05' -- 期程調整審核通過
							order by PBA.PROJ_ADJ_ID , PCA.PROGRESS ");

			var result = await ExecuteQueryMultipleAsync<ProjectScheOverviewModel, ProjectScheOverviewModel>(sql.ToString(), new { PROJECT_NO });
			return (result.result1.ToList(), result.result2.ToList());
		}

		/// <summary>
		/// 取得立案期程
		/// </summary>
		/// <param name="PROJECT_NO"></param>
		/// <returns></returns>
		public async Task<List<ProjectScheOverviewModel>> GetProjectOriginSchedule(string PROJECT_NO)
		{
			string sql = @" select 
								ISNULL(PCEH.CONTROL_DATE1,PCE.CONTROL_DATE1 ) as CONTROL_DATE1 ,
								PCEH.CONTROL_DATE1 ,
								ESTIMATED_ENDDATE ,
								CCI.CTRL_POINT, 
								PBL.CRT_DATE as ITEM_DATE, --立案審核通過日期
								PCH.CHECKITEM_NAME ,
								PB.CP_KIND
							from PROJECT_CHECKITEM_HIS PCH (nolock)
							left join CODE_CHECKPOINT_ITEM CCI (nolock)
								on PCH.CHECKITEM_SEQ = CCI.SEQ
							left join PROJECT_CONTROL_EXECUTE_HIS PCEH  (nolock)
								on PCH.HIS_ID  = PCEH.HIS_ID
							left join PROJECT_CONTROL_EXECUTE PCE (nolock)
								on PCH.PROJECT_NO = PCE.PROJECT_NO
							left join PROJECT_BASIC PB (nolock)
								on PCH.PROJECT_NO = PB.PROJECT_NO 
							left join PROJECT_BASIC_LOG PBL (nolock)
								on PCH.PROJECT_NO = PBL.PROJECT_NO 
									and PBL.LOG_STATUS = '4'
							where PCH.PROJECT_NO = @PROJECT_NO
								and PCH.HIS_ID = 
									(select top 1 HIS_ID  
									 from PROJECT_CHECKITEM_HIS PCH (nolock)
									 where PROJECT_NO = @PROJECT_NO)
							order by PCH.SEQ ";

			return (await ExecuteQueryAsync<ProjectScheOverviewModel>(sql, new { PROJECT_NO })).ToList();
		}
	}
}
