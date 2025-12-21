using SDO.Dac;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Mail;
using System.Web;
using System.Reflection;
using SDO.CryptSet;
using System.IO;
using System.ComponentModel;

namespace SDO.Services
{
    public class MailSetService : Service, IMailSetService
    {
        private readonly IMailSetDac dac;
        private readonly IUserProfile userProfile;
        private readonly IMailLogService mailLog;
        private readonly ISetParamService setParam;
        private SMTPSettingModel SmtpSet;
        private List<SetParamModel> MailSetting;
        private readonly IEncryptService encrypt;
        /// <summary>
        /// 完整資訊
        /// </summary>
        /// <param name="dac"></param>
        /// <param name="userProfile"></param>
        /// <param name="setParam"></param>
        /// <param name="mailLog"></param>
        /// <param name="encrypt"></param>
        public MailSetService(IMailSetDac dac, IUserProfile userProfile, ISetParamService setParam, IMailLogService mailLog, IEncryptService encrypt)
        {
            this.dac = dac;
            this.userProfile = userProfile;
            this.setParam = setParam;
            this.mailLog = mailLog;
            GetSmtpSetting();
            this.encrypt = encrypt;
        }
        /// <summary>
        /// 不需要個人訊息
        /// </summary>
        /// <param name="dac"></param>
        /// <param name="setParam"></param>
        /// <param name="mailLog"></param>
        /// <param name="encrypt"></param>
        public MailSetService(IMailSetDac dac, ISetParamService setParam, IMailLogService mailLog, IEncryptService encrypt)
        {
            this.dac = dac;
            this.setParam = setParam;
            this.mailLog = mailLog;
            GetSmtpSetting();
            this.encrypt = encrypt;
        }
        /// <summary>
        /// 不需要加密
        /// </summary>
        /// <param name="dac"></param>
        /// <param name="setParam"></param>
        /// <param name="mailLog"></param>
        public MailSetService(IMailSetDac dac, ISetParamService setParam, IMailLogService mailLog)
        {
            this.dac = dac;
            this.setParam = setParam;
            this.mailLog = mailLog;
            GetSmtpSetting();
        }
        /// <summary>
        /// 取得smtp設定
        /// </summary>
        private async void GetSmtpSetting()
        {
            //抓出設定檔
            MailSetting = (await setParam.GetSysParams("MailConfig")).ToList();
            var smtpServer = MailSetting.Where(x => x.SET_TYPE == "MailServer").Select(x => x.SET_VALUE).FirstOrDefault();
            int port = Convert.ToInt32(MailSetting.Where(x => x.SET_TYPE == "MailPort").Select(x => x.SET_VALUE).FirstOrDefault());
            var acc = MailSetting.Where(x => x.SET_TYPE == "MailId").Select(x => x.SET_VALUE).FirstOrDefault();
            var PD = MailSetting.Where(x => x.SET_TYPE == "MailPassword").Select(x => x.SET_VALUE).FirstOrDefault();
            var sender = MailSetting.Where(x => x.SET_TYPE == "SenderMail").Select(x => x.SET_VALUE).FirstOrDefault();
            var SenderTitle = MailSetting.Where(x => x.SET_TYPE == "SenderTitle").Select(x => x.SET_VALUE).FirstOrDefault();
            SmtpSet = new SMTPSettingModel
            {
                serverAddr = smtpServer,
                Port = port,
                Account = acc,
                PD = PD,
                Sender = sender,
                SenderName = SenderTitle
            };
        }
        /// <summary>
        /// 取得所有郵件範本
        /// </summary>
        /// <returns></returns>
        public async Task<IList<MailSetModel>> Read()
        {
            string userId = userProfile.GetLoginUser().USER_ID;
            return await dac.Read(userId);
        }
        /// <summary>
        /// 查詢郵件範本信息
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        public async Task<MailSetModel> ReadById(string mailId)
        {
            return await dac.ReadById(mailId);
        }
        /// <summary>
        /// 查詢郵件範本信息(有回傳model)
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        public MailSetModel ReadByIdSync(string mailId)
        {
            return dac.ReadByIdSync(mailId);
        }
        /// <summary>
        /// 查詢郵件範本收件人
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        public async Task<IList<RecipientModel>> ReadRecipientById(string mailId)
        {
            return await dac.ReadRecipientById(mailId);
        }
        /// <summary>
        /// 新增郵件範本
        /// </summary>
        /// <param name="mail"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> Create(MailSetMdfModel mail)
        {
            if (await IsExists(mail.MAIL_ID))
                return ChangeResult(ResultType.Fail | ResultType.Insert, mail.MAIL_ID);

            mail.CRT_USER = userProfile.GetLoginUser().USER_ID;
            mail.MDF_USER = userProfile.GetLoginUser().USER_ID;
            mail.MAP_ROLE_USER = userProfile.GetLoginUser().USER_ID;
            await dac.Insert(mail);
            return ChangeResult(ResultType.Success | ResultType.Insert);
        }
        /// <summary>
        /// 更新郵件範本
        /// </summary>
        /// <param name="mail"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> Update(MailSetMdfModel mail)
        {

            mail.CRT_USER = userProfile.GetLoginUser().USER_ID;
            mail.MDF_USER = userProfile.GetLoginUser().USER_ID;
            await dac.Update(mail);
            return ChangeResult(ResultType.Success | ResultType.Update);
        }
        /// <summary>
        /// 刪除郵件範本
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        public async Task<RtnResultModel> Delete(string mailId)
        {
            dac.BeginTransaction();
            TaskQueue tasks = new TaskQueue();
            tasks.AddTask(dac.Delete(mailId));
            tasks.AddTask(DeleteRecipient(mailId));
            await tasks.Done();
            dac.Commit();
            return ChangeResult(ResultType.Success | ResultType.Delete);
        }
        /// <summary>
        /// 更新寄，收件者資料
        /// </summary>
        /// <param name="updateRecipient"></param>
        /// <returns></returns>
        public async Task<IRtnResult> UpdateRecipient(RecipientMdfModel updateRecipient)
        {
            if (updateRecipient.RECIPIENT != null && updateRecipient.RECIPIENT.Any())
                Parallel.ForEach(updateRecipient.RECIPIENT, recipient =>
                {
                    recipient.MAIL_ID = updateRecipient.MAIL_ID;
                    recipient.CRT_USER = userProfile.GetLoginUser().USER_ID;
                    recipient.MDF_USER = userProfile.GetLoginUser().USER_ID;
                });

            dac.BeginTransaction();
            TaskQueue tasks = new TaskQueue();
            tasks.AddTask(DeleteRecipient(updateRecipient.MAIL_ID));
            tasks.AddTask(InsertRecipient(updateRecipient.RECIPIENT));
            await tasks.Done();
            dac.Commit();
            return ChangeResult(ResultType.Success | ResultType.Update);
        }
        /// <summary>
        /// 送信
        /// </summary>
        /// <param name="mail">email</param>
        /// <param name="RcvList">收件者</param>
        /// <param name="attachments">附件</param>
        /// <returns></returns>
        public async Task<bool> Send(MailSetModel mail, IList<RecipientModel> RcvList, List<Attachment> attachments = null)
        {
            //是否寄給公司收件信箱
            var sendToOneMail = MailSetting.FirstOrDefault(x => x.SET_TYPE == "SendToOneMail")?.SET_VALUE;
            var commonMail = MailSetting.FirstOrDefault(x => x.SET_TYPE == "Common_Mail");

            //取得smtp相關設定
            //login smpt
            SmtpClient smtpClient = new SmtpClient(SmtpSet.serverAddr, SmtpSet.Port);
            if (!smtpClient.UseDefaultCredentials)
                if(!string.IsNullOrEmpty(SmtpSet.Account) && !string.IsNullOrEmpty(SmtpSet.PD))
                smtpClient.Credentials = new System.Net.NetworkCredential(SmtpSet.Account, SmtpSet.PD);
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(SmtpSet.Sender, SmtpSet.SenderName);
            msg.Subject = HttpUtility.HtmlDecode(mail.MAIL_SUBJECT).Replace("\r\n", " ").Replace("\n", " ");
            msg.Body = HttpUtility.HtmlDecode(mail.MAIL_CONTENT).Replace("\r\n", "<br>").Replace("\n", "<br>"); ;
            msg.IsBodyHtml = true;

            // 夾帶附件
            if (attachments != null && attachments.Any())
            {
                foreach (Attachment attachment in attachments)
                {
                    msg.Attachments.Add(attachment);
                }
            }

            //設定log
            MailLogModel log = new MailLogModel
            {
                MAIL_SENDER = SmtpSet.serverAddr,
                MAIL_CONTENT = HttpUtility.HtmlDecode(mail.MAIL_CONTENT),
                MAIL_SUBJECT = HttpUtility.HtmlDecode(mail.MAIL_SUBJECT),
                MAIL_ID = mail.MAIL_ID??"",
                PROJECT_NO = mail.PROJECT_NO??""
            };

            // 一般收件人
            List<RecipientModel> commonRcvList = RcvList.Where(x => x.MAIL_TYPE == null || x.MAIL_TYPE == "1").ToList()??new();

            if (sendToOneMail == "Y")
            {
                //當此設定為Y，統一寄給公司收件信箱
                msg.To.Add(new MailAddress(commonMail.SET_VALUE, commonMail.MEMO));
                log.MAIL_RECEIVER += commonMail.SET_VALUE + ";";
                log.CC_RECEIVER = string.Empty;
                log.BCC_RECEIVER = string.Empty;
            }
            else
            {
                //自己傳的信箱
                // 一般收件人
                if (commonRcvList.Any())
                {
                    foreach (var rcv in commonRcvList)
                    {
                        msg.To.Add(new MailAddress(rcv.MAIL_ADDRESS, rcv.MAIL_TITLE));
                        log.MAIL_RECEIVER += rcv.MAIL_ADDRESS + ";";
                    }
                }
                else
                {
                    log.MAIL_RECEIVER = string.Empty;
                }

                // 副本收件者
                var ccRcvList = RcvList.Where(x => x.MAIL_TYPE == "2").ToList();
                if (ccRcvList.Any())
                {
                    foreach (var ccRcv in ccRcvList)
                    {
                        msg.CC.Add(new MailAddress(ccRcv.MAIL_ADDRESS, ccRcv.MAIL_TITLE));
                        log.CC_RECEIVER += $"{ccRcv.MAIL_ADDRESS};";
                    }
                    log.BCC_RECEIVER = string.Empty;
                }
                else
                {
                    log.CC_RECEIVER = string.Empty;
                    log.BCC_RECEIVER = string.Empty;
                }
            }

            //送信並記錄結果
            try
            {
                smtpClient.Send(msg);
                Thread.Sleep(1000);
                log.SEND_FLG = true;
                await mailLog.SetMailLog(log);
                return true;
            }
            catch (Exception ex)
            {
                log.SEND_FLG = false;
                await mailLog.SetMailLog(log);
                return false;
            }
        }
        /// <summary>
        /// 送信
        /// </summary>
        /// <param name="mail">email</param>
        /// <param name="rcvList">收件者</param>
        /// <param name="attachments">附件</param>
        /// <returns></returns>
        protected bool SendSync(MailSetModel mail, IList<RecipientModel> rcvList, List<Attachment> attachments = null)
        {
            //是否寄給公司收件信箱
            var sendToOneMail = MailSetting.FirstOrDefault(x => x.SET_TYPE == "SendToOneMail")?.SET_VALUE;
            var commonMail = MailSetting.FirstOrDefault(x => x.SET_TYPE == "Common_Mail");

            //取得smtp相關設定
            //login smpt
            SmtpClient smtpClient = new SmtpClient(SmtpSet.serverAddr, SmtpSet.Port);
            if (!smtpClient.UseDefaultCredentials)
                smtpClient.Credentials = new System.Net.NetworkCredential(SmtpSet.Account, SmtpSet.PD);
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(SmtpSet.Sender, SmtpSet.SenderName);
            msg.Subject = HttpUtility.HtmlDecode(mail.MAIL_SUBJECT).Replace("\r\n", " ").Replace("\n", " "); ;
            msg.Body = HttpUtility.HtmlDecode(mail.MAIL_CONTENT).Replace("\r\n", "<br>").Replace("\n", "<br>"); ;
            msg.IsBodyHtml = true;

            // 夾帶附件
            if(attachments!=null && attachments.Any())
            {
                foreach(Attachment attachment in attachments)
                {
                    msg.Attachments.Add(attachment);
                }
            }

            //設定log
            MailLogModel log = new MailLogModel
            {
                MAIL_SENDER = SmtpSet.serverAddr,
                MAIL_CONTENT = HttpUtility.HtmlDecode(mail.MAIL_CONTENT),
                MAIL_SUBJECT = HttpUtility.HtmlDecode(mail.MAIL_SUBJECT)
            };

            if (sendToOneMail == "Y")
            {
                //當此設定為Y，統一寄給公司收件信箱
                msg.To.Add(new MailAddress(commonMail.SET_VALUE, commonMail.MEMO));
                log.MAIL_RECEIVER += commonMail.SET_VALUE + ";";
            }
            else
            {
                //自己傳的信箱
                foreach (var rcv in rcvList)
                {
                    msg.To.Add(new MailAddress(rcv.MAIL_ADDRESS, rcv.MAIL_TITLE));
                    log.MAIL_RECEIVER += rcv.MAIL_ADDRESS + ";";
                }
            }

            log.CC_RECEIVER = string.Empty;
            log.BCC_RECEIVER = string.Empty;

            //送信並記錄結果
            try
            {
                smtpClient.Send(msg);
                Thread.Sleep(1000);
                log.SEND_FLG = true;
                mailLog.SetMailLogSync(log);
                return true;
            }
            catch (Exception ex)
            {
                log.SEND_FLG = false;
                mailLog.SetMailLogSync(log);
                return false;
            }
        }
        /// <summary>
        /// 檢查郵件範本存在
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private async Task<bool> IsExists(string mailId)
        {
            if (string.IsNullOrWhiteSpace(mailId))
                throw new ArgumentNullException("MAIL_ID");
            return await dac.ReadById(mailId) != default(MailSetModel);
        }
        /// <summary>
        /// 删除收件人信息
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        private async Task DeleteRecipient(string mailId)
        {
            await dac.DeleteRecipient(mailId);
        }
        /// <summary>
        /// 插入收件人信息
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private async Task InsertRecipient(IList<RecipientModel> models)
        {
            await dac.InsertRecipient(models);
        }
        /// <summary>
        /// 利用範本發送信件(異步)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="templateId">範本的ID</param>
        /// <param name="mailAddrs">Mail清單</param>
        /// <param name="templatePara">帶有範本參數所需屬性的物件</param>
        /// <returns></returns>
        public async Task<bool> SetTemplateSend<T>(MailTemplateSendModel<T> model)
        {
            MailSetModel mailTemplate = await ReadById(model.TemplateId);
            mailTemplate = ReplaceTemplateData(mailTemplate, model.TemplatePara);
            // 送信
            var isSend = await Send(mailTemplate, model.MailAddrs,model.Attachments);

            return isSend;
        }
        /// <summary>
        /// 利用範本發送信件(同步)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool SetTemplateSendSync<T>(MailTemplateSendModel<T> model)
        {
            MailSetModel template = ReadByIdSync(model.TemplateId); //取得信件範本
            template = ReplaceTemplateData(template, model.TemplatePara);
            // 送信
            var isSend = SendSync(template, model.MailAddrs,model.Attachments);

            return isSend;
        }
        /// <summary>
        /// 替換郵件範本參數
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mailTemplate">郵件範本</param>
        /// <param name="templatePara">替換參數資料</param>
        /// <returns></returns>
        private static MailSetModel ReplaceTemplateData<T>(MailSetModel mailTemplate, T templatePara)
        {
            PropertyInfo[] props = templatePara.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                // 取得客製屬性 DisplayName
                var attr = (DisplayNameAttribute)prop.GetCustomAttributes(typeof(DisplayNameAttribute), true).SingleOrDefault();
                string replaceText;
                if (attr != null)
                    replaceText = attr.DisplayName;
                else // 預設使用物件名稱
                    replaceText = prop.Name;

                string value = prop.GetValue(templatePara)?.ToString() ?? "";
                if (prop.Name == "PROJECT_NO")
                    mailTemplate.PROJECT_NO = value;

                mailTemplate.MAIL_CONTENT = mailTemplate.MAIL_CONTENT.Replace($"${replaceText}$", value);
                mailTemplate.MAIL_SUBJECT = mailTemplate.MAIL_SUBJECT.Replace($"${replaceText}$", value);
            }

            return mailTemplate;
        }
    }
}