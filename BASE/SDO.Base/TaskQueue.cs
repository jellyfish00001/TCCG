using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 此工具用來收集並管理無須取得回傳資料的Task
    /// 使用情境:
    ///     1.將Task 以 AddTask方法加入Queue中
    ///     2.於程式結束前執行 await Done() 確保各Task都已完成
    /// 注意事項:
    ///     1.請確認欲加入的Task是否影響後續程式執行。
    ///     2.若有一Task只需在程式結束前完成即可，可將Task加入Queue中，於程式結束前執行Done()確保其完成即可。
    ///     3.Done方法務必加上await前綴以確保Queue中的Task都執行完畢。
    /// </summary>
    public class TaskQueue
    {
        private readonly Queue<Task> tasks;

        public TaskQueue() => tasks = new Queue<Task>();

        public Queue<Task> GetQueue() => tasks;

        public int Count => tasks.Count;

        public void AddTask(Task task) => tasks.Enqueue(task);

        public async Task Done()
        {
            while (tasks.Count > 0)
            {
                await tasks.Dequeue();
            }
        }
    }
}
