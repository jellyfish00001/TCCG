using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SDO.Dac;
using SDO.Models;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using SDO.Utils;
using SDO.Base.Utils;

namespace SDO.Services
{
    public class EmpUserService : Service, IEmpUserService
    {
        private readonly IEmpUserDac dac;
        private readonly IScPolicyDac scPolicyDac;
        private readonly IPdHisMainDac pdHisMainDac;
        private readonly IUserProfile userProfile;
        private readonly IUserData userData;

        public EmpUserService(IEmpUserDac dac,
            IScPolicyDac scPolicyDac,
            IUserProfile userProfile,
            IPdHisMainDac pdHisMainDac)
        {
            this.dac = dac;
            this.scPolicyDac = scPolicyDac;
            this.userProfile = userProfile;
            this.pdHisMainDac = pdHisMainDac;
            this.userData = userProfile?.GetLoginUser();
        }

        public async Task<UserDataModel> GetUserById(string id, bool delFlg = false)
        {
            return await dac.GetUserById(id, delFlg);
        }

        public async Task<IList<UserDataModel>> GetUserByIds(string Ids)
        {
            string[] userIds = Ids.Split(',', StringSplitOptions.RemoveEmptyEntries);
            return await dac.GetUserByIds(userIds);
        }
        /// <summary>
        /// 取得登入驗證
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="PD"></param>
        /// <param name="Sys"></param>
        /// <returns></returns>
        public async virtual Task<(UserDataModel user,bool success)> GetLogInData(string Id, string PD, string Sys)
        {
            var user = await dac.GetUserById(Id);
            var loginUserID = (await dac.Login(new() { USER_ID = Id, USER_PD = PD }))?.USER_ID;
            return (user, !string.IsNullOrEmpty(loginUserID));
        }


        public async Task<IList<UserDataModel>> GetUserByOrg(string orgId = "")
        {
            return await dac.GetUserByOrg(orgId);
        }

        public async Task UpdateLoginInfo(string userId, bool login, bool flag = false)
        {
            if (!flag)
            {
                UserDataModel user = await GetUserById(userId);
                await dac.UpdateLoginInfo(userId, login, user.CON_FAULT, user.NONCON_FAULT, false);
            }
            else
            {
                await dac.UpdateLoginInfo(userId, login, flag: true);
            }
        }

        public async Task<UserDataModel> Login(LoginModel model)
        {
            return await dac.Login(model);
        }

        public async Task<IList<UserDataModel>> Read(EmpUserReadModel model)
        {
            return await dac.Read(model);
        }

        public async Task<RtnResultModel> Create(EmpUserModel createModel)
        {
            //確認該USER_ID 是否已存在並啟用
            (bool isExist, bool delFlg) = await CheckUserIdIsExists(createModel.USER_ID, false);
            if (isExist)
            {
                return ChangeResult(false, "帳號已存在");
            }

            //檢查帳號密碼原則
            UserDataModel checkModel = new UserDataModel()
            {
                USER_ID = createModel.USER_ID,
                USER_NAME = createModel.USER_NAME,
                USER_PD = createModel.USER_PD
            };

            string strPolicy = await CheckPolicy(checkModel, true);
            if (!string.IsNullOrWhiteSpace(strPolicy))
            {
                return ChangeResult(false, strPolicy);
            }

            dac.BeginTransaction();
            await dac.Insert(createModel);

            TaskQueue taskQueue = new TaskQueue();
            taskQueue.AddTask(UpdatePassword(createModel.USER_ID, createModel.USER_PD));//更新密碼
            taskQueue.AddTask(pdHisMainDac.Create(createModel.USER_ID, createModel.USER_PD));//加入密碼歷程

            await DeleteMapUserRole(createModel.USER_ID);//刪除使用者角色對應
            taskQueue.AddTask(InsertMapUserRole(createModel));

            taskQueue.AddTask(SetMapOrgUser(createModel)); // 寫入組織

            await taskQueue.Done();
            dac.Commit();

            return ChangeResult(true, "存檔成功");
        }

        public async Task<RtnResultModel> Update(EmpUserModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.USER_PD))
            {
                //檢查帳號密碼原則
                UserDataModel checkModel = new UserDataModel()
                {
                    USER_ID = model.USER_ID,
                    USER_NAME = model.USER_NAME,
                    USER_PD = model.USER_PD
                };
                string strPolicy = await CheckPolicy(checkModel, false);
                if (!string.IsNullOrWhiteSpace(strPolicy))
                    return ChangeResult(false, strPolicy);
            }

            model.MDF_USER = userProfile.GetLoginUser().USER_ID;

            bool changePd = !string.IsNullOrWhiteSpace(model.USER_PD);
            dac.BeginTransaction();
            await dac.Update(model);//更新使用者資料

            TaskQueue taskQueue = new TaskQueue();
            if (changePd)//是否更新密碼
            {
                taskQueue.AddTask(UpdatePassword(model.USER_ID, model.USER_PD));//更新密碼
                taskQueue.AddTask(pdHisMainDac.Create(model.USER_ID, model.USER_PD));//加入密碼歷程
            }
            await DeleteMapUserRole(model.USER_ID);//刪除使用者角色對應
            taskQueue.AddTask(InsertMapUserRole(model));

            taskQueue.AddTask(SetMapOrgUser(model)); // 寫入組織

            await taskQueue.Done();//等待所有Task完成
            dac.Commit();

            return ChangeResult(true, "存檔成功");
        }

        public async Task<RtnResultModel> Delete(string userId, string del_reason)
        {
            dac.BeginTransaction();
            TaskQueue taskQueue = new TaskQueue();
            taskQueue.AddTask(dac.Delete(userId, del_reason));
            taskQueue.AddTask(DeleteMapUserRole(userId));
            await taskQueue.Done();
            dac.Commit();
            return ChangeResult(ResultType.Success | ResultType.Delete);
        }

        private async Task<(bool isExist, bool delFlg)> CheckUserIdIsExists(string userId, bool delFlg)
        {
            UserDataModel result = await dac.CheckExists(userId, delFlg);
            if (result == default(UserDataModel))
                return (isExist: false, delFlg: false);
            return (isExist: true, delFlg: result.DEL_FLG);
        }

        private async Task UpdatePassword(string userId, string userPd)
        {
            await dac.UpdatePassword(userId, userPd);
        }

        public async Task<IList<SCUserModel>> ReadUserAgent()
        {
            IList<SCUserModel> userAgentList = new List<SCUserModel>();

            if (string.IsNullOrWhiteSpace(userProfile.GetLoginUser().AGENT_ID))
            {
                //登入身分不是代理人時，查出可代理的人員   
                userAgentList = await dac.ReadUserAgent(userProfile.GetLoginUser().USER_ID);
            }
            //將登入者+至data第一筆
            userAgentList.Insert(0, (SCUserModel)userProfile.GetLoginUser());
            return userAgentList;
        }

        /// <summary>
        /// 驗證輸入是否符合SCPOLICYM規範
        /// </summary>
        /// <param name="model"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual async Task<string> CheckPolicy(UserDataModel model, bool IsCreate, string PolicyID = "SDO")
        {
            string UserTable = "EMP_USER";
            StringBuilder strPolicy = new StringBuilder("");
            //根據POLICY_ID、POLICY_COMP_ID取得POLICY
            ScPolicyModel policy = await scPolicyDac.GetPolicy(PolicyID, "GSS");
            GetUserTable(PolicyID);
            if (policy != null)
            {
                bool checkResult;
                //當Action為create時驗證帳號長度
                if (model.USER_ID.Length < policy.ID_MIN_LEN || model.USER_ID.Length > 50 && IsCreate)
                {
                    strPolicy.Append(i18N.Message.S01 + policy.ID_MIN_LEN + "<br>");
                }
                //如果使用者填入密碼為空，只進行密碼長度驗證
                if (String.IsNullOrEmpty(model.USER_PD))
                {
                    if (policy.PASS_MIN_LEN != 0)
                        strPolicy.Append(i18N.Message.S02 + policy.PASS_MIN_LEN + "<br>");
                }
                //否則進行所有密碼設定限制的驗證
                else
                {
                    //驗證密碼長度
                    if (model.USER_PD.Length < policy.PASS_MIN_LEN || model.USER_PD.Length > 50)
                    {
                        strPolicy.Append(i18N.Message.S02 + policy.PASS_MIN_LEN + "<br>");
                    }
                    //如果設定密碼不能與使用者帳號與名稱相同，驗證密碼是否與使用者帳號或名稱相同
                    if ('Y'.Equals(policy.PASS_NO_SAME_ID_NAME) && model.USER_PD.Equals(model.USER_ID) || model.USER_PD.Equals(model.USER_NAME))
                    {
                        strPolicy.Append(i18N.Message.S03 + "<br>");
                    }
                    //如果設定密碼不能含有特殊字元
                    if ('Y'.Equals(policy.PASS_NO_SPEC_CHAR))
                    {
                        //判斷各字元是否為特殊字
                        foreach (char c in model.USER_PD)
                        {
                            if (!Regex.IsMatch(c.ToString(), @"[A-Za-z0-9]"))
                            {
                                strPolicy.Append(i18N.Message.S04 + "<br>");
                                break;
                            }
                        }
                    }
                    //如果設定密碼至少含n個特殊字元
                    if (policy.PASS_AT_LEAST_SPECIAL_CHARS > 0)
                    {
                        int countSpec = 0;
                        //加總特殊字元個數
                        for (int i = 0; i < model.USER_PD.Length; i++)
                        {
                            string p = model.USER_PD[i].ToString();
                            if (!Regex.IsMatch(p, @"[A-Za-z0-9]"))
                                countSpec += 1;
                        }
                        if (countSpec < policy.PASS_AT_LEAST_SPECIAL_CHARS)
                            strPolicy.Append(i18N.Message.S05 + policy.PASS_AT_LEAST_SPECIAL_CHARS + "<br>");
                    }
                    //如果設定密碼必須為文數字組合
                    if ('Y'.Equals(policy.PASS_MIX_CHAR_NUM))
                    {
                        int countUpperCase = 0;
                        int countNum = 0;
                        int countLowerCase = 0;
                        //判斷各類字元存在
                        for (int i = 0; i < model.USER_PD.Length; i++)
                        {
                            string str = model.USER_PD[i].ToString();
                            if (Regex.IsMatch(str, @"[A-Z]"))
                                countUpperCase = 1;
                            if (Regex.IsMatch(str, @"[a-z]"))
                                countLowerCase = 1;
                            if (Regex.IsMatch(str, @"[0-9]"))
                                countNum = 1;
                        }
                        if (countNum != 1 || countLowerCase + countUpperCase < 1)
                            strPolicy.Append(i18N.Message.S06 + "<br>");
                    }
                    //如果設定密碼相鄰兩字元不可相同
                    if ('Y'.Equals(policy.PASS_NO_SAME_2))
                    {
                        string laststr = "";
                        checkResult = true;
                        //比較相鄰兩字元是否相同
                        for (int i = 0; i < model.USER_PD.Length; i++)
                        {
                            string str = model.USER_PD[i].ToString();
                            if (laststr == str)
                                checkResult = false;
                            laststr = str;
                        }
                        if (!checkResult)
                            strPolicy.Append(i18N.Message.S07 + "<br>");
                    }
                    //如果設定密碼相鄰三字元不可為連續升冪或降冪
                    if ('Y'.Equals(policy.PASS_NO_CONT_3) && model.USER_PD.Length > 2)
                    {
                        checkResult = true;
                        //以ASCII判斷是否相鄰三字元為連續
                        for (int i = 0; i < model.USER_PD.Length - 2; i++)
                        {
                            int p = Asc(model.USER_PD[i]);
                            int p1 = Asc(model.USER_PD[i + 1]);
                            int p2 = Asc(model.USER_PD[i + 2]);
                            if (p1 == p + 1 && p2 == p1 + 1)
                                checkResult = false;
                        }
                        if (!checkResult)
                            strPolicy.Append(i18N.Message.S08 + "<br>");
                    }
                }

                //當Action為update且設定新密碼不能與過去?次重複才進行以下驗證
                if (policy.PASS_NO_SAME_PASS_TIMES > 0 && !IsCreate)
                {
                    //取得密碼變更紀錄檔，依照設定取得前幾筆紀錄
                    if (await pdHisMainDac.CheckPdSamePassTime(model.USER_ID, model.USER_PD, policy.PASS_NO_SAME_PASS_TIMES, UserTable))
                    {
                        strPolicy.Append(i18N.Message.S09 + policy.PASS_NO_SAME_PASS_TIMES + i18N.Message.S10 + "<br>");
                    }
                }
            }
            return strPolicy.ToString();
        }
        protected  virtual string  GetUserTable(string PolicyID = "SDO")
        {
            return "EMP_USER";
        }
        private int Asc(char chr)
        {
            return Convert.ToInt32(chr);
        }

        private async Task DeleteMapUserRole(string userId)
        {
            await dac.DeleteMap(userId);
        }

        private async Task InsertMapUserRole(EmpUserModel createModel)
        {
            if (createModel.ROLES != null && createModel.ROLES.Any())
            {
                IList<MapUserRoleModel> maps = new List<MapUserRoleModel>();
                foreach (string roleId in createModel.ROLES)
                {
                    maps.Add(new MapUserRoleModel()
                    {
                        ROLE_ID = roleId,
                        USER_ID = createModel.USER_ID
                    });
                }
                await dac.InsertMap(maps);
            }
        }

        /// <summary>
        /// 設定使用者組織
        /// </summary>
        /// <param name="createModel"></param>
        /// <returns></returns>
        private async Task SetMapOrgUser(EmpUserModel createModel)
        {
            // 先刪除
            await dac.DeleteMapOrgUser(createModel.USER_ID);
            // 後新增
            if (!string.IsNullOrEmpty(createModel.ORG_Id))
            {
                await dac.InsertMapOrgUser(createModel.ORG_Id, createModel.USER_ID, createModel.MDF_USER ?? createModel.CRT_USER);
            }
        }
    }
}
