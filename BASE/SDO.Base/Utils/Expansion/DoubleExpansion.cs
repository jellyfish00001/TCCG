using System;
using System.Globalization;
using System.Threading.Tasks;

namespace SDO.Utils
{
    public static class DoubleExpansion
    {
        public static double Round(this double oriValue ,int precision=2)
        {
            if (Double.IsNaN(oriValue) || Double.IsInfinity(oriValue))
                return 0;
            return Convert.ToDouble(Math.Round(Convert.ToDecimal(oriValue), precision, MidpointRounding.AwayFromZero));
        }
    }
}
