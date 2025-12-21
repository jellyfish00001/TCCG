using Microsoft.AspNetCore.Http;
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
    public class ProjectClosedDac : Dac, IProjectClosedDac
    {
        public ProjectClosedDac(IConnectionControlCenter connectionControlCenter,
           IHttpContextAccessor httpContextAccessor,
           ISqlTrace trace,
           IUserProfile profile,
           IParameterAdaptor ParameterAdaptor,
           IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {

        }
        
        /// <summary>
        /// 取得計畫結案資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        public async Task<ProjectFillCloseModel> GetProjectFillClose(string PROJECT_NO)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@"
                select 
                    main.PROJECT_NO,
                    audit.REVIEW_RESULT,
                    audit.REVIEW_COMMENTS,
                    main.MEMO_CLOSE,
                    budget.TOTAL_BUDGET,
                    payment.TOTAL_ACTUAL_COMP,
                    payment.IDENTITY_FIELD as PAYMENT_ID,
                    payment.ACTUAL_PAY,
                    payment.UNPAY,
                    payment.BALANCE,
                    payment.MDF_DATE,
                    (case when payment.DATA_DATE is null 
						then null
						else Concat(YEAR(payment.DATA_DATE)-1911, '_', Right('00' + Cast(Month(payment.DATA_DATE) as varchar), 2))
					end) as DATA_DATE_SHOW,
                    (case when main.PROJECT_STATUS = '5' then 0 else 1 end) as CanSave
                from PROJECT_BASIC main (nolock)
                left join 
                (
					select PROJECT_NO, TOTAL_ACTUAL_COMP, IDENTITY_FIELD, ACTUAL_PAY, UNPAY, BALANCE, MDF_DATE, DATA_DATE
					from PROJECT_PAYMENT
					where IDENTITY_FIELD in (
						select Max(IDENTITY_FIELD) from PROJECT_PAYMENT
						group by PROJECT_NO
					)
				) as payment
	                on main.PROJECT_NO = payment.PROJECT_NO
                left join PROJECT_AUDIT audit(nolock)
	                    on main.PROJECT_NO = audit.PROJECT_NO 
                        and audit.PLAN_REVIEW_TYPE = 'P2' -- 結案審核
                        and audit.IS_SEND = 0
                left join (select PROJECT_NO,
				                    SUM(BUDGET_CENTRAL+BUDGET_LOCAL) as TOTAL_BUDGET
		                    from PROJECT_BUDGET_SOURCE_G 
		                    GROUP BY PROJECT_NO) budget
                    on main.PROJECT_NO = budget.PROJECT_NO
                where main.PROJECT_NO = @PROJECT_NO");
            sql.AppendLine(";");
            sql.AppendLine(@"
                select
	                IDENTITY_FIELD,
                    FILE_NAME
                from PROJECT_ATTACHMENT (nolock)
                where FILE_KIND = '16' 
	                and FILE_UP_SOURCE = '02'
	                and PROJECT_NO = @PROJECT_NO");

            var dacResult = await ExecuteQueryMultipleAsync<ProjectFillCloseModel, ProjectAttachmentModel>(sql.ToString(), new { PROJECT_NO });

            ProjectFillCloseModel result = dacResult.result1.FirstOrDefault() ?? new();
            result.ProjAttachments = dacResult.result2.ToList();
            return result;
        }

        /// <summary>
        /// 新增計畫實際經費支用
        /// </summary>
        /// <param name="model"></param>
        public void AddMdfProjectPayment(ProjectFillCloseModel model)
        {
            string sql = $@"update PROJECT_PAYMENT 
                            set
                               DATA_DATE = @DATA_DATE,
                               TOTAL_ACTUAL_COMP = @TOTAL_ACTUAL_COMP,
                               ACTUAL_PAY = @ACTUAL_PAY,
                               UNPAY = @UNPAY,
                               BALANCE = @BALANCE,
                               MDF_USER = @MDF_USER,
                               MDF_DATE = {DTNow}
                            where PROJECT_NO = @PROJECT_NO
                            -- 若沒資料則新增
                            IF @@ROWCOUNT = 0
                            BEGIN
                                INSERT INTO PROJECT_PAYMENT (
                                    PROJECT_NO,
                                    DATA_DATE,
                                    TOTAL_ACTUAL_COMP,
                                    ACTUAL_PAY,
                                    UNPAY,
                                    BALANCE,
                                    CRT_USER,
                                    CRT_DATE,
                                    MDF_USER,
                                    MDF_DATE)
                                VALUES(
                                    @PROJECT_NO,
                                    @DATA_DATE,
                                    @TOTAL_ACTUAL_COMP,
                                    @ACTUAL_PAY,
                                    @UNPAY,
                                    @BALANCE,
                                    @CRT_USER,
                                    {DTNow},
                                    @MDF_USER,
                                    {DTNow})
                            END";
            ExecuteCommand(sql, model);
        }

        /// <summary>
        /// 更新計畫基本資料結案審核結果
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProjectCloseRvwResult(ProjectFillCloseModel model)
        {
            string sql = $@"update PROJECT_BASIC 
                            set PROJECT_STATUS = @PROJECT_STATUS,
                                MEMO_CLOSE = @MEMO_CLOSE,
                                FINISH_DATE = @FINISH_DATE,
                                MDF_USER = @MDF_DATE,
                                MDF_DATE = {DTNow}
                            where PROJECT_NO = @PROJECT_NO";
            ExecuteCommand(sql, model);
        }

        public int GetWorkEnd(string PROJECT_NO)
        {
            string sql = @"
                if (
	                select count(*) 
                    from PROJECT_CHECKITEM M1
                    inner join CODE_CHECKPOINT_ITEM M2 on M2.SEQ = M1.CHECKITEM_SEQ
                    where M1.PROJECT_NO = @PROJECT_NO and M2.CTRL_POINT = 'C'
                ) > 0
                begin
	                select M1.SEQ 
                    from PROJECT_CHECKITEM M1
                    inner join CODE_CHECKPOINT_ITEM M2 on M2.SEQ = M1.CHECKITEM_SEQ
                    where M1.PROJECT_NO = @PROJECT_NO and M2.CTRL_POINT = 'C'
                end
                else 
                begin
	                select top 1 SEQ 
                    from PROJECT_CHECKITEM
                    where PROJECT_NO = @PROJECT_NO
	                order by SEQ desc
                end";
            return ExecuteQuery<int>(sql, new { PROJECT_NO }).FirstOrDefault();
        }

        /// <summary>
        /// 清除驗收檢核點
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="SEQ"></param>
        public void ClearWorkEnd(string PROJECT_NO, int SEQ)
        {
            string sql = @"update PROJECT_CHECKITEM
                            set
	                            ACTUAL_ENDDATE = null
                            where PROJECT_NO = @PROJECT_NO
	                            and SEQ >= @SEQ";
            ExecuteCommand(sql, new { PROJECT_NO, SEQ });
        }
    }
}
