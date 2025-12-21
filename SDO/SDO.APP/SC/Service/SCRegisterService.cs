using SDO.Dac;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SDO.Services
{
    public class SCRegisterService : Service, ISCRegisterService
    {
        private readonly ISCUserDac scUserDac;
        private readonly ISecureRandomNum randomNum;
        private readonly IMailSetService mail;

        public SCRegisterService(ISCUserDac scUserDac, ISecureRandomNum randomNum, IMailSetService mail)
        {
            this.scUserDac = scUserDac;
            this.randomNum = randomNum;
            this.mail = mail;
        }

        /// <summary>
        /// 取得SC密碼規則
        /// </summary>
        /// <returns></returns>
        public async Task<SCPolicyModel> GetSCPolicy()
        {
            return await scUserDac.GetSCPolicy();
        }

        /// <summary>
        /// 密碼變更
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> ResetPW(SCRefreshModel model)
        {
            // 帳號密碼輸入錯誤
            SCUserModel scUser = await scUserDac.Login(new LoginModel { USER_ID = model.USER_ID, USER_PD = model.ORIGINAL_USER_PD });
            if (scUser == null)
            {
                return ChangeResult(false, "帳號密碼錯誤");
            }

            // 檢查密碼規則
            List<string> errMsgs = await CheckSCPolicy(model);
            if (errMsgs.Any())
            {
                return ChangeResult(false, string.Join("\n", errMsgs));
            }

            // 更新密碼
            await scUserDac.UpdatePassword(model.USER_ID, model.USER_PD);
            return ChangeResult(true, "密碼變更成功");
        }

        /// <summary>
        /// 檢查SC密碼規則
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<List<string>> CheckSCPolicy(SCRefreshModel model)
        {
            SCPolicyModel scPolicy = await scUserDac.GetSCPolicy();

            List<string> errMsgs = new List<string>();
            // 密碼最短長度限制 ※密碼為空，則直接回傳，以下不檢查
            if (model.USER_PD.Length < scPolicy.PASS_MIN_LEN)
            {
                errMsgs.Add($"密碼長度至少 {scPolicy.PASS_MIN_LEN} 個字元");

                // 密碼為空，則直接回傳，以下不檢查
                if (string.IsNullOrEmpty(model.USER_PD))
                {
                    return errMsgs;
                }
            }

            // 密碼不可與使用者代號或使用者名稱相同
            if (scPolicy.PASS_NO_SAME_ID_NAME == "Y" && model.USER_ID == model.USER_PD)
            {
                errMsgs.Add("密碼不可與帳號或姓名相同");
            }

            // 密碼4項至少需符合3項(大寫英文字母,小寫英文字母,數字,特殊符號)
            if (scPolicy.PASS_MIX_CHAR_NUM == "Y")
            {
                int intNum = 0, intUCase = 0, intLCase = 0, intSpec = 0;
                foreach (char c in model.USER_PD)
                {
                    string s = c.ToString();
                    intNum = Regex.IsMatch(s, @"[0-9]") || intNum == 1 ? 1 : 0;
                    intUCase = Regex.IsMatch(s, @"[A-Z]") || intUCase == 1 ? 1 : 0;
                    intLCase = Regex.IsMatch(s, @"[a-z]") || intLCase == 1 ? 1 : 0;
                    intSpec = !Regex.IsMatch(s, @"[A-Za-z0-9]") || intSpec == 1 ? 1 : 0;
                }

                int cnt = intNum + intUCase + intLCase + intSpec;
                if (cnt < 3)
                {
                    errMsgs.Add("密碼至少需符合3項(大寫英文字母、小寫英文字母、數字、特殊符號)");
                }
            }

            // 不可含空白或特殊字元，只能用文字 A-Z 或數字 1-9 組成
            if (scPolicy.PASS_NO_SPEC_CHAR == "Y")
            {
                foreach (char c in model.USER_PD)
                {
                    if (!Regex.IsMatch(c.ToString(), @"[A-Za-z0-9]"))
                    {
                        errMsgs.Add(i18N.Message.S04);
                        break;
                    }
                }
            }

            // 至少含 {0} 個特殊字元
            int atLeastSpecialChars = 0;
            if (int.TryParse(scPolicy.PASS_AT_LEAST_SPECIAL_CHARS, out atLeastSpecialChars) && atLeastSpecialChars > 0)
            {
                int cnt = 0;
                foreach (char c in model.USER_PD)
                {
                    if (!Regex.IsMatch(c.ToString(), @"[A-Za-z0-9]"))
                    {
                        cnt++;
                    }
                }

                if (cnt < atLeastSpecialChars)
                {
                    errMsgs.Add($"至少含 {atLeastSpecialChars} 個特殊字元");
                }
            }

            // 相鄰二字元不可相同
            if (scPolicy.PASS_NO_SAME_2 == "Y")
            {
                string laststr = string.Empty;
                bool checkResult = true;
                foreach (char c in model.USER_PD)
                {
                    string s = c.ToString();
                    checkResult = !(laststr == s);
                    laststr = s;
                }

                if (!checkResult)
                {
                    errMsgs.Add(i18N.Message.S07);
                }
            }

            // 相鄰三字元不可為連續升冪或降冪
            if (scPolicy.PASS_NO_CONT_3 == "Y" && model.USER_PD.Length > 2)
            {
                bool checkResult = true;
                for (int i = 0; i < model.USER_PD.Length - 2; i++)
                {
                    int p = Convert.ToInt32(model.USER_PD[i]);
                    int p1 = Convert.ToInt32(model.USER_PD[i + 1]);
                    int p2 = Convert.ToInt32(model.USER_PD[i + 2]);
                    checkResult = !(p1 == p + 1 && p2 == p1 + 1);
                }

                if (!checkResult)
                {
                    errMsgs.Add(i18N.Message.S08);
                }
            }

            // 不可與最近 {0} 次內重複
            if (scPolicy.PASS_NO_SAME_PAST_TIMES > 0)
            {
                List<SCPswdHismModel> pswdHishs = (await scUserDac.GetPswdHishs(model.USER_ID, scPolicy.PASS_NO_SAME_PAST_TIMES)).ToList();
                if (pswdHishs.Where(x => x.PASSWORD == model.USER_PD).Any())
                {
                    errMsgs.Add($"不可與最近 {scPolicy.PASS_NO_SAME_PAST_TIMES} 次內重複");
                }
            }

            return errMsgs;
        }

        /// <summary>
        /// 忘記密碼
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> ForgotPW(SCQuestionModel model)
        {
            // 檢查資訊
            bool check = await scUserDac.CheckExists(model.USER_ID, model.USER_NAME, model.USER_EMAIL);
            if (!check)
            {
                return ChangeResult(false, "使用者資訊不正確");
            }

            // 更新密碼，密碼為亂數
            string userPd = randomNum.CreateRandomEnNum(8);
            await scUserDac.UpdatePassword(model.USER_ID, userPd, "Y");

            // 寄信
            MailSetModel mailSet = new MailSetModel()
            {
                MAIL_SUBJECT = "密碼重置通知",
                MAIL_CONTENT = $"您新的登入密碼為：{userPd} <br/> 重新登入"
            };

            List<RecipientModel> rcvList = new List<RecipientModel>
            {
                new RecipientModel { MAIL_ADDRESS = model.USER_EMAIL, MAIL_TITLE = model.USER_NAME }
            };

            bool isSend = await mail.Send(mailSet, rcvList);
            if (!isSend)
            {
                return ChangeResult(false, "系統異常");
            }

            return ChangeResult(true, "密碼重置成功，請至您的電子郵件信箱收取新的登入資訊");
        }
    }
}
