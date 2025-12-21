using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SDO.Services
{
    public class DimRightService : Service, IDimRightService
    {
        private readonly IDimRightDac dac;

        public DimRightService(IDimRightDac dac)
        {
            this.dac = dac;
        }

        public async Task<IList<DimRightModel>> Read()
        {
            return await dac.Read();
        }

        public async Task<DimRightModel> ReadById(string rightId)
        {
            return await dac.ReadById(rightId, true);
        }

        public async Task<IList<DimRightModel>> ReadByRole(string roleId = "")
        {
            return await dac.ReadByRole(roleId);
        }

        public async Task<RtnResultModel> Create(DimRightModel right)
        {
            (bool isExist, bool delFlg) = await IsExistAndDelFlg(right);

            if (isExist && !delFlg)
                return ChangeResult(ResultType.Fail | ResultType.Insert, HtmlEncode(right.RIGHT_ID));

            right.DEL_FLG = false;

            dac.BeginTransaction();

            if (isExist && delFlg)
                await dac.Update(right);
            else
                await dac.Insert(right);

            await UpdateMaoRightFunction(right);
            dac.Commit();

            return ChangeResult(ResultType.Success | ResultType.Insert);
        }

        public async Task<RtnResultModel> Update(DimRightModel right)
        {
            dac.BeginTransaction();
            TaskQueue taskQueue = new TaskQueue();
            taskQueue.AddTask(dac.Update(right));
            taskQueue.AddTask(UpdateMaoRightFunction(right));
            await taskQueue.Done();
            dac.Commit();

            return ChangeResult(ResultType.Success | ResultType.Update);
        }

        public async Task<RtnResultModel> Delete(string rightId)
        {
            dac.BeginTransaction();
            await dac.Delete(rightId);
            await dac.DeleteMap(rightId);
            dac.Commit();
            return ChangeResult(ResultType.Success | ResultType.Delete);
        }

        /// <summary>
        /// 更新權利功能對應
        /// </summary>
        /// <param name="right"></param>
        /// <returns></returns>
        private async Task UpdateMaoRightFunction(DimRightModel right)
        {

            await dac.DeleteMap(right.RIGHT_ID);

            IList<MapRightFunctionModel> mapParams = new List<MapRightFunctionModel>();
            foreach (string function in right.FUNCTIONS)
            {
                MapRightFunctionModel mapParam = new MapRightFunctionModel()
                {
                    RIGHT_ID = right.RIGHT_ID,
                    FUNCTION_ID = function
                };
                mapParams.Add(mapParam);
            }

            if (mapParams.Any())
                await dac.InsertMap(mapParams);
        }

        /// <summary>
        /// 確認Right是否存在及取得其DelFlg
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        private async Task<(bool isExist, bool delFlg)> IsExistAndDelFlg(DimRightModel right)
        {
            DimRightModel result = await dac.ReadById(right.RIGHT_ID, false);
            if (result == default(DimRightModel))
                return (isExist: false, delFlg: false);
            return (isExist: true, delFlg: result.DEL_FLG);
        }
    }
}
