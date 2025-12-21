using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SDO.Models
{
    public static class ParseUtil
    {
        public static Tout Parse<Tin, Tout>(Tin source) where Tout : new()
        {
            //建立實體
            Tout dest = Activator.CreateInstance<Tout>();
            //取得屬性
            IEnumerable<PropertyInfo> props = typeof(Tout).GetProperties().Where(prop => prop.CanWrite);

            //綁定對應屬性值
            foreach (PropertyInfo prop in props)
            {
                PropertyInfo p = typeof(Tin).GetProperty(prop.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (p != null && p.CanRead)
                {
                    object val = p.GetValue(source);
                    prop.SetValue(dest, val);
                }
            }

            return dest;
        }

        public static void Parse<Tin, Tout>(Tin source, out Tout destination) where Tout : new()
        {
            destination = Parse<Tin, Tout>(source);
        }


        public static bool TryParse<Tin, Tout>(Tin source, out Tout destination) where Tout : new()
        {
            try
            {
                Parse(source, out destination);

                return true;
            }
            catch (Exception)
            {
                destination = default;
                return false;
            }
        }
        /// <summary>
        /// 判斷是否為Json
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        public static bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return false; }
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 檢查Dictionary是否有該Key且值不為null
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static bool CheckDictItemExistedAndNotEmpty(this Dictionary<string, object> dict, string Key)
        {
            return dict.ContainsKey(Key) && dict[Key] != null && !string.IsNullOrEmpty(dict[Key].ToString());
        }
    }
}