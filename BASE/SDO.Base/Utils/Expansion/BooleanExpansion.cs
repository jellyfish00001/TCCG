using System;
using System.Threading.Tasks;

namespace SDO.Utils
{
    public static class BooleanExpansion
    {
        public static bool IsTrue(this bool flag, Action action)
        {
            if(flag)
                action.Invoke();
            return flag;
        }

        public static async Task<bool> IsTrue(this bool flag, Func<Task> action)
        {
            if (flag)
                await action.Invoke();
            return flag;
        }

        public static async Task<bool> IsTrue(this Task<bool> flag, Func<Task> action)
        {
            if (await flag)
                await action.Invoke();
            return await flag;
        }

        public static bool IsFalse(this bool flag, Action action)
        {
            if (!flag)
                action.Invoke();
            return flag;
        }

        public static async Task<bool> IsFalse(this bool flag, Func<Task> action)
        {
            if (!flag)
                await action.Invoke();
            return flag;
        }

        public static async Task<bool> IsFalse(this Task<bool> flag, Func<Task> action)
        {
            if (!await flag)
                await action.Invoke();
            return await flag;
        }
    }
}
