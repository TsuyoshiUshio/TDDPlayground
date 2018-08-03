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

            // 実行順序は保証できない
            var artifacts = await col.ParallelForEachAsync<int, ValueTuple<int, DateTime>>(10, async (x) =>
            {

                Console.WriteLine($"{x}: Accepted.");
                await Task.Delay(TimeSpan.FromSeconds(5));

                // 成果物があれば、ディクショナリに詰める
                var dateTime = DateTime.Now;
                ValueTuple<int, DateTime> artifact = (x, dateTime);

                Console.WriteLine($"{x}: Done by {artifact.Item2}");
                return artifact;
                
            });


            Console.WriteLine("---ordered");

            // 順番は保証されている。
            foreach ( var elm  in artifacts)
            {
                Console.WriteLine($"x: {elm.Item1} time: {elm.Item2} ");
            }
        }
    }
}
