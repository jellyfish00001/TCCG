using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Utils
{
    public interface ISecureRandomNum
    {
        /// <summary>
        /// 產生一個非負數的亂數
        /// </summary>
        int Next();
        /// <summary>
        /// 產生一個大於等於零且小於等於max的整數亂數
        /// </summary>
        /// <param name="max">最大值</param>
        int Next(int max);
        /// <summary>
        /// 產生一個大於等於min且小於等於max的整數亂數
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        int Next(int min, int max);
        /// <summary>
        /// 產生一個大於0且小於1的Double亂數
        /// </summary>
        /// <returns></returns>
        double NextDouble();
        /// <summary>
        /// 產生一個大於0且小於 max 的 Double 亂數
        /// </summary>
        /// <param name="max">最大值</param>
        double NextDouble(int max);
        /// <summary>
        /// 產生一個大於 min 且小於 max 的 Double 亂數
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        double NextDouble(int min, int max);
        /// <summary>
        /// 建立亂數-數字
        /// </summary>
        /// <param name="numCount">數量</param>
        /// <returns></returns>
        string CreateRandomNum(int numCount);
        /// <summary>
        /// 建立亂數-英數字
        /// </summary>
        /// <param name="numCount">數量</param>
        /// <returns></returns>
        string CreateRandomEnNum(int numCount);
    }
}
