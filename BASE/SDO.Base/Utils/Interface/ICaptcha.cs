using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDO.Models;
namespace SDO.Utils
{
    public interface ICaptcha
    {
        CaptchaModel GenerateValidateText();
        Task<bool> CheckCaptcha(string captcha, string captchaEncoded);
    }
}
