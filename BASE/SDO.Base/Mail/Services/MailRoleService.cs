using SDO.Dac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SDO.Models;
using System.Transactions;
using SDO.Utils;

namespace SDO.Services
{
    public class MailRoleService : Service, IMailRoleService
    {
        private readonly IMailRoleDac dac;
        private readonly IUserProfile userProfile;
        public MailRoleService(IMailRoleDac dac,
            IUserProfile userProfile)
        {
            this.dac = dac;
            this.userProfile = userProfile;
        }
        public MailRoleService(IMailRoleDac dac)
        {
            this.dac = dac;
        }

        public async Task<IList<UserCountModel>> Read()
        {
            return await dac.Read();
        }

        public async Task<MailRoleModel> ReadById(string roleId)
        {
            return await dac.ReadById(roleId);
        }

        public async Task<IList<MailRoleuUserModel>> ReadUsers(string roleId)
        {
            return await dac.ReadUsers(roleId);
        }

        public async Task<RtnResultModel> Create(MailRoleMdfModel role)
        {
            (bool isExists, bool delFlg) = await IsExistsAndDelFlg(role.ROLE_ID);
            if (isExists && !delFlg)
                return ChangeResult(ResultType.Fail | ResultType.Insert, role.ROLE_ID);

            role.CRT_USER = userProfile.GetLoginUser().USER_ID;
            role.MDF_USER = userProfile.GetLoginUser().USER_ID;

            dac.BeginTransaction();
            TaskQueue taskQueue = new TaskQueue();

            if (isExists)
                taskQueue.AddTask(dac.Update(role));
            else
                taskQueue.AddTask(dac.Insert(role));

            taskQueue.AddTask(UpdateMapUserMailRole(role));
            await taskQueue.Done();
            dac.Commit();

            return ChangeResult(ResultType.Success | ResultType.Insert);
        }

        public async Task<RtnResultModel> Update(MailRoleMdfModel role)
        {
            role.MDF_USER = userProfile.GetLoginUser().USER_ID;

            dac.BeginTransaction();
            TaskQueue taskQueue = new TaskQueue();
            taskQueue.AddTask(dac.Update(role));
            taskQueue.AddTask(UpdateMapUserMailRole(role));
            await taskQueue.Done();
            dac.Commit();

            return ChangeResult(ResultType.Success | ResultType.Update);
        }

        public async Task<RtnResultModel> Delete(string roleId)
        {
            dac.BeginTransaction();
            TaskQueue taskQueue = new TaskQueue();
            taskQueue.AddTask(dac.Delete(roleId));
            taskQueue.AddTask(DeleteMap(roleId));
            await taskQueue.Done();
            dac.Commit();

            return ChangeResult(ResultType.Success | ResultType.Delete);
        }


        private async Task<(bool isExists, bool delFlg)> IsExistsAndDelFlg(string roleId)
        {
            MailRoleModel role = await dac.ReadById(roleId);
            if (role == default(MailRoleModel))
                return (isExists: false, delFlg: false);
            return (isExists: true, delFlg: role.DEL_FLG);
        }

        private async Task UpdateMapUserMailRole(MailRoleMdfModel role)
        {
            Task deleteMap = DeleteMap(role.ROLE_ID);

            ParseUtil.Parse(role, out MapUserMailRoleModel map);

            await deleteMap;

            await InsertMap(map);
        }

        private async Task DeleteMap(string roleId)
        {
            await dac.DeleteMap(roleId);
        }

        private async Task InsertMap(MapUserMailRoleModel map)
        {
            if (map.USERS == null || !map.USERS.Any())
                return;

            MapUserMailRoleMdfModel[] param = new MapUserMailRoleMdfModel[map.USERS.Length];
            Parallel.For(0, map.USERS.Length, (i) =>
            {
                param[i] = new MapUserMailRoleMdfModel()
                {
                    ROLE_ID = map.ROLE_ID,
                    USER_ID = map.USERS[i],
                    CRT_USER = userProfile.GetLoginUser().USER_ID
                };
            });

            await dac.InsertMap(param);
            return;
        }
    }
}