using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
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
    public class ProjectExecuteDac : Dac, IProjectExecuteDac
    {
        public ProjectExecuteDac(IConnectionControlCenter connectionControlCenter,
           IHttpContextAccessor httpContextAccessor,
           ISqlTrace trace,
           IUserProfile profile,
           IParameterAdaptor ParameterAdaptor,
           IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }
        #region 每月辦理情形
        /// <summary>
        /// 取得計畫每月辦理情形(單筆)
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="SEQ"></param>
        /// <returns></returns>
        public async Task<ProjectEngineeringProgressTableModel> GetProjecFillExecute(string PROJECT_NO, string SEQ)
        {
            string sql = @"SELECT TOP 1
                                    M2.SEQ,
                                    ISNULL(M2.PROJECT_NO, @PROJECT_NO) AS PROJECT_NO,
                                    M1.PROJECT_YEAR AS YEAR,
                                    M1.PROJECT_MONTH AS MONTH,
                                    M2.IPC_RES_PRG,
                                    M2.IPC_ACT_PRG,
                                    M2.EXECUTE_CONDITION,
                                    M2.ASSISTANT_ITEM,
                                    M2.IS_SEND ";
            //沒有SEQ增加上一個月執行情形欄位
            if (string.IsNullOrEmpty(SEQ))
            {
                sql += @"
            , LEAD(M2.EXECUTE_CONDITION) OVER (ORDER BY M1.SEQ DESC) AS LAST_EXECUTE_CONDITION ";
            }

            sql += @"FROM PROJECT_FILL_CYCLE (NOLOCK) M1
                     LEFT JOIN PROJECT_ENGINEERING_PROGRESS (NOLOCK) M2 
                       ON M1.PROJECT_YEAR = M2.YEAR 
                      AND M1.PROJECT_MONTH = M2.MONTH
                      AND M2.PROJECT_NO = @PROJECT_NO ";

            if (!string.IsNullOrEmpty(SEQ))
            {
                sql += " WHERE M2.SEQ = @SEQ ";
            }

            sql += " ORDER BY M1.SEQ DESC";
            return await ExecuteQueryFirstOrDefaultAsync<ProjectEngineeringProgressTableModel>(sql, new { PROJECT_NO, SEQ });
        }


        /// <summary>
        /// 取得計畫每月辦理情形清單
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="DATA_TYPE">資料種類 0:預設近5個月 1:全部</param>
        /// <returns></returns>
        public async Task<List<ProjectEngineeringProgressGridModel>> GetProjecFillExecuteList(string PROJECT_NO, string DATA_TYPE)
        {
            string sql = $@"
                SELECT { (DATA_TYPE == "0" ? "TOP 5" : string.Empty) }
                    M2.SEQ,
                    M2.PROJECT_NO,
                    M2.YEAR,
                    M2.MONTH,
                    M2.IPC_RES_PRG,
                    M2.IPC_ACT_PRG,
                    M2.TEN_RES_PRG,
                    M2.TEN_ACT_PRG,
                    M2.EXECUTE_CONDITION,
                    M2.ASSISTANT_ITEM,
                    M2.SEND_DATE,
                    M2.DISREGARD,
                   	M2.OVERDUE_DAY 
                FROM PROJECT_BASIC (NOLOCK) M1 
                INNER JOIN PROJECT_ENGINEERING_PROGRESS (NOLOCK) M2 
                    ON M1.PROJECT_NO = M2.PROJECT_NO
                INNER JOIN PROJECT_FILL_CYCLE (NOLOCK) M3 
				    ON M3.PROJECT_YEAR = M2.YEAR AND M3.PROJECT_MONTH = M2.MONTH
                WHERE M1.PROJECT_NO = @PROJECT_NO 
                ORDER BY M2.YEAR DESC, M2.MONTH DESC ";
            return (await ExecuteQueryAsync<ProjectEngineeringProgressGridModel>(sql, new { PROJECT_NO })).ToList();
        }

        /// <summary>
        /// 新增計畫工程進度
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int InsertProjectEngineeringProgress(ProjectEngineeringProgressModel model)
        {
            string sql = $@"
                INSERT INTO PROJECT_ENGINEERING_PROGRESS
                    (PROJECT_NO
                    ,YEAR
                    ,MONTH
                    ,IPC_RES_PRG
                    ,IPC_ACT_PRG
                    ,IS_DELAY
                    ,EXECUTE_CONDITION
                    ,ASSISTANT_ITEM
                    ,CRT_USER
                    ,CRT_DATE
                    ,MDF_USER
                    ,MDF_DATE)
                OUTPUT INSERTED.SEQ
                VALUES
                    (@PROJECT_NO
                    ,@YEAR
                    ,@MONTH
                    ,@IPC_RES_PRG
                    ,@IPC_ACT_PRG
                    ,@IS_DELAY
                    ,@EXECUTE_CONDITION
                    ,@ASSISTANT_ITEM
                    ,@CRT_USER
                    ,{DTNow}
                    ,@MDF_USER
                    ,{DTNow})";
            return ExecuteQuery<int>(sql, model, isSetUSER: true).FirstOrDefault();
        }

        /// <summary>
        /// 修改計畫工程進度
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void UpdateProjectEngineeringProgress(ProjectEngineeringProgressModel model)
        {
            string sql = $@"
                UPDATE PROJECT_ENGINEERING_PROGRESS
                SET IPC_RES_PRG = @IPC_RES_PRG,
                    IPC_ACT_PRG = @IPC_ACT_PRG,
                    IS_DELAY = @IS_DELAY,
                    EXECUTE_CONDITION = @EXECUTE_CONDITION,
                    ASSISTANT_ITEM = @ASSISTANT_ITEM,
                    MDF_USER = @MDF_USER,
                    MDF_DATE = {DTNow}
                WHERE SEQ = @SEQ ";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 比較兩個月執行情形相似度
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<float> ExecutionProgress(ProjectEngineeringProgressModel model)
        {

            string sql = "select dbo.FN_Resemble_onebyone(@EXECUTE_CONDITION,@LAST_EXECUTE_CONDITION)";
            return await ExecuteQueryFirstOrDefaultAsync<float>(sql, new
            {
                model.EXECUTE_CONDITION,
                model.LAST_EXECUTE_CONDITION
            });
        }

            /// <summary>
            /// 新增計畫落後原因
            /// </summary>
            /// <param name="model"></param>
            public void InsertProjectDelayCausal(ProjectDelayCausalInsertModel model)
        {
            string sql = $@"
                INSERT INTO PROJECT_DELAY_CAUSAL
                    (PROJECT_NO
                    ,DATA_YEAR
                    ,DATA_MONTH
                    ,DELAY_KIND       
                    ,CRT_USER
                    ,CRT_DATE
                    ,MDF_USER
                    ,MDF_DATE)
                VALUES
                    (@PROJECT_NO
                    ,@DATA_YEAR
                    ,@DATA_MONTH
                    ,@DELAY_KIND       
                    ,@CRT_USER
                    ,{DTNow}
                    ,@MDF_USER
                    ,{DTNow})";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 修改計畫落後原因
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectDelayCausal(ProjectDelayCausalInsertModel model)
        {
            string sql = $@"
                UPDATE PROJECT_DELAY_CAUSAL
                SET DELAY_KIND = @DELAY_KIND,
                    MDF_USER = @MDF_USER,
                    MDF_DATE = {DTNow}
                WHERE PROJECT_NO = @PROJECT_NO
                AND DATA_YEAR = @DATA_YEAR
                AND DATA_MONTH = @DATA_MONTH";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 刪除計畫落後原因
        /// </summary>
        /// <param name="model"></param>
        public void DeleteProjectDelayCausal(ProjectDelayCausalInsertModel model)
        {
            string sql = $@"
                DELETE PROJECT_DELAY_CAUSAL
                WHERE PROJECT_NO = @PROJECT_NO
                AND DATA_YEAR = @DATA_YEAR
                AND DATA_MONTH = @DATA_MONTH";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 檢查是否落後(工程類:根據控制點CTRL_POINT；非工程類)
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="lastDay">填報月最後一天</param>
        /// <param name="ctrlPoint"></param>
        /// <returns></returns>
        public async Task<bool> CheckIsDelay(string projectNo, DateTime lastDay, string ctrlPoint)
        {
            string joinSql = string.Empty;
            if (!string.IsNullOrEmpty(ctrlPoint))
            {
                string condition = ctrlPoint == "A" ? "<" : ">";
                joinSql = $@"
                    INNER JOIN (SELECT C.ESTIMATED_ENDDATE,C.PROJECT_NO
			                    FROM PROJECT_CHECKITEM (NOLOCK) C 
			                    LEFT JOIN CODE_CHECKPOINT_ITEM (NOLOCK) B 
				                    ON C.CHECKITEM_SEQ = B.SEQ  
			                    WHERE C.PROJECT_NO = @PROJECT_NO  AND B.CTRL_POINT = @CTRL_POINT) D
	                ON A.PROJECT_NO = D.PROJECT_NO 
		                AND A.ESTIMATED_ENDDATE {condition} D.ESTIMATED_ENDDATE";
            }

            string sql = $@"
                SELECT CASE WHEN Count(A.PROJECT_NO) >= 1
                       THEN 1
                       ELSE 0
                       END
                FROM PROJECT_CHECKITEM (NOLOCK) A
                {joinSql}
                WHERE A.PROJECT_NO = @PROJECT_NO 				 
                AND CONVERT(VARCHAR, A.ESTIMATED_ENDDATE, 111) <= CONVERT(VARCHAR, @LastDay, 111)　--預計完成日期＜＝ 填報月最後一天 
                AND A.ACTUAL_ENDDATE IS NULL";
            return await ExecuteQueryFirstOrDefaultAsync<bool>(sql, new { PROJECT_NO = projectNo,LastDay = lastDay, CTRL_POINT = ctrlPoint });
        }

        /// <summary>
        /// 取得執行類別，工程類or非工程類
        /// </summary>
        /// <param name="projectNo"></param>
        /// <returns></returns>
        public async Task<string> GetCpKind(string projectNo)
        {
            string sql = $@"
                SELECT CP_KIND
                FROM PROJECT_BASIC (NOLOCK) 
                WHERE PROJECT_NO = @PROJECT_NO ";
            return await ExecuteQueryFirstOrDefaultAsync<string>(sql, new { PROJECT_NO = projectNo});
        }
        #endregion

        #region 落後原因分析
        /// <summary>
        /// 取得計畫落後原因分析(單筆)
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="SEQ"></param>
        /// <returns></returns>
        public async Task<ProjectDelayCausalModel> GetProjectDelayCausal(string PROJECT_NO, string SEQ = "")
        {
            string sql = @"
                SELECT 
                    SEQ,
                    PROJECT_NO,
                    DATA_YEAR,
                    DATA_MONTH,
                    DELAY_KIND,
                    DELAY_CLASS_C,
                    DELAY_SUBCLASS_C,
                    DELAY_RESPON,
                    DELAY_CAUSAL,
                    SOLUTION,
                    COORDINATION,
                    DEADLINES
                FROM PROJECT_DELAY_CAUSAL (NOLOCK) M1";
            if (string.IsNullOrEmpty(SEQ))
            {
                sql += @"
                    INNER JOIN (
                        SELECT TOP 1 PROJECT_YEAR,PROJECT_MONTH
                        FROM PROJECT_FILL_CYCLE (NOLOCK)
                        ORDER BY SEQ DESC
                    ) M2 
                        ON M2.PROJECT_YEAR = M1.DATA_YEAR 
                        AND M2.PROJECT_MONTH = M1.DATA_MONTH
                    WHERE PROJECT_NO = @PROJECT_NO";
            }
            else
            {
                sql += @"
                    WHERE SEQ = @SEQ";
            }
            return await ExecuteQueryFirstOrDefaultAsync<ProjectDelayCausalModel>(sql, new { PROJECT_NO, SEQ });
        }

        /// <summary>
        /// 取得計畫落後原因分析
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="DATA_TYPE">資料種類 0:預設近5個月 1:全部</param>
        /// <returns></returns>
        public async Task<List<ProjectDelayCausalModel>> GetProjectDelayCausalList(string PROJECT_NO, string DATA_TYPE)
        {
            string sql = $@"
                SELECT { (DATA_TYPE == "0" ? "TOP 5" : string.Empty) }
                    M1.SEQ,
                    M1.PROJECT_NO,
                    M1.DATA_YEAR,
                    M1.DATA_MONTH,
                    M1.DELAY_KIND,
                    M2.SET_VALUE AS DELAY_CLASS_C,
                    M3.DELAY_CLASS_SUB_ITEM AS DELAY_SUBCLASS_C,
                    M4.SET_VALUE AS DELAY_RESPON,
                    M1.DELAY_CAUSAL,
                    M1.SOLUTION,
                    M1.COORDINATION,
                    M1.DEADLINES
                FROM PROJECT_DELAY_CAUSAL (NOLOCK) M1 
                LEFT JOIN SET_PARAM (NOLOCK) M2 
                    ON M2.SET_ITEM = 'DELAY_TYPE' AND M1.DELAY_CLASS_C = M2.SET_TYPE
                LEFT JOIN CODE_DELAY_CLASS (NOLOCK) M3 
                    ON M3.DELAY_CLASS_SUB_ID = M1.DELAY_SUBCLASS_C 
                LEFT JOIN SET_PARAM (NOLOCK) M4 
                    ON M4.SET_ITEM = 'DELAY_RESPON' AND M1.DELAY_RESPON = M4.SET_TYPE
                WHERE M1.PROJECT_NO = @PROJECT_NO
                ORDER BY M1.DATA_YEAR DESC, M1.DATA_MONTH DESC";
            return (await ExecuteQueryAsync<ProjectDelayCausalModel>(sql, new { PROJECT_NO })).ToList();
        }

        /// <summary>
        /// 修改計畫落後原因
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectDelayCausal(ProjectDelayCausalModel model)
        {
            string sql = $@"
                UPDATE PROJECT_DELAY_CAUSAL
                SET DELAY_CLASS_C = @DELAY_CLASS_C,
                    DELAY_SUBCLASS_C = @DELAY_SUBCLASS_C,
                    DELAY_RESPON = @DELAY_RESPON,
                    DELAY_CAUSAL = @DELAY_CAUSAL,
                    SOLUTION = @SOLUTION,
                    COORDINATION = @COORDINATION,
                    DEADLINES = @DEADLINES,
                    MDF_USER = @MDF_USER,
                    MDF_DATE = {DTNow}
                WHERE SEQ = @SEQ ";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 刪除計畫落後原因
        /// </summary>
        /// <param name="SEQ"></param>
        /// <returns></returns>
        public void DeleteProjectDelayCausal(string SEQ)
        {
            string sql = $@"
                DELETE PROJECT_DELAY_CAUSAL
                WHERE SEQ = @SEQ ";
            ExecuteCommand(sql, new { SEQ });
        }
        #endregion

        #region 檢核點完成日期
        /// <summary>
        /// 取得檢核點完成日期
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<ProjectFillCkptComModel> GetProjectFillCkptCom(string PROJECT_NO)
        {
            string sql = @" select 
                                main.PROJECT_NO,
                                main.PROJECT_AW_STATUS,
                                main.CP_KIND,
                                main.CONTRACT_FINISH_DATE,
                                main.IS_USER_FTY_DATA,
                                main.TENDER_AWARDING_AMT,
								main.PROCUREMENT_AMT,
                                main.IS_TYCG_PROJECT,
                                exe.PCC_PROJECT_UID,
                                exe.PCC_PROJECT_NO,
                                exe.PCC_PROJECT_NAME,
                                exe.REAL_CONTACT,
                                exe.REAL_TEL,
                                exe.REAL_EMAIL,
                                exe.FACTORY_CONTACT,
                                exe.FACTORY_TEL,
								progress.IS_SEND
                            from PROJECT_BASIC  (nolock) main
                            inner join PROJECT_CONTROL_EXECUTE (nolock) exe
                                on main.PROJECT_NO = exe.PROJECT_NO
							left join (select PROJECT_NO,IS_SEND from PROJECT_ENGINEERING_PROGRESS (nolock) progressData
										inner join (select top(1) PROJECT_YEAR, PROJECT_MONTH 
													from PROJECT_FILL_CYCLE (nolock)
													order by SEQ desc ) cycleData
										on progressData.YEAR = cycleData.PROJECT_YEAR and progressData.MONTH = cycleData.PROJECT_MONTH) progress
							on main.PROJECT_NO = progress.PROJECT_NO
                            where main.PROJECT_NO = @PROJECT_NO";

            return await ExecuteQueryFirstOrDefaultAsync<ProjectFillCkptComModel>(sql, new { PROJECT_NO });
        }

        /// <summary>
        /// 儲存檢核點完成日期 - 更新計畫基本資料 
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectBasicCkptCom(ProjectFillCkptComModel model)
        {
            string sql = $@"update PROJECT_BASIC 
                            set CONTRACT_FINISH_DATE = @CONTRACT_FINISH_DATE,
                                TENDER_AWARDING_AMT = @TENDER_AWARDING_AMT,
                                PROCUREMENT_AMT = @PROCUREMENT_AMT,
                                IS_TYCG_PROJECT = @IS_TYCG_PROJECT,
                                IS_USER_FTY_DATA = @IS_USER_FTY_DATA,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            where PROJECT_NO = @PROJECT_NO";
            ExecuteCommand(sql, model);
        }

        ///// <summary>
        ///// 更新依契約預定完工日
        ///// </summary>
        ///// <param name="PROJECT_NO"></param>
        ///// <param name="CONTRACT_FINISH_DATE"></param>
        //public void UpdateContactFinishDate(string PROJECT_NO, DateTime CONTRACT_FINISH_DATE)
        //{
        //    string sql = $@"update PROJECT_BASIC 
        //                    set CONTRACT_FINISH_DATE = @CONTRACT_FINISH_DATE,
        //                        MDF_USER = @MDF_USER,
        //                        MDF_DATE = {DTNow}
        //                    where PROJECT_NO = @PROJECT_NO";
        //    ExecuteCommand(sql, new
        //    {
        //        PROJECT_NO,
        //        CONTRACT_FINISH_DATE,
        //        MDF_USER = UserId
        //    });
        //}

        /// <summary>
        /// 取得當期辦理情形資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<ProjectEngineeringProgressModel> GetCurrentProjectEngineeringProgress(string PROJECT_NO)
        {
            string sql = @"select 
                                SEQ,
                                PROJECT_NO,
                                YEAR,
                                MONTH,
                                IPC_RES_PRG,
                                IPC_ACT_PRG,
                                EXECUTE_CONDITION,
                                ASSISTANT_ITEM,
                                IS_DELAY
                            from PROJECT_ENGINEERING_PROGRESS (nolock)
                            where PROJECT_NO = @PROJECT_NO
                                and YEAR = (select TOP 1 PROJECT_YEAR from PROJECT_FILL_CYCLE order by CRT_DATE desc)
                                and MONTH = (select TOP 1 PROJECT_MONTH from PROJECT_FILL_CYCLE order by CRT_DATE desc)";

            return await ExecuteQueryFirstOrDefaultAsync<ProjectEngineeringProgressModel>(sql, new { PROJECT_NO });
        }

        /// <summary>
        /// 更新檢核點完成日期 - 工程發包金額、決標金額
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="FLOOR_PRICE"></param>
        /// <param name="BID_TOTAL_PRICE"></param>
        public void UpdateProjFillChkptComEngBid(string PROJECT_NO, int FLOOR_PRICE, int BID_TOTAL_PRICE)
        {
            string sql = $@"update PROJECT_ENGINEERING_BID 
                            set FLOOR_PRICE = @FLOOR_PRICE,
                                BID_TOTAL_PRICE = @BID_TOTAL_PRICE,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            where PROJECT_NO = @PROJECT_NO";
            ExecuteCommand(sql, new
            {
                PROJECT_NO,
                FLOOR_PRICE,
                BID_TOTAL_PRICE,
                MDF_USER = UserId
            });
        }

        /// <summary>
        /// 更新檢核點完成日期 - 實際完成日期
        /// </summary>
        /// <param name="models"></param>
        public void UpdateActualEndDate(List<ProjectCusCheckpointModel> models)
        {
            string sql = $@"update PROJECT_CHECKITEM 
                            set ACTUAL_ENDDATE = @ACTUAL_ENDDATE_FOR_SAVE,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            where SEQ = @SEQ";
            ExecuteCommand(sql, models);
        }

        /// <summary>
        /// 同步更新檢核點完成日期 - 實際完成日期
        /// </summary>
        /// <param name="model"></param>
        public void UpdateActualEndDateSync(ProjectCusCheckpointModel model)
        {
            string sql = $@"update PROJECT_CHECKITEM
                            set
	                            ACTUAL_ENDDATE = @ACTUAL_ENDDATE_FOR_SAVE,
	                            MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            where PROJECT_NO = @PROJECT_NO and CHECKITEM_SEQ > @CHECKITEM_SEQ";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 更新計畫聯繫資訊
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectFillContact(ProjectFillCkptComModel model)
        {
            string sql = $@"update PROJECT_CONTROL_EXECUTE 
                            set 
                                REAL_CONTACT = @REAL_CONTACT,
                                REAL_TEL = @REAL_TEL,
                                REAL_EMAIL = @REAL_EMAIL,
                                PCC_PROJECT_UID = @PCC_PROJECT_UID,
                                PCC_PROJECT_NO = @PCC_PROJECT_NO,
                                FACTORY_CONTACT = @FACTORY_CONTACT,
                                FACTORY_TEL = @FACTORY_TEL,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            where PROJECT_NO = @PROJECT_NO";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 清空當次週期已填報的檢核點完成日期、辦理情形及落後原因
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="projFillCycle"></param>
        public void ClearCycleData(string PROJECT_NO, ProjectFillCycleModel projFillCycle)
        {
            string sql = @"
                update chkItem
                set ACTUAL_ENDDATE = null
                from PROJECT_CHECKITEM chkItem
                inner join CODE_CHECKPOINT_ITEM chkPoint on chkItem.CHECKITEM_SEQ = chkPoint.SEQ
                where chkItem.PROJECT_NO = @PROJECT_NO 
	                and chkItem.MDF_DATE >= @FILL_START_DATE
	                and chkItem.SEQ > (
		                select m1.SEQ from PROJECT_CHECKITEM m1
		                inner join CODE_CHECKPOINT_ITEM m2 on m1.CHECKITEM_SEQ = m2.SEQ
		                where m1.PROJECT_NO = @PROJECT_NO and m2.CTRL_POINT = 'A'
	                );
                update PROJECT_ENGINEERING_PROGRESS 
                set EXECUTE_CONDITION = '', 
	                ASSISTANT_ITEM = '', 
	                IPC_RES_PRG = null, 
	                IPC_ACT_PRG = null, 
	                TEN_RES_PRG = null, 
	                TEN_ACT_PRG = null
                where PROJECT_NO = @PROJECT_NO and YEAR = @PROJECT_YEAR and MONTH = @PROJECT_MONTH;
                delete PROJECT_DELAY_CAUSAL 
                where PROJECT_NO = @PROJECT_NO and DATA_YEAR = @PROJECT_YEAR and DATA_MONTH = @PROJECT_MONTH";
            ExecuteCommand(sql, new
            {
                PROJECT_NO,
                projFillCycle.PROJECT_YEAR,
                projFillCycle.PROJECT_MONTH,
                projFillCycle.FILL_START_DATE
            });
        }
        #endregion

        #region 其他資料

        #region 招標情形
        /// <summary>
        /// 取得其他資料招標情形
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectBidModel>> GetProjectBid(string PROJECT_NO)
        {
            string sql = $@"SELECT PROJECT_NO,
                                BID_KIND,
	                            dbo.FN_GetSetParam('BID_KIND', BID_KIND) AS BID_NAME,	                            
	                            AWARD_BID_DATE,
	                            BID_TENDER,
                                2 AS editType
                            FROM PROJECT_BID (NOLOCK)
                            WHERE PROJECT_NO = @PROJECT_NO";
            return (await ExecuteQueryAsync<ProjectBidModel>(sql, new { PROJECT_NO })).ToList();
        }

        /// <summary>
        /// 取得招標情形歷程
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectBidDetailModel>> GetProjectBidDetail(string PROJECT_NO)
        {
            string sql = $@"SELECT SEQ,
	                            PROJECT_NO,
	                            BID_KIND,
	                            DETAIL_TYPE,
	                            DETAIL_DATE,
	                            DETAIL_REASON
                            FROM PROJECT_BID_DETAIL(NOLOCK)
                            WHERE PROJECT_NO = @PROJECT_NO";
            return (await ExecuteQueryAsync<ProjectBidDetailModel>(sql, new { PROJECT_NO })).ToList();
        }

        /// <summary>
        /// 新增其他資料招標情形
        /// </summary>
        /// <param name="model"></param>
        public void InsertProjectBid(List<ProjectBidModel> model)
        {
            string sql = $@"INSERT INTO PROJECT_BID
                                (PROJECT_NO,
                                BID_KIND,
                                AWARD_BID_DATE,
                                BID_TENDER,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            VALUES
                                (@PROJECT_NO,
                                @BID_KIND,
                                @AWARD_BID_DATE,
                                @BID_TENDER,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 更新其他資料招標情形
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectBid(List<ProjectBidModel> model)
        {
            string sql = $@"UPDATE PROJECT_BID
                                SET AWARD_BID_DATE = @AWARD_BID_DATE,
                                BID_TENDER = @BID_TENDER,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE PROJECT_NO = @PROJECT_NO AND BID_KIND = @BID_KIND";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 新增招標情形歷程
        /// </summary>
        /// <param name="model"></param>
        public void InsertProjectBidDetail(List<ProjectBidDetailModel> model)
        {
            string sql = $@"INSERT INTO PROJECT_BID_DETAIL
                                (PROJECT_NO,
                                BID_KIND,
                                DETAIL_TYPE,
                                DETAIL_DATE,
                                DETAIL_REASON,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            VALUES
                                (@PROJECT_NO,
                                @BID_KIND,
                                @DETAIL_TYPE,
                                @DETAIL_DATE,
                                @DETAIL_REASON,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 更新招標情形歷程
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectBidDetail(List<ProjectBidDetailModel> model)
        {
            string sql = $@"UPDATE PROJECT_BID_DETAIL
                                SET DETAIL_DATE = @DETAIL_DATE,
                                DETAIL_REASON = @DETAIL_REASON,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE SEQ = @SEQ";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 刪除招標情形歷程
        /// </summary>
        /// <param name="model"></param>
        public void DeleteProjectBidDetail(List<ProjectBidDetailModel> model)
        {
            string sql = $@"DELETE PROJECT_BID_DETAIL WHERE SEQ = @SEQ";
            ExecuteCommand(sql, model);
        }
        #endregion

        #region 相關活動
        /// <summary>
        /// 取得其他資料相關活動
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectActivityModel>> GetProjectActivity(string PROJECT_NO)
        {
            string sql = $@"SELECT SEQ,
	                            PROJECT_NO,
	                            ACTIVITY_KIND,
                                dbo.FN_GetSetParam('ACTIVITY_KIND', ACTIVITY_KIND) as ACTIVITY_KIND_NAME,
	                            IS_ACTIVITY,
	                            ACTIVITY_DATE,
	                            ACTIVITY_NAME,
                                2 AS editType
                            FROM PROJECT_ACTIVITY(NOLOCK)
                            WHERE PROJECT_NO = @PROJECT_NO";
            return (await ExecuteQueryAsync<ProjectActivityModel>(sql, new { PROJECT_NO })).ToList();
        }

        /// <summary>
        /// 新增相關活動
        /// </summary>
        /// <param name="model"></param>
        public void InsertProjectActivity(List<ProjectActivityModel> model)
        {
            string sql = $@"INSERT INTO PROJECT_ACTIVITY 
                                (PROJECT_NO,
                                ACTIVITY_KIND,
                                IS_ACTIVITY,
                                ACTIVITY_DATE,
                                ACTIVITY_NAME,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            VALUES
                                (@PROJECT_NO,
                                @ACTIVITY_KIND,
                                @IS_ACTIVITY,
                                @ACTIVITY_DATE,
                                @ACTIVITY_NAME,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 更新相關活動
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectActivity(List<ProjectActivityModel> model)
        {
            string sql = $@"UPDATE PROJECT_ACTIVITY
                                SET IS_ACTIVITY = @IS_ACTIVITY,
                                ACTIVITY_DATE = @ACTIVITY_DATE,
                                ACTIVITY_NAME = @ACTIVITY_NAME,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE SEQ = @SEQ";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 刪除相關活動
        /// </summary>
        /// <param name="model"></param>
        public void DeleteProjectActivity(List<ProjectActivityModel> model)
        {
            string sql = $@"DELETE PROJECT_ACTIVITY WHERE SEQ = @SEQ";
            ExecuteCommand(sql, model);
        }
        #endregion

        #region 相關審查
        /// <summary>
        /// 取得其他資料相關審查
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectReviewModel>> GetProjectReview(string PROJECT_NO)
        {
            string sql = $@"SELECT SEQ,
	                            PROJECT_NO,
	                            REVIEW_KIND,
                                dbo.FN_GetSetParam('REVIEW_KIND', REVIEW_KIND) as REVIEW_NAME,
	                            IS_REVIEW,
	                            SEND_DATE,
	                            REVIEW_DATE,
                                OTH_RVWNAME,
                                2 AS editType
                            FROM PROJECT_REVIEW (NOLOCK)
                            WHERE PROJECT_NO = @PROJECT_NO
                            ORDER BY REVIEW_KIND";
            return (await ExecuteQueryAsync<ProjectReviewModel>(sql, new { PROJECT_NO })).ToList();
        }

        /// <summary>
        /// 新增相關審查
        /// </summary>
        /// <param name="model"></param>
        public void InsertProjectReview(List<ProjectReviewModel> model)
        {
            string sql = $@"INSERT INTO PROJECT_REVIEW 
                                (PROJECT_NO,
                                REVIEW_KIND,
                                IS_REVIEW,
                                SEND_DATE,
                                REVIEW_DATE,
                                OTH_RVWNAME,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            VALUES
                                (@PROJECT_NO,
                                @REVIEW_KIND,
                                @IS_REVIEW,
                                @SEND_DATE,
                                @REVIEW_DATE,
                                @OTH_RVWNAME,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 更新相關審查
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectReview(List<ProjectReviewModel> model)
        {
            string sql = $@"UPDATE PROJECT_REVIEW
                                SET IS_REVIEW = @IS_REVIEW,
                                SEND_DATE = @SEND_DATE,
                                REVIEW_DATE = @REVIEW_DATE,
                                OTH_RVWNAME = @OTH_RVWNAME,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE SEQ = @SEQ";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 刪除相關審查
        /// </summary>
        /// <param name="model"></param>
        public void DeleteProjectReview(List<ProjectReviewModel> model)
        {
            string sql = $@"DELETE PROJECT_REVIEW WHERE SEQ = @SEQ";
            ExecuteCommand(sql, model);
        }
        #endregion

        #region 廠商資訊
        /// <summary>
        /// 取得其他資料廠商資訊
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectTenderModel>> GetProjectTender(string PROJECT_NO)
        {
            string sql = $@"SELECT SEQ,
	                            PROJECT_NO,
	                            TENDER_KIND,
                                dbo.FN_GetSetParam('TENDER_KIND', TENDER_KIND) as TENDER_KIND_NAME,
                                TENDER_NAME,
	                            REG_NO,
	                            TENDER_ADDR,
	                            CONTACT_NAME,
                                CONTACT_PHONE,
                                CONTACT_EMAIL
                            FROM PROJECT_TENDER (NOLOCK)
                            WHERE PROJECT_NO = @PROJECT_NO";
            return (await ExecuteQueryAsync<ProjectTenderModel>(sql, new { PROJECT_NO })).ToList();
        }

        /// <summary>
        /// 新增廠商資訊
        /// </summary>
        /// <param name="model"></param>
        public void InsertProjectTender(List<ProjectTenderModel> model)
        {
            string sql = $@"INSERT INTO PROJECT_TENDER 
                                (PROJECT_NO,
                                TENDER_KIND,
                                TENDER_NAME,
                                REG_NO,
                                TENDER_ADDR,
                                CONTACT_NAME,
                                CONTACT_PHONE,
                                CONTACT_EMAIL,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            VALUES
                                (@PROJECT_NO,
                                @TENDER_KIND,
                                @TENDER_NAME,
                                @REG_NO,
                                @TENDER_ADDR,
                                @CONTACT_NAME,
                                @CONTACT_PHONE,
                                @CONTACT_EMAIL,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 更新廠商資訊
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectTender(List<ProjectTenderModel> model)
        {
            string sql = $@"UPDATE PROJECT_TENDER
                                SET TENDER_KIND = @TENDER_KIND,
                                TENDER_NAME = @TENDER_NAME,
                                REG_NO = @REG_NO,
                                TENDER_ADDR = @TENDER_ADDR,
                                CONTACT_NAME = @CONTACT_NAME,
                                CONTACT_PHONE = @CONTACT_PHONE,
                                CONTACT_EMAIL = @CONTACT_EMAIL,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE SEQ = @SEQ";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 刪除廠商資訊
        /// </summary>
        /// <param name="model"></param>
        public void DeleteProjectTender(List<ProjectTenderModel> model)
        {
            string sql = $@"DELETE PROJECT_TENDER WHERE SEQ = @SEQ";
            ExecuteCommand(sql, model);
        }
        #endregion
        #endregion

        #region 執行情形送出
        /// <summary>
        /// 檢查計畫聯繫資訊必填欄位是否均填
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<bool> CheckProjectContactValid(string PROJECT_NO)
        {
            string sql = @" select count(PROJECT_NO) 
                            from PROJECT_CONTROL_EXECUTE (nolock) 
                            where PROJECT_NO = @PROJECT_NO 
                                and (REAL_CONTACT is null or
                                     REAL_TEL is null or
                                     REAL_EMAIL is null )";
            return await ExecuteQueryFirstOrDefaultAsync<int>(sql, new { PROJECT_NO }) == 0;
        }

        /// <summary>
        /// 檢查計畫檢核點項目最後一筆實際完成日期時否有填
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<bool> ChkLastChkptActualEndDateIsFilled(string PROJECT_NO)
        {
            string sql = @"select TOP 1 ACTUAL_ENDDATE 
                           from PROJECT_CHECKITEM (nolock)
                           where PROJECT_NO = @PROJECT_NO 
                           order by PROGRESS desc";

            var actualEndDate = await ExecuteQueryFirstOrDefaultAsync<string>(sql, new { PROJECT_NO });
            return !string.IsNullOrEmpty(actualEndDate);
        }

        /// <summary>
        /// 檢查當期執行情形是否已送出
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<bool> CheckProjectFillExecuteIsSend(string PROJECT_NO)
        {
            string sql = @"select COUNT(*) 
                           from PROJECT_ENGINEERING_PROGRESS (nolock) eng
						   inner join (select top (1) PROJECT_YEAR, 
                                                      PROJECT_MONTH 
                                       from PROJECT_FILL_CYCLE 
                                       order by SEQ desc ) cycleData
						   on eng.YEAR = cycleData.PROJECT_YEAR 
                               and eng.MONTH = cycleData.PROJECT_MONTH
                           where PROJECT_NO = @PROJECT_NO
                               and eng.IS_SEND = 1 --已送出
                               and eng.SEND_DATE IS NOT NULL --送出時間";
            return await ExecuteQueryFirstOrDefaultAsync<int>(sql, new
            {
                PROJECT_NO
            }) > 0;
        }

        /// <summary>
        /// 送出當期執行情形
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="YEAR"></param>
        /// <param name="MONTH"></param>
        public void SendProjectFillExecute(string PROJECT_NO, string YEAR, string MONTH)
        {
            string sql = $@"update PROJECT_ENGINEERING_PROGRESS 
                            set IS_SEND = 1,
                                SEND_DATE = {DTNow} 
                            where PROJECT_NO = @PROJECT_NO
                                and YEAR = @YEAR 
                                and MONTH = @MONTH";
            ExecuteCommand(sql, new
            {
                PROJECT_NO,
                YEAR,
                MONTH
            });
        }

        /// <summary>
        /// 檢查計畫是否為工程類
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<bool> CheckIsEngineeringType(string PROJECT_NO)
        {
            string sql = @"select CP_KIND 
                           from PROJECT_BASIC (nolock)
                           where PROJECT_NO = @PROJECT_NO";
            string cpKind = await ExecuteQueryFirstOrDefaultAsync<string>(sql, new { PROJECT_NO });

            return !string.IsNullOrEmpty(cpKind) && (cpKind == "0");
        }

        /// <summary>
        /// 檢查是否辦理開工
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<bool> CheckIsStartWork(string PROJECT_NO)
        {
            string sql = @"select chkItem.ACTUAL_ENDDATE
                           from PROJECT_CHECKITEM chkItem (nolock)
                           left join CODE_CHECKPOINT_ITEM code (nolock)
                              on chkItem.CHECKITEM_SEQ = code.SEQ
                           where PROJECT_NO = @PROJECT_NO
                              and code.CTRL_POINT = 'A' ";
            string actualEndDate = await ExecuteQueryFirstOrDefaultAsync<string>(sql, new { PROJECT_NO });

            return !string.IsNullOrEmpty(actualEndDate);
        }

        /// <summary>
        /// 檢查是否本府執行案件
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<bool> CheckIsTycgProject(string PROJECT_NO)
        {
            string sql = @"select IS_TYCG_PROJECT
                           from PROJECT_BASIC(nolock)
                           where PROJECT_NO = @PROJECT_NO";
            bool tycgPROJECT = await ExecuteQueryFirstOrDefaultAsync<bool>(sql, new { PROJECT_NO });

            return tycgPROJECT;
        }

        /// <summary>
        /// 檢查是否辦理竣工
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<bool> CheckIsCompletedWork(string PROJECT_NO)
        {
            string sql = @"select chkItem.ACTUAL_ENDDATE
                           from PROJECT_CHECKITEM chkItem (nolock)
                           left join CODE_CHECKPOINT_ITEM code (nolock)
                              on chkItem.CHECKITEM_SEQ = code.SEQ
                           where PROJECT_NO = @PROJECT_NO
                              and code.CTRL_POINT = 'B' ";
            return (await ExecuteQueryFirstOrDefaultAsync<DateTime?>(sql, new { PROJECT_NO })).HasValue;
        }

        /// <summary>
        /// 更新每月辦理情形-累計預定施工進度% & 累計實際施工進度%
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="prg"></param>
        public void UpdateFillCycleIpcPrg(string PROJECT_NO, int? prg)
        {
            string sql = @"
                update PROJECT_ENGINEERING_PROGRESS
                set 
	                IPC_RES_PRG = @prg,
	                IPC_ACT_PRG = @prg
                where PROJECT_NO = @PROJECT_NO
	                and SEQ = (
		                select Max(SEQ)
		                from PROJECT_ENGINEERING_PROGRESS
		                where PROJECT_NO = @PROJECT_NO
	                )";
            ExecuteCommand(sql, new { PROJECT_NO, prg });
        }
        #endregion

        #region 管考備註
        /// <summary>
        /// 取得計畫基本資料(管考備註需要的欄位)
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<ProjectBasicForProjectFillAuditModel> GetProjectBasic(string PROJECT_NO)
        {
            string sql = $@"SELECT PROJECT_NO,
                                dbo.FN_GetSetParam('PROJECT_STATUS', PROJECT_STATUS) AS PROJECT_STATUS,
                                FINISH_DATE,
                                SCORE_A,
                                NOTES_FOR_BUDGET,
                                NOTES_FOR_SCHEDULE
                            FROM PROJECT_BASIC (NOLOCK)
                            WHERE PROJECT_NO = @PROJECT_NO";
            return await ExecuteQueryFirstOrDefaultAsync<ProjectBasicForProjectFillAuditModel>(sql, new { PROJECT_NO });
        }

        /// <summary>
        /// 更新計畫基本資料
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectBasic(ProjectBasicForProjectFillAuditModel model)
        {
            string sql = $@"UPDATE PROJECT_BASIC
                                SET NOTES_FOR_BUDGET = @NOTES_FOR_BUDGET,
                                NOTES_FOR_SCHEDULE = @NOTES_FOR_SCHEDULE,
                                SCORE_A = @SCORE_A,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE PROJECT_NO = @PROJECT_NO";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 取得管考審核意見
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectEngineeringAuditOpinionModel>> GetProjectEngineeringAuditOpinion(string PROJECT_NO)
        {
            string sql = $@"SELECT SEQ,
                                PROJECT_NO,
                                YEAR,
                                MONTH,
                                AUDIT_OPINION,
                                CHECK_RESULT,
                                IMPROVEMENT_OPINION,
                                COM_IPCMEMO
                            FROM PROJECT_ENGINEERING_AUDIT_OPINION (NOLOCK)
                            WHERE PROJECT_NO = @PROJECT_NO";
            return (await ExecuteQueryAsync<ProjectEngineeringAuditOpinionModel>(sql, new { PROJECT_NO })).ToList();
        }

        /// <summary>
        /// 新增管考審核意見
        /// </summary>
        /// <param name="model"></param>
        public int InsertProjectEngineeringAuditOpinion(ProjectEngineeringAuditOpinionModel model)
        {
            string sql = $@"INSERT INTO PROJECT_ENGINEERING_AUDIT_OPINION 
                                (PROJECT_NO,
                                YEAR,
                                MONTH,
                                AUDIT_OPINION,
                                CHECK_RESULT,
                                IMPROVEMENT_OPINION,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            OUTPUT INSERTED.SEQ
                            VALUES
                                (@PROJECT_NO,
                                @YEAR,
                                @MONTH,
                                @AUDIT_OPINION,
                                @CHECK_RESULT,
                                @IMPROVEMENT_OPINION,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            return ExecuteQuery<int>(sql, model, isSetUSER: true).FirstOrDefault();
        }

        /// <summary>
        /// 更新管考審核意見
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectEngineeringAuditOpinion(ProjectEngineeringAuditOpinionModel model)
        {
            string sql = $@"UPDATE PROJECT_ENGINEERING_AUDIT_OPINION
                                SET YEAR = @YEAR,
                                MONTH = @MONTH,
                                AUDIT_OPINION = @AUDIT_OPINION,
                                CHECK_RESULT = @CHECK_RESULT,
                                IMPROVEMENT_OPINION = @IMPROVEMENT_OPINION,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE SEQ = @SEQ";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 刪除管考審核意見
        /// </summary>
        /// <param name="SEQ"></param>
        public void DeleteProjectEngineeringAuditOpinion(int SEQ)
        {
            string sql = $@"DELETE PROJECT_ENGINEERING_AUDIT_OPINION WHERE SEQ = @SEQ";
            ExecuteCommand(sql, new { SEQ });
        }

        /// <summary>
        /// 取得會議列管
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectConferenceModel>> GetProjectConference(string PROJECT_NO)
        {
            string sql = $@"SELECT IDENTITY_FIELD,
                                PROJECT_NO,
                                CONFERENCE_GENRE,
                                CONFERENCE_NUM,
                                CONFERENCE_TIME
                            FROM PROJECT_CONFERENCE (NOLOCK)
                            WHERE PROJECT_NO = @PROJECT_NO
                            ORDER BY CONFERENCE_TIME, CONFERENCE_NUM";
            return (await ExecuteQueryAsync<ProjectConferenceModel>(sql, new { PROJECT_NO })).ToList();
        }

        /// <summary>
        /// 新增會議列管
        /// </summary>
        /// <param name="model"></param>
        public void InsertProjectConference(List<ProjectConferenceModel> model)
        {
            string sql = $@"INSERT INTO PROJECT_CONFERENCE 
                                (PROJECT_NO,
                                CONFERENCE_GENRE,
                                CONFERENCE_NUM,
                                CONFERENCE_TIME,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            VALUES
                                (@PROJECT_NO,
                                @CONFERENCE_GENRE,
                                @CONFERENCE_NUM,
                                @CONFERENCE_TIME,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 更新會議列管
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectConference(List<ProjectConferenceModel> model)
        {
            string sql = $@"UPDATE PROJECT_CONFERENCE
                                SET CONFERENCE_GENRE = @CONFERENCE_GENRE,
                                CONFERENCE_NUM = @CONFERENCE_NUM,
                                CONFERENCE_TIME = @CONFERENCE_TIME,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE IDENTITY_FIELD = @IDENTITY_FIELD";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 刪除會議列管
        /// </summary>
        /// <param name="model"></param>
        public void DeleteProjectConference(List<ProjectConferenceModel> model)
        {
            string sql = $@"DELETE PROJECT_CONFERENCE WHERE IDENTITY_FIELD = @IDENTITY_FIELD";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 取得逾期繳交填報紀錄
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectDelayfillModel>> GetProjectDelayfill(string PROJECT_NO)
        {
            string sql = $@"SELECT SEQ,
                                PROJECT_NO,
                                FILL_TIME,
                                FILL_REASON
                            FROM PROJECT_DELAYFILL (NOLOCK)
                            WHERE PROJECT_NO = @PROJECT_NO
                            ORDER BY FILL_TIME";
            return (await ExecuteQueryAsync<ProjectDelayfillModel>(sql, new { PROJECT_NO })).ToList();
        }

        /// <summary>
        /// 新增逾期繳交填報紀錄
        /// </summary>
        /// <param name="model"></param>
        public void InsertProjectDelayfill(List<ProjectDelayfillModel> model)
        {
            string sql = $@"INSERT INTO PROJECT_DELAYFILL 
                                (PROJECT_NO,
                                FILL_TIME,
                                FILL_REASON,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            VALUES
                                (@PROJECT_NO,
                                @FILL_TIME,
                                @FILL_REASON,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 更新逾期繳交填報紀錄
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectDelayfill(List<ProjectDelayfillModel> model)
        {
            string sql = $@"UPDATE PROJECT_DELAYFILL
                                SET FILL_TIME = @FILL_TIME,
                                FILL_REASON = @FILL_REASON,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE SEQ = @SEQ";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 刪除逾期繳交填報紀錄
        /// </summary>
        /// <param name="model"></param>
        public void DeleteProjectDelayfill(List<ProjectDelayfillModel> model)
        {
            string sql = $@"DELETE PROJECT_DELAYFILL WHERE SEQ = @SEQ";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 取得計畫分併案記錄檔
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectMergeLogModel>> GetProjectMergeLog(string PROJECT_NO)
        {
            string sql = $@"SELECT SEQ,
                                PROJECT_NO,
                                MERGE_STATUS,
                                PROMERGE_DATE
                            FROM PROJECT_MERGE_LOG (NOLOCK)
                            WHERE PROJECT_NO = @PROJECT_NO";
            return (await ExecuteQueryAsync<ProjectMergeLogModel>(sql, new { PROJECT_NO })).ToList();
        }

        /// <summary>
        /// 取得計畫分併案記錄檔 分案次數
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<int> GetProjectMergeLogCount(string PROJECT_NO)
        {
            string sql = $@"SELECT COUNT(PROJECT_NO)
                            FROM PROJECT_MERGE_LOG (NOLOCK)
                            WHERE PROJECT_NO = @PROJECT_NO AND MERGE_STATUS = '01'";
            return (await ExecuteQueryFirstOrDefaultAsync<int>(sql, new { PROJECT_NO }));
        }

        /// <summary>
        /// 新增計畫分併案記錄檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int InsertProjectMergeLog(ProjectMergeLogModel model)
        {
            string sql = $@"INSERT INTO PROJECT_MERGE_LOG 
                                (PROJECT_NO,
                                MERGE_STATUS,
                                PROMERGE_DATE,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            OUTPUT INSERTED.SEQ
                            VALUES
                                (@PROJECT_NO,
                                @MERGE_STATUS,
                                @PROMERGE_DATE,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            return ExecuteQuery<int>(sql, model, isSetUSER: true).FirstOrDefault();
        }

        /// <summary>
        /// 更新計畫分併案記錄檔
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectMergeLog(ProjectMergeLogModel model)
        {
            string sql = $@"UPDATE PROJECT_MERGE_LOG
                                SET MERGE_STATUS = @MERGE_STATUS,
                                PROMERGE_DATE = @PROMERGE_DATE,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE SEQ = @SEQ";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 刪除計畫分併案記錄檔
        /// </summary>
        /// <param name="SEQ"></param>
        public void DeleteProjectMergeLog(int SEQ)
        {
            string sql = $@"DELETE PROJECT_MERGE_LOG WHERE SEQ = @SEQ";
            ExecuteCommand(sql, new { SEQ });
        }

        /// <summary>
        /// 取得計畫撤銷資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<AdjustAuditModel> GetRevokeData(string PROJECT_NO)
        {
            string sql = @"SELECT a.PROJ_ADJ_ID
                                ,a.PROJECT_NO
								,a.PROJECT_NAME
								,a.AW_KIND
								,a.APPRV_DATE
								,a.ADJUST_REASON
								,a.PROJECT_AW_STATUS
								,STUFF((
									SELECT CASE WHEN s.SET_TYPE = '99' THEN s.SET_VALUE + '：' + a.OTHER_REASON ELSE s.SET_VALUE END + '。'
									FROM PROJECT_MAPPING_DATA (NOLOCK) m
									INNER JOIN SET_PARAM (NOLOCK) s ON s.SET_ITEM = 'REVOKE_REASON' AND s.SET_TYPE = m.SET_TYPE
									WHERE PROJECT_NO = a.PROJECT_NO AND SOURCE_ID = CAST(a.PROJ_ADJ_ID AS VARCHAR)
									FOR XML PATH('')
								), 1, 0, '') AS OTHER_REASON --已經組好的撤銷原因
                            FROM PROJECT_BASIC_ADJ (NOLOCK) a
                            WHERE a.DEL_FLG != 1
                                AND a.PROJECT_NO = @PROJECT_NO
                                AND a.PROJECT_AW_STATUS = 'W05'";
            return await ExecuteQueryFirstOrDefaultAsync<AdjustAuditModel>(sql, new { PROJECT_NO });
        }

        /// <summary>
        /// 取得實地查證情形
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectFactFindingModel>> GetProjectFactFinding(string PROJECT_NO)
        {
            string sql = $@"SELECT SEQ,
                                PROJECT_NO,
                                FFDATE,
                                COMPLETEREPLYDATE,
                                FFSCORE,
                                FFCOMMENT,
                                FFREPORT,
                                FFREPORT_DATE,
                                REPLYYN
                            FROM PROJECT_FACT_FINDING (NOLOCK)
                            WHERE PROJECT_NO = @PROJECT_NO";
            return (await ExecuteQueryAsync<ProjectFactFindingModel>(sql, new { PROJECT_NO })).ToList();
        }

        /// <summary>
        /// 新增實地查證情形
        /// </summary>
        /// <param name="model"></param>
        public int InsertProjectFactFinding(ProjectFactFindingModel model)
        {
            string sql = $@"INSERT INTO PROJECT_FACT_FINDING 
                                (PROJECT_NO,
                                FFDATE,
                                COMPLETEREPLYDATE,
                                FFSCORE,
                                FFCOMMENT,
                                FFREPORT,
                                FFREPORT_DATE,
                                REPLYYN,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            OUTPUT INSERTED.SEQ
                            VALUES
                                (@PROJECT_NO,
                                @FFDATE,
                                @COMPLETEREPLYDATE,
                                @FFSCORE,
                                @FFCOMMENT,
                                @FFREPORT,
                                @FFREPORT_DATE,
                                @REPLYYN,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            return ExecuteQuery<int>(sql, model, isSetUSER: true).FirstOrDefault();
        }

        /// <summary>
        /// 更新實地查證情形
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectFactFinding(ProjectFactFindingModel model)
        {
            string sql = $@"UPDATE PROJECT_FACT_FINDING
                                SET FFDATE = @FFDATE,
                                COMPLETEREPLYDATE = @COMPLETEREPLYDATE,
                                FFSCORE = @FFSCORE,
                                FFCOMMENT = @FFCOMMENT,
                                FFREPORT = @FFREPORT,
                                FFREPORT_DATE = @FFREPORT_DATE,
                                REPLYYN = @REPLYYN,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE SEQ = @SEQ";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 刪除實地查證情形
        /// </summary>
        /// <param name="SEQ"></param>
        public void DeleteProjectFactFinding(int SEQ)
        {
            string sql = $@"DELETE PROJECT_FACT_FINDING WHERE SEQ = @SEQ";
            ExecuteCommand(sql, new { SEQ });
        }

        /// <summary>
        /// 取得結案明細資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectCloseDetailsModel>> GetProjectCloseDetails(string PROJECT_NO)
        {
            string sql = $@"SELECT SEQ,
                                PROJECT_NO,
                                CLOSE_DETAILS_TYPE,
                                REF_DATE,
                                REF_MEMO
                            FROM PROJECT_CLOSE_DETAILS (NOLOCK)
                            WHERE PROJECT_NO = @PROJECT_NO";
            return (await ExecuteQueryAsync<ProjectCloseDetailsModel>(sql, new { PROJECT_NO })).ToList();
        }

        /// <summary>
        /// 新增結案明細資料
        /// </summary>
        /// <param name="model"></param>
        public void InsertProjectCloseDetails(List<ProjectCloseDetailsModel> model)
        {
            string sql = $@"INSERT INTO PROJECT_CLOSE_DETAILS 
                                (PROJECT_NO,
                                CLOSE_DETAILS_TYPE,
                                REF_DATE,
                                REF_MEMO,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            VALUES
                                (@PROJECT_NO,
                                @CLOSE_DETAILS_TYPE,
                                @REF_DATE,
                                @REF_MEMO,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 更新結案明細資料
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectCloseDetails(List<ProjectCloseDetailsModel> model)
        {
            string sql = $@"UPDATE PROJECT_CLOSE_DETAILS
                                SET CLOSE_DETAILS_TYPE = @CLOSE_DETAILS_TYPE,
                                REF_DATE = @REF_DATE,
                                REF_MEMO = @REF_MEMO,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE SEQ = @SEQ";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 刪除結案明細資料
        /// </summary>
        /// <param name="model"></param>
        public void DeleteProjectCloseDetails(List<ProjectCloseDetailsModel> model)
        {
            string sql = $@"DELETE PROJECT_CLOSE_DETAILS WHERE SEQ = @SEQ";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 取得結案意見
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<ProjectCloseMemoModel> GetProjectCloseMemo(string PROJECT_NO)
        {
            string sql = $@"SELECT PROJECT_NO, TOTAL_SCORE 
                            FROM PROJECT_CLOSE_MEMO (NOLOCK) 
                            WHERE PROJECT_NO = @PROJECT_NO";
            return (await ExecuteQueryFirstOrDefaultAsync<ProjectCloseMemoModel>(sql, new { PROJECT_NO }));
        }

        /// <summary>
        /// 新增結案意見
        /// </summary>
        /// <param name="model"></param>
        public void InsertProjectCloseMemo(ProjectCloseMemoModel model)
        {
            string sql = $@"INSERT INTO PROJECT_CLOSE_MEMO 
                                (PROJECT_NO,                               
                                TOTAL_SCORE,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            VALUES
                                (@PROJECT_NO,                                
                                @TOTAL_SCORE,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 更新結案意見
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectCloseMemo(ProjectCloseMemoModel model)
        {
            string sql = $@"UPDATE PROJECT_CLOSE_MEMO
                                SET TOTAL_SCORE = @TOTAL_SCORE,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE PROJECT_NO = @PROJECT_NO";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 取得未於期限內提出計畫調整資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectBasicAdjForDelayApply>> GetDelayApply(string PROJECT_NO)
        {
            string sql = @"SELECT d.SET_VALUE AS SCHE_TYPE, m.APPRV_DATE
                           FROM PROJECT_BASIC_ADJ (NOLOCK) m
                           INNER JOIN SET_PARAM (NOLOCK) d ON d.SET_ITEM = 'SCHE_TYPE' AND d.SET_TYPE = m.SCHE_TYPE
                           WHERE m.PROJECT_NO = @PROJECT_NO AND m.IS_DELAY_APPLY = 1 AND m.DEL_FLG = 0 AND PROJECT_AW_STATUS = 'B05'";
            return (await ExecuteQueryAsync<ProjectBasicAdjForDelayApply>(sql, new { PROJECT_NO })).ToList();
        }
        #endregion

        #region 預算執行情形
        /// <summary>
        /// 取得計畫預算執行情形
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<List<ProjectBudgetExecuteModel>> GetProjectFillBudgetExec(string PROJECT_NO)
        {
            string sql = $@"SELECT SEQ,
	                            PROJECT_NO,
	                            EXEC_YEAR,
                                EXEC_MONTH,
	                            GT_EXPANDED_BUDGET,
	                            GT_ACT_BUDGET,
	                            GT_AP,
                                GT_BALANCE,
                                GT_TOTAL,
                                GT_EXEC_RATE,
                                YEAR_BUDGET_EXPANDED,
                                YEAR_BUDGET_ALLOCATED,
                                YEAR_EXEC_BUDGET,
                                YEAR_EXEC_RATE,
                                EXEC_RATE_FAILED_NOTE,
                                EXEC_NOTE,
                                CRT_DATE
                            FROM PROJECT_BUDGET_EXECUTE (NOLOCK)
                            WHERE PROJECT_NO = @PROJECT_NO
                            ORDER BY EXEC_YEAR DESC, EXEC_MONTH DESC";
            return (await ExecuteQueryAsync<ProjectBudgetExecuteModel>(sql, new { PROJECT_NO })).ToList();
        }

        /// <summary>
        /// 新增計畫預算執行情形
        /// </summary>
        /// <param name="model"></param>
        public int InsertProjectFillBudgetExec(ProjectBudgetExecuteModel model)
        {
            string sql = $@"INSERT INTO PROJECT_BUDGET_EXECUTE 
                                (PROJECT_NO,
                                EXEC_YEAR,
                                EXEC_MONTH,
                                GT_EXPANDED_BUDGET,
                                GT_ACT_BUDGET,
                                GT_AP,
                                GT_BALANCE,
                                GT_TOTAL,
                                GT_EXEC_RATE,
                                YEAR_BUDGET_EXPANDED,
                                YEAR_BUDGET_ALLOCATED,
                                YEAR_EXEC_BUDGET,
                                YEAR_EXEC_RATE,
                                EXEC_RATE_FAILED_NOTE,
                                EXEC_NOTE,
                                CRT_USER,
                                CRT_DATE,
                                MDF_USER,
                                MDF_DATE)
                            OUTPUT INSERTED.SEQ
                            VALUES
                                (@PROJECT_NO,
                                @EXEC_YEAR,
                                @EXEC_MONTH,
                                @GT_EXPANDED_BUDGET,
                                @GT_ACT_BUDGET,
                                @GT_AP,
                                @GT_BALANCE,
                                @GT_TOTAL,
                                @GT_EXEC_RATE,
                                @YEAR_BUDGET_EXPANDED,
                                @YEAR_BUDGET_ALLOCATED,
                                @YEAR_EXEC_BUDGET,
                                @YEAR_EXEC_RATE,
                                @EXEC_RATE_FAILED_NOTE,
                                @EXEC_NOTE,
                                @CRT_USER,
                                {DTNow},
                                @MDF_USER,
                                {DTNow})";
            return ExecuteQuery<int>(sql, model, isSetUSER: true).FirstOrDefault();
        }

        /// <summary>
        /// 更新計畫預算執行情形
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectFillBudgetExec(ProjectBudgetExecuteModel model)
        {
            string sql = $@"UPDATE PROJECT_BUDGET_EXECUTE
                                SET GT_EXPANDED_BUDGET = @GT_EXPANDED_BUDGET,
                                GT_ACT_BUDGET = @GT_ACT_BUDGET,
                                GT_AP = @GT_AP,
                                GT_BALANCE = @GT_BALANCE,
                                GT_TOTAL = @GT_TOTAL,
                                GT_EXEC_RATE = @GT_EXEC_RATE,
                                YEAR_BUDGET_EXPANDED = @YEAR_BUDGET_EXPANDED,
                                YEAR_BUDGET_ALLOCATED = @YEAR_BUDGET_ALLOCATED,
                                YEAR_EXEC_BUDGET = @YEAR_EXEC_BUDGET,
                                YEAR_EXEC_RATE = @YEAR_EXEC_RATE,
                                EXEC_RATE_FAILED_NOTE = @EXEC_RATE_FAILED_NOTE,
                                EXEC_NOTE = @EXEC_NOTE,
                                MDF_USER = @MDF_USER,
                                MDF_DATE = {DTNow}
                            WHERE SEQ = @SEQ";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 刪除計畫預算執行情形
        /// </summary>
        /// <param name="model"></param>
        public void DeleteProjectFillBudgetExec(ProjectBudgetExecuteModel model)
        {
            string sql = $"DELETE PROJECT_BUDGET_EXECUTE WHERE SEQ = @SEQ";
            ExecuteCommand(sql, model);
        }
        #endregion

        #region 共用

        /// <summary>
        /// 檢查當期執行情形是否未送出或未超過填報週期
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<bool> CheckProjFillExeDataCanSave(string PROJECT_NO)
        {
            // 查詢當期工程進度資料筆數SQL
            string currentProgressDataCntSQL = @"select count(*) from PROJECT_ENGINEERING_PROGRESS progress (nolock)
                                                            inner join (select top 1 PROJECT_YEAR, 
                                                                                        PROJECT_MONTH, 
                                                                                        FILL_END_DATE 
			                                                            from PROJECT_FILL_CYCLE (nolock)
			                                                            order by SEQ desc) as fillCycle
	                                                        on progress.YEAR = fillCycle.PROJECT_YEAR 
                                                                and progress.MONTH = fillCycle.PROJECT_MONTH	
                                                            inner join PROJECT_BASIC basic
                                                                on basic.PROJECT_NO = progress.PROJECT_NO
                                                            where progress.PROJECT_NO = @PROJECT_NO";

            string sql = $@"--排除無當期工程進度資料
                            if(({currentProgressDataCntSQL}) = 0 )
                                begin 
                                    select 1
                                end
                            else
                                begin
                                    {currentProgressDataCntSQL}
                                    and CASE 
		                                    -- 若使用界接資料，僅考慮是否在填報週期內
		                                    WHEN basic.IS_USER_FTY_DATA = 1 and {DTNow} < fillCycle.FILL_END_DATE then 1
		                                    -- 否則判斷當期執行情形是否未送出或未超過填報週期
		                                    WHEN basic.IS_USER_FTY_DATA = 0 and
			                                    (progress.IS_SEND = 0 or {DTNow} < fillCycle.FILL_END_DATE) then 1
		                                    ELSE 0 
	                                    END = 1;
                                end";

            return await ExecuteQueryFirstOrDefaultAsync<int>(sql, new {PROJECT_NO}) > 0;
        }

        /// <summary>
        /// 取消當期執行情形送出
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        public void CancelSend(string PROJECT_NO)
        {
            string sql = $@"update PROJECT_ENGINEERING_PROGRESS 
                            set IS_SEND = 0 ,
                                SEND_DATE = null,
                                MDF_USER = @UserId,
                                MDF_DATE = {DTNow}
                            where  PROJECT_NO = @PROJECT_NO
	                            and YEAR = (select top 1 PROJECT_YEAR from PROJECT_FILL_CYCLE order by SEQ desc)
	                            and MONTH = (select top 1 PROJECT_MONTH from PROJECT_FILL_CYCLE order by SEQ desc)";

            ExecuteCommand(sql, new { PROJECT_NO, UserId });
        }

        /// <summary>
        /// 取得計畫是否送出
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<bool> GetIsSend(string PROJECT_NO)
        {
            string sql = @"select IS_SEND from PROJECT_ENGINEERING_PROGRESS 
                            where PROJECT_NO = @PROJECT_NO
	                            and YEAR = (select top 1 PROJECT_YEAR from PROJECT_FILL_CYCLE order by SEQ desc)
	                            and MONTH = (select top 1 PROJECT_MONTH from PROJECT_FILL_CYCLE order by SEQ desc)";
            return (await ExecuteQueryAsync<bool>(sql, new { PROJECT_NO })).FirstOrDefault();
        }

        public async Task<ProjectFillCycleModel> GetProjFillCycle()
        {
            string sql = @"select top 1 SEQ,
                                PROJECT_YEAR,
                                PROJECT_MONTH,
                                FILL_START_DATE,
                                FILL_END_DATE 
                            from PROJECT_FILL_CYCLE (nolock)
                            order by FILL_END_DATE desc";
            return await ExecuteQueryFirstOrDefaultAsync<ProjectFillCycleModel>(sql);
        }
        #endregion
    }
}
