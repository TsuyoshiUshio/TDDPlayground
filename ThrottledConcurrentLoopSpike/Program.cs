using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThrottledConcurrentLoopSpike
{

    class Program
    {
        static void Main(string[] args)
        {
            ExecuteAsync().GetAwaiter().GetResult();
            Console.WriteLine("Paralel task has been finished.");
            Console.ReadLine();

        }

        public static async  Task ExecuteAsync()
        {
            List<int> list = Enumerable.Range(1, 30).ToList();

            IReadOnlyList<int> col = new ReadOnlyCollection<int>(list);

            var dictionary = new ConcurrentDictionary<int, ValueTuple<int,DateTime>>();

            // 実行順序は保証できない
            await col.IndexedParallelForEachAsync(10, async (x, index) =>
            {

                Console.WriteLine($"{index}: Accepted.");
                await Task.Delay(TimeSpan.FromSeconds(5));

                // 成果物があれば、ディクショナリに詰める
                var dateTime = DateTime.Now;
                ValueTuple<int, DateTime> artifact = (x, dateTime);
                dictionary.TryAdd(index, artifact);

                Console.WriteLine($"{index}: Done by {artifact.Item2}");
                
            });

            Console.WriteLine("---ordered");

            // 順番は保証されている。
            foreach ( var elm  in col.Select((item, index) => new { item, index }))
            {
                ValueTuple<int, DateTime> result = default(ValueTuple<int, DateTime>);
                dictionary.TryGetValue(elm.index, out result);
                Console.WriteLine($"index : {elm.index} x: {elm.item} time: {result.Item2} ");
            }
        }
    }
}
