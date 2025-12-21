using SDO.Dac;
using SDO.Models;
using SDO.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace SDO.Services
{
    public class DimRoleService : Service, IDimRoleService
    {
        private readonly IDimRoleDac dac;
        private readonly IUserProfile userProfile;

        public DimRoleService(IDimRoleDac dac,
            IUserProfile userProfile)
        {
            this.dac = dac;
            this.userProfile = userProfile;
        }

        public async Task<IList<DimRoleModel>> Read()
        {
            return await dac.Read();
        }

        public async Task<DimRoleModel> ReadById(string roleId)
        {
            return await dac.ReadById(roleId, true);
        }

        public async Task<RtnResultModel> Create(DimRoleMdfModel role)
        {
            (bool isExist, bool delFlg) = await IsExistAndDelFlg(role);

            if (isExist && !delFlg)
                return ChangeResult(ResultType.Fail | ResultType.Insert, HtmlEncode(role.ROLE_ID));

            dac.BeginTransaction();
            TaskQueue taskQueue = new TaskQueue();

            if (isExist && delFlg)
            {
                taskQueue.AddTask(dac.Update(role));
            }
            else
            {
                taskQueue.AddTask(dac.Insert(role));
            }

            taskQueue.AddTask(UpdateMapRoleRight(role));
            await taskQueue.Done();
            dac.Commit();

            return ChangeResult(ResultType.Success | ResultType.Insert);
        }

        public async Task<RtnResultModel> Update(DimRoleMdfModel role)
        {
            dac.BeginTransaction();
            TaskQueue taskQueue = new TaskQueue();
            taskQueue.AddTask(dac.Update(role));
            taskQueue.AddTask(UpdateMapRoleRight(role));
            await taskQueue.Done();
            dac.Commit();

            return ChangeResult(ResultType.Success | ResultType.Update);
        }

        public async Task<RtnResultModel> Delete(string roleId)
        {
            dac.BeginTransaction();
            TaskQueue taskQueue = new TaskQueue();
            taskQueue.AddTask(dac.Delete(roleId));
            taskQueue.AddTask(dac.DeleteMap(roleId));
            await taskQueue.Done();
            dac.Commit();

            return ChangeResult(ResultType.Success | ResultType.Delete);
        }

        public async Task<IList<DimRoleModel>> ReadByUser(string userId)
        {
            return await dac.ReadByUser(userId);
        }

        /// <summary>
        /// 確認Role是否存在及取得其DelFlg
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        private async Task<(bool isExist, bool delFlg)> IsExistAndDelFlg(DimRoleMdfModel role)
        {
            DimRoleModel result = await dac.ReadById(role.ROLE_ID, false);
            if (result == default(DimRoleModel))
                return (isExist: false, delFlg: false);
            return (isExist: true, delFlg: result.DEL_FLG);
        }

        /// <summary>
        /// 更新角色權利對應
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        private async Task UpdateMapRoleRight(DimRoleMdfModel role)
        {
            await dac.DeleteMap(role.ROLE_ID);

            IList<MapRoleRightModel> mapRoles = new List<MapRoleRightModel>();
            foreach (string rightId in role.RIGHTS)
            {
                mapRoles.Add(new MapRoleRightModel()
                {
                    ROLE_ID = role.ROLE_ID,
                    RIGHT_ID = rightId,
                    CRT_USER = userProfile.GetLoginUser().USER_ID
                });
            }

            if (mapRoles.Any())
                await dac.InsertMap(mapRoles);
        }
    }
}