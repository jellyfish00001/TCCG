using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SDO.Services
{
    public class ListExecService : Service, IListExecService
    {
        private readonly IListExecDac dac;

        public ListExecService(IListExecDac dac)
        {
            this.dac = dac;
        }

        /// <summary>
        /// 查詢先期計畫資料
        /// </summary>
        /// <returns></returns>
        public async Task<List<ListExecModel>> GetPWSProjectList(ListExecQueryModel model)
        {
            var result = await dac.GetPWSProjectList(model);
            return result;
        }

        /// <summary>
        /// 查詢機關是否截止
        /// </summary>
        /// <returns></returns>
        public async Task<bool> GetOrgDeadline(string OU_ID)
        {
            var result = await dac.GetOrgDeadline(OU_ID);
            return result;
        }

        /// <summary>
        /// 刪除計畫
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DeleteProjectList(List<string> PLANNO)
        {
            using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var planNo in PLANNO)
                {
                    // 對列表中的每一個PLANNO進行刪除操作
                    // 刪除計畫主檔
                    await dac.DeletePWSSDPLANMAIN(new List<string> { planNo });
                    // 刪除跨年度預算
                    await dac.DeletePROJECT_BUDGET_SOURCE_G(new List<string> { planNo });
                    // 刪除歷年執行情形
                    await dac.DeletePWSSDHISTORYEXE(new List<string> { planNo });
                    // 刪除近三年相關研究
                    await dac.DeletePWSSDTREEYEARPLAN(new List<string> { planNo });
                    // 刪除經費需求細項
                    await dac.DeletePWSSDPLANFUND(new List<string> { planNo });
                    // 刪除小組審核紀錄
                    await dac.DeletePWSSDVIEW(new List<string> { planNo });
                    // 刪除附件檔案
                    await dac.DeletePROJECT_ATTACHMENT(new List<string> { planNo });
                }

                scope.Complete();
            }
            return true;
        }


    }
}
