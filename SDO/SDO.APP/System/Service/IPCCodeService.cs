using SDO.Base.Utils;
using SDO.Base.Utils.Models;
using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class IPCCodeService : Service, IIPCCodeService
    {
        private readonly IIPCCodeDac dac;
        private readonly IDropDownDac dropdownDac;
        private readonly IProjectCommonDac projectCommonDac;
        public IPCCodeService(IIPCCodeDac dac, IDropDownDac dropdownDac, IProjectCommonDac projectCommonDac)
        {
            this.dac = dac;
            this.dropdownDac = dropdownDac;
            this.projectCommonDac = projectCommonDac;
        }

        /// <summary>
        /// 取得執行方式
        /// </summary>
        /// <param name="CP_KIND"></param>
        /// <param name="isShowDel"></param>
        /// <returns></returns>
        public async Task<List<IPCCodeCheckpointModel>> GetCodeCheckpoint(string CP_KIND, bool isShowDel)
        {
            return await dac.GetCodeCheckpoint(CP_KIND, isShowDel);
        }

        /// <summary>
        /// 儲存執行方式
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public RtnResultModel SaveCodeCheckpoint(List<IPCCodeCheckpointModel> models)
        {
            List<IPCCodeCheckpointModel> insertData = models.Where(x => (editTypeEnum)x.editType == editTypeEnum.Add).ToList();
            List<IPCCodeCheckpointModel> updateData = models.Where(x => (editTypeEnum)x.editType == editTypeEnum.Modify).ToList();
            dac.BeginTransaction();
            dac.InsertCodeCheckpoint(insertData);
            dac.UpdateCodeCheckpoint(updateData);
            dac.Commit();
            return ChangeResult(true, "存檔成功");
        }

        /// <summary>
        /// 取得自訂檢核點
        /// </summary>
        /// <param name="CHK_POINT_CLASS_ID"></param>
        /// <param name="forSettings"></param>
        /// <returns></returns>
        public async Task<List<IPCCusChkItemModel>> GetCusChkItem(int CHK_POINT_CLASS_ID,bool forSettings)
        {
            return await dac.GetCusChkItem(CHK_POINT_CLASS_ID, forSettings);
        }

        /// <summary>
        /// 儲存自訂檢核點
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public RtnResultModel SaveCusChkItem(List<IPCCusChkItemModel> models)
        {
            List<IPCCusChkItemModel> insertData = models.Where(x => (editTypeEnum)x.editType == editTypeEnum.Add).ToList();
            List<IPCCusChkItemModel> updateData = models.Where(x => (editTypeEnum)x.editType == editTypeEnum.Modify).ToList();
            dac.BeginTransaction();
            dac.InsertCusChkItem(insertData);
            dac.UpdateCusChkItem(updateData);
            dac.Commit();
            return ChangeResult(true, "存檔成功");
        }

        /// <summary>
        /// 取得落後原因類別
        /// </summary>
        /// <param name="DELAY_CLASS_ID"></param>
        /// <param name="DEL_FLG"></param>
        /// <returns></returns>
        public async Task<List<IPCCodeDelayClassModel>> GetCodeDelayClass(string DELAY_CLASS_ID, bool? DEL_FLG)
        {
            return await dac.GetCodeDelayClass(DELAY_CLASS_ID, DEL_FLG);
        }

        /// <summary>
        /// 儲存落後原因類別
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> SaveCodeDelayClass(List<IPCCodeDelayClassModel> models)
        {
            // 檢查
            string errMsg = await CheckCodeDelayClasses(models);
            if (!string.IsNullOrEmpty(errMsg))
            {
                return ChangeResult(false, errMsg);
            }

            List<IPCCodeDelayClassModel> insertData = models.Where(x => (editTypeEnum)x.editType == editTypeEnum.Add).ToList();
            List<IPCCodeDelayClassModel> updateData = models.Where(x => (editTypeEnum)x.editType == editTypeEnum.Modify).ToList();

            dac.BeginTransaction();
            dac.InsertCodeDelayClass(insertData);
            dac.UpdateCodeDelayClass(updateData);
            dac.Commit();
            return ChangeResult(true, "存檔成功");
        }

        /// <summary>
        /// 檢查落後原因類別
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private async Task<string> CheckCodeDelayClasses(List<IPCCodeDelayClassModel> models)
        {
            List<IPCCodeDelayClassModel> checkData = models.Where(x => x.OLD_DELAY_CLASS_SUB_ID != x.DELAY_CLASS_SUB_ID).ToList();
            if (!checkData.Any())
            {
                return string.Empty;
            }

            // 判斷 落後項目代碼 是否已存在
            List<string> subIds = await dac.GetExistDelayClassSubId(checkData.Select(x => x.DELAY_CLASS_SUB_ID).ToArray());
            // 判斷 落後項目 是否已存在
            List<string> subItems = await dac.GetExistDelayClassSubItem(checkData.Select(x => x.DELAY_CLASS_SUB_ITEM).ToArray());
            // 取得計畫落後原因的落後項目清單
            List<string> delaySubClasses = await projectCommonDac.GetDelaySubClasses();
            List<string> useSubIds = checkData.Where(x => delaySubClasses.Contains(x.OLD_DELAY_CLASS_SUB_ID)).Select(x => x.OLD_DELAY_CLASS_SUB_ID).ToList();

            if (subIds.Any() || subItems.Any() || useSubIds.Any())
            {
                List<string> errMsgs = new List<string>();
                if (subIds.Any())
                {
                    errMsgs.Add($"落後項目代碼 {string.Join("、", subIds)} 已存在");
                }
                if (subItems.Any())
                {
                    errMsgs.Add($"落後項目 {string.Join("、", subItems)} 已存在");
                }
                if (useSubIds.Any())
                {
                    errMsgs.Add($"落後項目代碼 {string.Join("、", useSubIds)} 已使用");
                }
                return $"存檔失敗\n{string.Join("\n", errMsgs)}";
            }

            return string.Empty;
        }

        /// <summary>
        /// 取得預算來源
        /// </summary>
        /// <param name="LEVEL_MARK">1:本府預算來源 2:中央預算來源</param>
        /// <param name="DEL_FLG"></param>
        /// <returns></returns>
        public async Task<List<IPCCodePlanItemModel>> GetCodePlanItem(string LEVEL_MARK, bool? DEL_FLG)
        {
            return await dac.GetCodePlanItem(LEVEL_MARK, DEL_FLG);
        }

        /// <summary>
        /// 儲存預算來源
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> SaveCodePlanItem(List<IPCCodePlanItemModel> models)
        {
            // 檢查
            string errMsg = await CheckCodePlanItems(models);
            if (!string.IsNullOrEmpty(errMsg))
            {
                return ChangeResult(false, errMsg);
            }

            List<IPCCodePlanItemModel> insertData = 
                models.Where(x => (editTypeEnum)x.editType == editTypeEnum.Add).ToList();
            List<IPCCodePlanItemModel> updateData = 
                models.Where(x => (editTypeEnum)x.editType == editTypeEnum.Modify).ToList();

            dac.BeginTransaction();
            dac.InsertCodePlanItem(insertData);
            dac.UpdateCodePlanItem(updateData);
            dac.Commit();
            return ChangeResult(true, "存檔成功");
        }

        /// <summary>
        /// 檢查預算來源
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private async Task<string> CheckCodePlanItems(List<IPCCodePlanItemModel> models)
        {
            List<IPCCodePlanItemModel> checkData = models.Where(x => x.OLD_PLAN_ITEM_ID != x.PLAN_ITEM_ID).ToList();
            if (!checkData.Any())
            {
                return string.Empty;
            }

            List<string> usedCodes = await projectCommonDac.GetPlanItems(checkData.First().LEVEL_MARK);
            List<string> usePlanItemIds = checkData.Where(x => usedCodes.Contains(x.OLD_PLAN_ITEM_ID)).Select(x => x.OLD_PLAN_ITEM_ID).ToList();
            if (usePlanItemIds.Any())
            {
                return $"儲存失敗\n預算編號 {string.Join("、", usePlanItemIds)} 已使用";
            }

            return string.Empty;
        }

        /// <summary>
        /// 判斷有無該年度工作日
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<int> GetWorkingDayCountByYear(int year)
        {
            return await dac.GetWorkingDayCountByYear(year);
        }

        /// <summary>
        /// 取得工作日
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public async Task<List<IPCWorkingDayModel>> GetWorkingDay(string startDate, string endDate)
        {
            return await dac.GetWorkingDay(startDate, endDate);
        }

        /// <summary>
        /// 產生年度工作日
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> GenerateWorkingDay(int year)
        {
            //確認是否已有該年度工作日
            int count = await dac.GetWorkingDayCountByYear(year);
            if (count > 0)
            {
                return ChangeResult(false, "已有明年工作日");
            }

            List<IPCWorkingDayModel> workingDay = new List<IPCWorkingDayModel>();
            DateTime day = new DateTime(year, 1, 1);
            for (int i = 0; i < day.DayOfYear; i++)
            {
                int week = (int)day.DayOfWeek;
                workingDay.Add(new IPCWorkingDayModel
                {
                    DATE = day,
                    IS_WORKING = week != 0 && week != 6
                });
                day = day.AddDays(1);
            }

            dac.InsertWorkingDay(workingDay);
            return ChangeResult(true, "明年工作日產生成功");
        }

        /// <summary>
        /// 儲存工作日
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public RtnResultModel SaveWorkingDay(List<IPCWorkingDayModel> models)
        {
            dac.UpdateWorkingDay(models);
            return ChangeResult(true, "存檔成功");
        }

        /// <summary>
        /// 取得機關窗口維護
        /// </summary>
        /// <returns></returns>
        public async Task<List<IPCSetContactModel>> GetSetContact()
        {
            List<IPCSetContactModel> contactList = new();
            //取得機關清單
            List<DropDownListModel> orgList = await dropdownDac.GetOrganList();
            //取得所有機關聯絡窗口資料
            List<IPCDeptContactModel> data = await dac.GetDeptContact();
            //各機關聯絡窗口資料
            foreach (DropDownListModel item in orgList)
            {
                contactList.Add(new IPCSetContactModel
                {
                    ORGAN = item.value,
                    ORGAN_NAME = item.text,
                    DeptContact = data.Where(x => x.ORGAN == item.value).ToList()
                });
            }
            return contactList;
        }

        /// <summary>
        /// 取得機關聯絡窗口
        /// </summary>
        /// <param name="ORGAN"></param>
        /// <returns></returns>
        public async Task<List<IPCDeptContactModel>> GetSetContactByOrgan(string ORGAN)
        {         
            return await dac.GetDeptContactByOrgan(ORGAN);
        }

        /// <summary>
        /// 儲存機關窗口維護
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public RtnResultModel SaveSetContact(IPCSetContactModel model)
        {
            dac.BeginTransaction();
            dac.DeleteDeptContact(model.ORGAN);
            dac.InsertDeptContact(model.DeptContact);
            dac.Commit();                
            return ChangeResult(true, "存檔成功");
        }
    }
}
