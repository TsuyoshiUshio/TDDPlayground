using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottledConcurrentLoopSpike
{
    public static class IReadOnlyListExtensions
    {
        public static async Task ParallelForEachAsync<T>(this IReadOnlyList<T> items, int maxConcurrency, Func<T, Task> action)
        {
            using (var semaphore = new SemaphoreSlim(maxConcurrency))
            {
                var tasks = new Task[items.Count];
                for (int i = 0; i < items.Count; i++)
                {
                    tasks[i] = InvokeThrottledAction(items[i], action, semaphore);
                }

                await Task.WhenAll(tasks);
            }
        }

        static async Task InvokeThrottledAction<T>(T item, Func<T, Task> action, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();
            try
            {
                await action(item);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public static async Task<IEnumerable<R>> ParallelForEachAsync<T,R>(this IReadOnlyList<T> items, int maxConcurrency, Func<T, Task<R>> action)
        {
            using (var semaphore = new SemaphoreSlim(maxConcurrency))
            {
                var tasks = new Task<R>[items.Count];
                for (int i = 0; i < items.Count; i++)
                {
                    tasks[i] = InvokeThrottledAction<T,R>(items[i], action, semaphore);
                }

                return await Task.WhenAll<R>(tasks.AsEnumerable());
            }
        }

        static async Task<R> InvokeThrottledAction<T, R>(T item, Func<T, Task<R>> action, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();
            try
            {
                return await action(item);
            }
            finally
            {
                semaphore.Release();
            }
        }


        public static async Task IndexedParallelForEachAsync<T>(this IReadOnlyList<T> items, int maxConcurrency, Func<T, int, Task> action)
        {
            using (var semaphore = new SemaphoreSlim(maxConcurrency))
            {
                var tasks = new Task[items.Count];
                for (int i = 0; i < items.Count; i++)
                {
                    tasks[i] = InvokeIndexedThrottledAction(i, items[i], action, semaphore);
                }

                await Task.WhenAll(tasks);
            }
        }

        static async Task InvokeIndexedThrottledAction<T>(int index, T item, Func<T, int, Task> action, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();
            try
            {
                await action(item, index);
            }
            finally
            {
                semaphore.Release();
            }
        }

    }
}
