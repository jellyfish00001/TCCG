using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Security.Cryptography;

namespace SDO.Utils
{
    /// <summary>
    /// Number 相關擴充方法
    /// </summary>
    /// <remarks>
    /// </remarks>
    public static class NumberExpansion
    {
        /// <summary>
        /// 將阿拉伯數字轉為國字數字
        /// </summary>
        /// <param name="number"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToChineseNumber(this int number, string format = "")
        {
            return ToChineseNumber(number.ToString(), format);
        }

        /// <summary>
        /// 將阿拉伯數字轉為國字數字
        /// </summary>
        /// <param name="number">阿拉伯數字字串</param>
        /// <param name="format">欲轉換的格式</param>
        /// <remarks>
        /// 1. 目前 format 包含下列格式，可持續擴充
        ///     A: 壹、貳...
        ///     預設: 一、二...
        /// </remarks>
        /// <returns></returns>
        public static string ToChineseNumber(this string number, string format = "")
        {
            // 回傳值
            string cNum = string.Empty;

            // 依格式設定單位及數字
            if (!string.IsNullOrEmpty(number))
            {
                // 國字單位
                List<string> cUnit;

                // 國字數字
                List<string> cDigit;

                string[] test = { "仟", "佰", "拾", "千", "百", "十" };

                switch (format)
                {
                    case "A":
                        cUnit = new List<string> { "", "拾", "佰", "仟", "萬", "拾", "佰", "仟", "億", "拾", "佰", "仟", "兆", "拾", "佰", "仟" };
                        cDigit = new List<string> { "壹", "貳", "參", "肆", "伍", "陸", "柒", "捌", "玖" };
                        break;
                    default:
                        cUnit = new List<string> { "", "十", "百", "千", "萬", "十", "百", "千", "億", "十", "百", "千", "兆", "十", "百", "千" };
                        cDigit = new List<string> { "一", "二", "三", "四", "五", "六", "七", "八", "九" };
                        break;
                }

                // 逐一數字及單位轉換
                for (int i = 0; i < number.Length; i++)
                {
                    // 由右至左取得單一數字
                    string currentNum = number.Substring(number.Length - i - 1, 1);

                    // 取得單位
                    string unit = cUnit[i];

                    // 若數字不是 0 對應國字，若是 0 則串零
                    if (!currentNum.Equals("0"))
                    {
                        int numPos = Convert.ToInt32(currentNum) - 1;
                        cNum = string.Format("{0}{1}{2}", cDigit[numPos], unit, cNum);
                    }
                    else
                    {
                        if (test.Contains(unit) && !string.IsNullOrEmpty(cNum))
                        {
                            cNum = string.Format("零{0}", cNum);
                        }
                        else
                        {
                            cNum = string.Format("{0}{1}", cNum, unit);
                        }
                    }
                }

                // 去除多餘的零
                cNum = cNum.Replace("零零零", "零");
                cNum = cNum.Replace("零零", "零");
                cNum = cNum.Replace("零兆", "兆");
                cNum = cNum.Replace("零億", "億");
                cNum = cNum.Replace("零萬", "萬");
                cNum = cNum.Replace("兆億萬", "兆");
                cNum = cNum.Replace("兆億", "兆");
                cNum = cNum.Replace("億萬", "億");

                // 若為 "一十" 開頭，則將開頭的 "一" 去除
                if (cNum.StartsWith("一十"))
                {
                    cNum = cNum.Substring(1, cNum.Length - 1);
                }
            }

            return cNum;
        }

        /// <summary>
        /// 數字轉英文字母
        /// 1 => A
        /// 2 => B
        /// 3 => C....
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string ToEnglishAlph(this int number)
        {
            if (number > 0 && number < 27)
            {
                string[] englishAlphs = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Select(x => x.ToString()).ToArray();
                return englishAlphs[number - 1];
            }
            return "";
        }
        /// <summary>
        /// 將值取到小數特定位數
        /// </summary>
        /// <param name="value"></param>
        /// <param name="roundTo">第幾位</param>
        /// <returns></returns>
        public static decimal GetRound(this decimal value, int roundTo = 2)
        {
            return Math.Round(value, roundTo, MidpointRounding.AwayFromZero);
        }
    }
}