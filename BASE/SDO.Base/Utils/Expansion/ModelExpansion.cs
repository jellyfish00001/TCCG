using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SDO.Utils
{
    public static class ModelExpansion
    {
        /// <summary>
        /// ModelMapper 深拷
        /// </summary>
        /// <typeparam name="TL"></typeparam>
        /// <typeparam name="TR"></typeparam>
        /// <param name="Source">來源</param>
        /// <param name="Dest">輸出</param>
        /// <returns></returns>
        public static TR ModelMapper<TL, TR>(TL Source, TR Dest)
        {
            if (Source == null)
                return Dest;

            var MapConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TL, TR>();
            });
            IMapper Mapper = MapConfig.CreateMapper();
            return Mapper.Map<TL, TR>(Source);
        }

        /// <summary>
        /// HtmlEncode 所有字串欄位
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">Model</param>
        /// <param name="IsFileResult">Model內參數是否包含路徑</param>
        /// <returns></returns>
        public static T EncodeModel<T>(T model, bool IsFileResult = false)
        {
            Type tl = typeof(T);
            var result = Activator.CreateInstance<T>();
            if (model == null)
            {
                model = Activator.CreateInstance<T>();
            }

            //t1抓可讀可寫的屬性
            var propertiesTL = tl.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var prop in propertiesTL)
            {
                var value = prop.GetValue(model, null);
                if (value == null)
                    continue;
                //string 才encode
                if (prop.PropertyType == typeof(string))
                {
                    string Sval = HttpUtility.HtmlEncode(value.ToString());
                    Sval = IsFileResult ? Sval.Replace("..", "") : Sval;
                    prop.SetValue(result, Sval, null);
                }
                else
                    //其它直接設定值就好....
                    prop.SetValue(result, value, null);
            }
            return result;
        }
        /// <summary>
        /// 物件轉Dictionary
        /// </summary>
        /// <param name="o">需轉換物件</param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDictionary(this object o)
        {
            Dictionary<string, object> Dict = new Dictionary<string, object>();
            var props = o.GetType().GetProperties().Where(x=>x.CanRead||x.CanWrite);
            foreach (var prop in props)
            {
                Dict.Add(prop.Name, prop.GetValue(o));
            }
            return Dict;
        }

        /// <summary>
        /// Distinct By Specific Key
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
        
    }
}
