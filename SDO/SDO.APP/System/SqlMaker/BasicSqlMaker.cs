using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Reflection;
using System.IO;

namespace SDO.SqlMaker
{
    public class BasicSqlMaker : IBasicSqlMaker
    {
        private string dataBaseName;

        private class Filter
        {
            public string Name { get; set; }

            public bool IsArray { get; set; }
        }

        public string SetDataBaseName(string name) => dataBaseName = name;
        public string GetDataBaseName() => dataBaseName;

        /// <summary>
        /// 產出簡易Select語法
        /// 使用說明:
        ///     1.請先設定dataBaseName。  使用SetDataBaseName(string name)
        ///     2.以Model內的可寫屬性為讀取欄位
        ///     3.以Model內的可讀欄位為條件欄位
        ///     4.各條件之間的邏輯為AND
        ///     5.屬性若不為ValueType則不列入讀取或條件欄位
        ///     6.可讀欄位不為null才加入條件欄位。 (請注意預設值不為null的欄位)
        ///     7.條件欄位支援valueType的array及IList。 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public string Read<T>(T model)
        {
            StringBuilder stringBuilder = new StringBuilder();
            IList<string> propList;
            IList<Filter> filterList;
            stringBuilder.AppendLine("SELECT");

            PropertyInfo[] props = model.GetType().GetProperties();

            propList = GetPropList(props);

            foreach (string propName in propList)
                stringBuilder.AppendLine($"[{propName}]" + (propList.Last() == propName ? "" : ","));

            stringBuilder.AppendLine($"FROM dbo.{dataBaseName} (nolock) ");

            filterList = GetFilterList(props, model);

            foreach(Filter filter in filterList)
            {
                if(filterList.First() == filter)
                {
                    stringBuilder.AppendLine("WHERE");
                    stringBuilder.AppendLine(GetWhereString(filter));
                }
                else
                {
                    stringBuilder.AppendLine($"AND {GetWhereString(filter)}");
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public string Insert<T>(T model)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"INSERT INTO dbo.{dataBaseName}");
            stringBuilder.AppendLine("(");

            PropertyInfo[] props = model.GetType().GetProperties();

            IList<string> propList = GetPropList(props);

            foreach (string propName in propList)
                stringBuilder.AppendLine($"[{propName}]" + (propList.Last() == propName ? "" : ","));
            stringBuilder.AppendLine(")");

            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine("(");

            foreach (string propName in propList)
                stringBuilder.AppendLine($"?{propName}?" + (propList.Last() == propName ? "" : ","));

            stringBuilder.AppendLine(")");

            return stringBuilder.ToString();
        }

        public string Delete<T>(T model)
        {
            StringBuilder stringBuilder = new StringBuilder();
            IList<Filter> filterList;

            stringBuilder.AppendLine($"DELETE FROM dbo.{dataBaseName}");

            PropertyInfo[] props = model.GetType().GetProperties();
            filterList = GetFilterList(props, model);
            if(!filterList.Any())
                return "";//不允許無條件刪除

            foreach (Filter filter in filterList)
            {
                if (filterList.First() == filter)
                {
                    stringBuilder.AppendLine("WHERE");
                    stringBuilder.AppendLine(GetWhereString(filter));
                }
                else
                {
                    stringBuilder.AppendLine($"AND {GetWhereString(filter)}");
                }
            }

            return stringBuilder.ToString();
        }

        private string GetWhereString(Filter filter)
        {
            if (filter.IsArray)
                return $"{filter.Name} IN ?{filter.Name}?";
            return $"{filter.Name} = ?{filter.Name}?";
        }

        private IList<string> GetPropList(PropertyInfo[] props)
        {
            IList<string> propList = new List<string>();
            foreach (PropertyInfo prop in props)
            {
                if (!prop.CanWrite)//過濾不可寫
                    continue;

                Type propType = prop.PropertyType;

                if (!propType.IsValueType && propType != typeof(string))//過濾非ValueType
                    continue;

                propList.Add(prop.Name);
            }

            return propList;
        }

        private IList<Filter> GetFilterList<T>(PropertyInfo[] props, T model)
        {
            IList<Filter> filterList = new List<Filter>();
            foreach (PropertyInfo filter in props)
            {
                Type filterType = filter.PropertyType;

                if (!filter.CanRead)//過濾不可寫
                    continue;

                if (filter.GetValue(model) == null)//過濾NULL
                    continue;

                if (filter.GetValue(model) is IEnumerable)//是否為Array ILIST
                {
                    filterList.Add(new Filter()
                    {
                        Name = filter.Name,
                        IsArray = true
                    });
                }
                else
                {
                    if (!filterType.IsValueType && filterType != typeof(string))//過濾非ValueType
                        continue;

                    filterList.Add(new Filter()
                    {
                        Name = filter.Name,
                        IsArray = false
                    });
                    continue;
                }
            }

            return filterList;
        }
    }
}
