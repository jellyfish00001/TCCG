using Ionic.Zip;
using Microsoft.AspNetCore.StaticFiles;
using SDO.Base.Utils;
using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class IPCSetParamService : SetParamService, IIPCSetParamService
    {
        private IIPCSetParamDac ipcSetParamDac;
        private ISetParamDac setParamDac;
        private IIPCCodeDac ipcCodeDac;
        private IProjectCommonDac projectCommonDac;

        public IPCSetParamService(ISetParamDac setParamDac, 
            Utils.ISysParam sysParam, 
            IIPCSetParamDac ipcSetParamDac,
            IIPCCodeDac ipcCodeDac,
            IProjectCommonDac projectCommonDac) : base(setParamDac, sysParam)
        {
            this.ipcSetParamDac = ipcSetParamDac;
            this.setParamDac = setParamDac;
            this.ipcCodeDac = ipcCodeDac;
            this.projectCommonDac = projectCommonDac;
        }

        /// <summary>
        /// 取得setItem類別的系統參數
        /// </summary>
        /// <param name="setItem"></param>
        /// <param name="delFlg"></param>
        /// <returns></returns>
        public async Task<IList<IPCSetParamModel>> GetSysParams(string setItem, bool? delFlg, int fromWhere = 0)
        {
            return await ipcSetParamDac.GetSysParams(setItem, delFlg, fromWhere);
        }

        /// <summary>
        /// 取得多組system param
        /// </summary>
        /// <param name="setItems">key:setItem；value:是否有預設值</param>
        /// <returns></returns>
        public Task<Dictionary<string, object>> GetParamByItems(Dictionary<string, bool> setItems)
        {
            Dictionary<string, object> ddl = new Dictionary<string, object>();
            List<Task> tasks = new List<Task>();
            object lockObj = new ();
            foreach (string setItem in setItems.Keys)
            {
                Task t = Task.Run(async () =>
                {
                    IList<IPCSetParamModel> result = await GetSysParams(setItem,false);
                    if (setItems[setItem])
                    {
                        result.Insert(0, new IPCSetParamModel { SET_VALUE = "請選擇", SET_TYPE = "" });
                    }

                    lock (lockObj)
                    {
                        ddl.Add(setItem, result);
                    }
                });
                tasks.Add(t);
            }
            Task.WaitAll(tasks.ToArray());
            return Task.FromResult(ddl);
        }

        /// <summary>
        /// 儲存SYS_PARAM資料
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> SaveParamItem(List<IPCSetParamModel> models)
        {
            // 檢查
            string errMsg = await CheckParamItems(models);
            if (!string.IsNullOrEmpty(errMsg))
            {
                return ChangeResult(false, errMsg);
            }

            List<IPCSetParamModel> createModels = 
                models.Where(x => (editTypeEnum)x.editType == editTypeEnum.Add).ToList();
            List<IPCSetParamModel> updateModels = 
                models.Where(x => (editTypeEnum)x.editType == editTypeEnum.Modify).ToList();
            List<IPCSetParamModel> deleteModels = 
                models.Where(x => (editTypeEnum)x.editType == editTypeEnum.Delete).ToList();

            ipcSetParamDac.BeginTransaction();

            // 新增
            if (createModels.Any())
            {
                ipcSetParamDac.Insert(createModels);
            }

            // 修改
            if (updateModels.Any())
            {
                ipcSetParamDac.Update(updateModels);

                if (updateModels.All(x => x.SET_ITEM == "DELAY_TYPE"))
                {
                    ipcCodeDac.UpdateCodeDelayClass(updateModels);
                }
            }

            // 刪除
            if (deleteModels.Any())
            {
                foreach (var model in deleteModels)
                {
                    await setParamDac.Delete(model.SET_ITEM, model.SET_TYPE);
                }
            }

            ipcSetParamDac.Commit();
            return ChangeResult(true, "存檔成功");
        }

        /// <summary>
        /// 檢查資料
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private async Task<string> CheckParamItems(List<IPCSetParamModel> models)
        {
            List<IPCSetParamModel> checkData = models.Where(x => x.OLD_SET_TYPE != x.SET_TYPE).ToList();
            if (!checkData.Any())
            {
                return string.Empty;
            }

            List<string> errMsgs = new();

            // 落後類別
            if (checkData.All(x => x.SET_ITEM == "DELAY_TYPE"))
            {
                List<string> delayClasses = await projectCommonDac.GetDelayClasses();
                List<string> useSetTypes = checkData.Where(x => delayClasses.Contains(x.OLD_SET_TYPE)).Select(x => x.OLD_SET_TYPE).ToList();
                if (useSetTypes.Any())
                {
                    errMsgs.Add($"類別代碼 {string.Join("、", useSetTypes)} 已使用");
                }
            }

            // 特殊加註
            if (checkData.All(x => x.SET_ITEM == "SPEC_NOTE"))
            {
                List<string> usedCodes = await projectCommonDac.GetUsedCode("SPEC_NOTE");
                List<string> useSetTypes = checkData.Where(x => usedCodes.Contains(x.OLD_SET_TYPE)).Select(x => x.OLD_SET_TYPE).ToList();
                if (useSetTypes.Any())
                {
                    errMsgs.Add($"設定代碼 {string.Join("、", useSetTypes)} 已使用");
                }
            }

            return errMsgs.Any() ? $"儲存失敗\n{string.Join("\n", errMsgs)}" : string.Empty;
        }
    }
}
