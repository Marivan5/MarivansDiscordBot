using System;
using System.Threading.Tasks;

namespace MarvBotV3
{
    public static class TaskEnumerableExtensions
    {
        public static async Task Pipe<TSource, TResult>(this Task<TSource> source, Action<TSource> pipe) =>
            pipe(await source.ConfigureAwait(false));

        public static async Task<TResult>
            Pipe<TSource, TResult>(this Task<TSource> source, Func<TSource, TResult> pipe) =>
            pipe(await source.ConfigureAwait(false));

        public static async Task<TResult> Pipe<TSource, TResult>(this Task<TSource> source,
            Func<TSource, Task<TResult>> pipe) =>
            await pipe(await source.ConfigureAwait(false))
                .ConfigureAwait(false);
    }
}
