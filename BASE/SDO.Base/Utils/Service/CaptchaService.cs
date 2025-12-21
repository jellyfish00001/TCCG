using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using SDO.CryptSet;
using SDO.Models;
namespace SDO.Utils
{
    public class CaptchaService : ICaptcha
    {
        private readonly IEncryptService EncryptService;
        private readonly IDecryptService DecryptService;
        private readonly ISecureRandomNum randomNum;
        private readonly ISysParam sysParam;

        public CaptchaService(IEncryptService EncryptService, IDecryptService DecryptService, ISecureRandomNum randomNum, ISysParam sysParam)
        {
            this.EncryptService = EncryptService;
            this.DecryptService = DecryptService;
            this.randomNum = randomNum;
            this.sysParam = sysParam;
        }

        /// <summary>
        /// 產生驗證碼
        /// </summary>
        /// <returns>RandNum 驗證碼,byte[] Img</returns>
        public CaptchaModel GenerateValidateText()
        {
            string validateNum = randomNum.CreateRandomNum(4);
            if (validateNum == null || validateNum.Trim() == String.Empty)
                return new CaptchaModel();
            var ImgBse64 = Convert.ToBase64String(GenerateValidateImage(validateNum));
            validateNum = $"{validateNum}&{DateTimeUtil.GMT8.AddMinutes(5).ToString("yyyyMMddHHmmss")}";

            var encryptCaptcha = EncryptService.AES256(validateNum).encryptedString;
            var captcha = new CaptchaModel
            {
                CaptchaEncode = encryptCaptcha,
                Img = ImgBse64
            };
            return captcha;
        }

        private byte[] GenerateValidateImage(string validateNum)
        {
            //生成bitmap圖像
            Bitmap image = new Bitmap(validateNum.Length * 28 + 10, 46);
            Graphics g = Graphics.FromImage(image);
            try
            {
                //生成隨機生成器
                //Random random = new Random();
                g.Clear(Color.White);
                //畫圖片背景噪音線
                for (int i = 0; i < 25; i++)
                {
                    int x1 = randomNum.Next(image.Width);
                    int x2 = randomNum.Next(image.Width);
                    int y1 = randomNum.Next(image.Height);
                    int y2 = randomNum.Next(image.Height);
                    g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                }
                Font font = new Font("Arial", 28, (FontStyle.Bold | FontStyle.Italic));
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.DarkRed, 1.2f, true);
                g.DrawString(validateNum, font, brush, 2, 2);
                //畫圖片的前景噪音點
                for (int i = 0; i < 100; i++)
                {
                    int x = randomNum.Next(image.Width);
                    int y = randomNum.Next(image.Height);
                    image.SetPixel(x, y, Color.FromArgb(randomNum.Next()));
                }
                //畫圖片的邊框線
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);
                MemoryStream ms = new MemoryStream();
                //將圖像保存到指定的流
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                return (ms.ToArray());
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }

        /// <summary>
        /// 檢查驗證碼
        /// </summary>
        /// <param name="Captcha">驗證碼</param>
        /// <param name="EncodeCaptcha">加密過驗證碼</param>
        /// <returns></returns>
        public async Task<bool> CheckCaptcha(string captcha, string encodeCaptcha)
        {
            SetParamModel paramModel = await sysParam.GetSysParam("SystemConfig", "IsEnableCapcha")??new();
            string isEnableCapcha = paramModel.SET_VALUE;
            
            if (isEnableCapcha == "N")
            {
                return true;
            }
            //解析encode字串
            string decodeStr = DecryptService.AES256(encodeCaptcha).decryptedString; // 解密
            string[] decodeArray = decodeStr.Split("&");
            //如果拆字串有少代表少了時間戳記或是驗證碼
            if (decodeArray.Length != 2)
            {
                return false;
            }
            //取驗證碼
            string decodeCaptcha = decodeArray[0];
            DateTime dateTime;
            if (!DateTime.TryParseExact(decodeArray[1], "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out dateTime))
            {
                return false;
            }
            //比對有效時間
            if (dateTime < DateTimeUtil.GMT8)
            {
                return false;
            }
            //比對驗證碼
            return captcha.Equals(decodeCaptcha);
        }
    }
}