﻿using System;
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

    }
}
