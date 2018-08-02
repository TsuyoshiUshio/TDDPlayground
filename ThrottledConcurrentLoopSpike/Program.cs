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
            List<ValueTuple<int, int>> list = Enumerable.Range(1, 30).Select((element, index) =>
                { return (index: index, value: element); }
            ).ToList();

            IReadOnlyList<ValueTuple<int,int>> col = new ReadOnlyCollection<ValueTuple<int,int>>(list);

            var dictionary = new ConcurrentDictionary<int, DateTime>();

            // 実行順序は保証できない
            await col.ParallelForEachAsync(10, async (x) =>
            {

                Console.WriteLine($"{x.Item1}: Accepted.");
                await Task.Delay(TimeSpan.FromSeconds(5));

                // 成果物があれば、ディクショナリに詰める
                var dateTime = DateTime.Now;
                dictionary.TryAdd(x.Item1, dateTime);

                Console.WriteLine($"{x.Item1}: Done by {dateTime}");
                
            });

            Console.WriteLine("---ordered");

            // 順番は保証されている。
            foreach ( var elm  in col)
            {
                DateTime result = DateTime.Now;
                dictionary.TryGetValue(elm.Item1, out result);
                Console.WriteLine($"index : {elm.Item1} time: {result} ");
            }


        }
    }
}
