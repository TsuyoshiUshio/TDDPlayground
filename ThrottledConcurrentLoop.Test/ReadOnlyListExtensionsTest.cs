using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThrottledConcurrentLoopSpike;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace ThrottledConcurrentLoop.Test
{
    [TestClass]
    public class ReadOnlyListExtensionsTest
    {
        [TestMethod]
        public async Task TestParallelThrottling()
        {
            List<int> list = Enumerable.Range(0, 100).ToList<int>();
            IReadOnlyList<int> col = new ReadOnlyCollection<int>(list);

            var count = 0;
            var concurrentCount = new ConcurrentStack<int>();

            col.ParallelForEachAsync<int>(10, async x =>
            {
                concurrentCount.Push((Interlocked.Increment(ref count)));
                await Task.Delay(1000);
                concurrentCount.Push((Interlocked.Decrement(ref count)));
            });

            Assert.AreEqual(concurrentCount.Max(), 10);

        }


        [TestMethod]
        public async Task TestParallelThrottlingRx()
        {
            List<int> list = Enumerable.Range(0, 100).ToList<int>();
            IReadOnlyList<int> col = new ReadOnlyCollection<int>(list);
            var subject = new Subject<int>();
            List<Exception> exs = new List<Exception>();
            subject.Window(TimeSpan.FromSeconds(3))
                .SelectMany(o => o.Sum())
                .Subscribe(x =>
                {
                    try
                    {
                        Assert.AreEqual(10, x);
                    } catch(Exception e)
                    {
                        exs.Add(e);
                    }
                });
           
            await col.ParallelForEachAsync<int>(10, async x =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                subject.OnNext(1);
                await Task.Delay(TimeSpan
                    .FromSeconds(2));
            });

            Assert.AreEqual(0, exs.Count);

        }
    }
}
