using System;
using System.Security.Cryptography;
namespace SDO.Utils
{
    public class SecureRandomNum: ISecureRandomNum
    {
        private static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(Guid.NewGuid().GetHashCode().ToString());

        /// <summary>
        /// 產生一個非負數的亂數
        /// </summary>
        public  int Next()
        {
            byte[] bytes = new byte[4];
            rng.GetBytes(bytes);
            int value = BitConverter.ToInt32(bytes, 0);
            if (value < 0) value = -value;
            return value;
        }

        /// <summary>
        /// 產生一個大於等於零且小於等於max的整數亂數
        /// </summary>
        /// <param name="max">最大值</param>
        public  int Next(int max)
        {
            byte[] bytes = new byte[4];
            rng.GetBytes(bytes);
            int value = BitConverter.ToInt32(bytes, 0);
            value = value % (max - 1);
            if (value < 0) value = -value;
            return value;
        }

        /// <summary>
        /// 產生一個大於等於min且小於等於max的整數亂數
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        public  int Next(int min, int max)
        {
            int value = Next(max - min) + min;
            return value;
        }

        /// <summary>
        /// 產生一個大於0且小於1的Double亂數
        /// </summary>
        /// <returns></returns>
        public  double NextDouble()
        {
            var bytes = new byte[8];
            rng.GetBytes(bytes);
            var ul = BitConverter.ToUInt64(bytes, 0) / (1 << 11);
            double d = ul / (double)(1UL << 53);
            return d;
        }

        /// <summary>
        /// 產生一個大於0且小於 max 的 Double 亂數
        /// </summary>
        /// <param name="max">最大值</param>
        public  double NextDouble(int max)
        {
            return NextDouble() * max;
        }

        /// <summary>
        /// 產生一個大於 min 且小於 max 的 Double 亂數
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        public  double NextDouble(int min, int max)
        {
            return NextDouble(max - min) + min;
        }

        /// <summary>
        /// 建立亂數-數字
        /// </summary>
        /// <param name="numCount">數量</param>
        /// <returns></returns>
        public string CreateRandomNum(int numCount)
        {
            string allChar = "1,2,3,4,5,6,7,8,9,0";
            string[] allCharArray = allChar.Split(',');//差分成陣列
            string RndNum = "";
            int temp = -1;//記錄上次亂數值的數值，儘量避免產生幾個相同的亂數
            //Random rand = new Random();
            for (int i = 0; i < numCount; i++)
            {
                int t = Next(9);
                if (temp == t)
                {
                    return CreateRandomNum(numCount);
                }
                temp = t;
                RndNum += allCharArray[t];
            }
            return RndNum;
        }

        /// <summary>
        /// 建立亂數-英數字
        /// </summary>
        /// <param name="numCount">數量</param>
        /// <returns></returns>
        public string CreateRandomEnNum(int numCount)
        {
            string str = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            string result = string.Empty;
            int temp = -1;
            for (var i = 0; i < numCount; i++)
            {
                int t = Next(str.Length);
                while(temp == t)
                {
                    t = Next(str.Length);
                }

                temp = t;
                result += str[t];
            }
            return result;
        }
    }
}
