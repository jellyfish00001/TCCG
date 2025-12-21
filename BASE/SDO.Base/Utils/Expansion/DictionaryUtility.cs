using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Base.Utils.Expansion
{
    public static class DictionaryUtility
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            TValue result;
            dict.TryGetValue(key, out result);
            return result;
        }
        public static T ToAnonymousType<T, TValue>(this IDictionary<string, TValue> dict, T anonymousPrototype)
        {
            // get the sole constructor
            var ctor = anonymousPrototype.GetType().GetConstructors().Single();

            // conveniently named constructor parameters make this all possible...
            var args = from p in ctor.GetParameters()
                       let val = dict.GetValueOrDefault(p.Name)
                       select val != null && p.ParameterType.IsAssignableFrom(val.GetType()) ? (object)val : null;

            return (T)ctor.Invoke(args.ToArray());
        }
    }
}
