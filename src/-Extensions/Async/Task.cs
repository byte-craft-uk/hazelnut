﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Hazelnut
{
    partial class HazelnutExtensions
    {
        /// <summary>
        /// It's recommended to use Task.Factory.RunSync() instead.
        /// If you can't, at then call this while making it explicit that you know what you're doing.
        /// </summary>        
        [EscapeGCop("I AM the solution to the GCop warning itself!")]
        public static TResult RiskDeadlockAndAwaitResult<TResult>(this Task<TResult> task)
              => Task.Run(async () => await task.ConfigureAwait(false)).Result;

        /// <summary>
        /// If the task is not completed already it throws an exception warning you to await the task.
        /// If the task wraps an exception, the wrapped exception will be thrown.
        /// Otherwise the result will be returned.
        /// Use this instead of calling the Result property when you know that the result is ready to avoid deadlocks.
        /// </summary>
        [EscapeGCop("I AM the solution to the GCop warning itself!")]
        public static TResult GetAlreadyCompletedResult<TResult>(this Task<TResult> @this)
        {
            if (@this == null) return default;

            if (!@this.IsCompleted)
                throw new InvalidOperationException("This task is not completed yet. Do you need to await it?");

            if (@this.Exception is not null)
                throw @this.Exception.InnerException ?? @this.Exception;

            return @this.Result;
        }

        /// <summary>
        /// If the task is not completed already it throws an exception warning you to await the task.
        /// If the task wraps an exception, a default value will be returned.
        /// Otherwise the result will be returned.
        /// Use this when you know that the task is already completed and you want to get the result or a default value in case of .
        /// </summary>
        [EscapeGCop("I AM the solution to the GCop warning itself!")]
        public static TResult GetAlreadyCompletedResultOrDefault<TResult>(this Task<TResult> @this)
        {
            if (@this == null) return default;

            if (!@this.IsCompleted)
                throw new InvalidOperationException("This task is not completed yet. Do you need to await it?");

            if (@this.Exception is not null) return default;

            return @this.Result;
        }

        /// <summary>
        /// Waits for a task to complete, and then if it contains an exception, it will be thrown.
        /// </summary>
        public static void WaitAndThrow(this Task @this)
        {
            if (@this == null) return;

            try { @this.Wait(); }
            catch (AggregateException ex) { throw ex.InnerException; }

            if (@this.Exception?.InnerException != null)
                throw @this.Exception.InnerException;

            if (@this.Exception != null)
                throw @this.Exception;
        }

        public static async Task WithTimeout(this Task @this, TimeSpan timeout, Action success = null, Action timeoutAction = null)
        {
            if (await Task.WhenAny(@this, Task.Delay(timeout)).ConfigureAwait(false) == @this) success?.Invoke();
            else if (timeoutAction is null) throw new TimeoutException("The task didn't complete within " + timeout + "ms");
            else timeoutAction();
        }

        public static async Task<T> WithTimeout<T>(this Task<T> @this, TimeSpan timeout, Action success = null, Func<T> timeoutAction = null)
        {
            if (await Task.WhenAny(@this, Task.Delay(timeout)).ConfigureAwait(false) == @this)
            {
                success?.Invoke();
                return @this.GetAlreadyCompletedResult();
            }

            if (timeoutAction == null)
                throw new TimeoutException("The task didn't complete within " + timeout + "ms");
            else return timeoutAction();
        }

        public static Task<List<T>> ToList<T>(this Task<IEnumerable<T>> @this)
            => @this.Get(x => x.OrEmpty().ToList());

        public static Task<T[]> ToArray<T>(this Task<IEnumerable<T>> @this)
            => @this.Get(x => x.OrEmpty().ToArray());

        public static async Task AwaitAll(this IEnumerable<Task> @this)
            => await Task.WhenAll(@this).ConfigureAwait(false);

        public static async Task<IEnumerable<T>> AwaitAll<T>(this IEnumerable<Task<T>> @this)
            => await Task.WhenAll(@this).ConfigureAwait(false);

        /// <summary>
        /// Casts the result type of the input task as if it were covariant.
        /// </summary>
        /// <typeparam name="TOriginal">The original result type of the task</typeparam>
        /// <typeparam name="TTarget">The covariant type to return</typeparam>
        /// <param name="this">The target task to cast</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<TTarget> AsTask<TOriginal, TTarget>(this Task<TOriginal> @this)
            where TOriginal : TTarget => @this?.ContinueWith(t => (TTarget)t.GetAlreadyCompletedResult());

        /// <summary>
        /// Casts it into a Task of IEnumerable, so the Linq methods can be invoked on it.
        /// </summary> 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<IEnumerable<T>> ForLinq<T>(this Task<T[]> @this)
            => @this.AsTask<T[], IEnumerable<T>>();

        /// <summary>
        /// Casts it into a Task of IEnumerable, so the Linq methods can be invoked on it.
        /// </summary> 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<IEnumerable<T>> ForLinq<T>(this Task<List<T>> @this)
            => @this.AsTask<List<T>, IEnumerable<T>>();

        /// <summary>
        /// Casts it into a Task of IEnumerable, so the Linq methods can be invoked on it.
        /// </summary> 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<IEnumerable<T>> ForLinq<T>(this Task<IList<T>> @this)
            => @this.AsTask<IList<T>, IEnumerable<T>>();

        /// <summary>
        /// Casts it into a Task of IEnumerable, so the Linq methods can be invoked on it.
        /// </summary> 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<IEnumerable<T>> ForLinq<T>(this Task<IOrderedEnumerable<T>> @this)
            => @this.AsTask<IOrderedEnumerable<T>, IEnumerable<T>>();

        /// <summary>
        /// A shorter more readable alternative to ContinueWith().
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<TResult> Get<TSource, TResult>(this Task<TSource> @this,
            Func<TSource, TResult> expression)
        {
            if (@this == null) return expression(default);
            return expression(await @this.ConfigureAwait(continueOnCapturedContext: false));
        }

        /// <summary>
        /// A shorter more readable alternative to nested ContinueWith() methods.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<TResult> Get<TSource, TResult>(this Task<TSource> @this, Func<TSource, Task<TResult>> expression)
        {
            var item = await @this.ConfigureAwait(continueOnCapturedContext: false);
            return await expression(item).ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// A shorter more readable alternative to nested ContinueWith() methods.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task Then<TSource>(this Task<TSource> @this, Action<TSource> action)
        {
            var item = await @this.ConfigureAwait(continueOnCapturedContext: false);
            action(item);
        }

        /// <summary>
        /// A shorter more readable alternative to nested ContinueWith() methods.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task Then(this Task @this, Action action)
        {
            await @this.ConfigureAwait(continueOnCapturedContext: false);
            action();
        }

        /// <summary>
        /// A shorter more readable alternative to nested ContinueWith() methods.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task Then(this Task @this, Func<Task> then)
        {
            await @this.ConfigureAwait(continueOnCapturedContext: false);
            await then();
        }

        /// <summary>
        /// Awaits this task. If the result was an exception,
        /// it will return the default value of TResult rather than throwing the exception.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<TResult> ResultOrDefault<TResult>(this Task<TResult> @this)
        {
            try { return await @this.ConfigureAwait(continueOnCapturedContext: false); }
            catch
            {
                // No logging is needed
                return default;
            }
        }

        /// <summary>
        /// Awaits all tasks (in parallel) and will return the result of the fastest one.
        /// It will move on to the next one if the result is an empty string.
        /// Also it will immediately return null if the specified timeout got exceeded.
        /// </summary>
        public static Task<string> FastestOrDefault(this IEnumerable<Task<string>> @this, TimeSpan timeout)
            => @this.FastestOrDefault(result => result.HasValue(), timeout);

        /// <summary>
        /// Awaits all tasks (in parallel) and will return the result of the fastest one.
        /// It will await for the second fastest task if the resultValidator returns false and so on.
        /// Also it will immediately return a default if the specified timeout got exceeded.
        /// </summary>
        public static Task<T> FastestOrDefault<T>(this IEnumerable<Task<T>> @this, Func<T, bool> resultValidator, TimeSpan timeout)
        {
            var source = new TaskCompletionSource<T>();

            // No need to wait on the tasks
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            @this.Select(t => t.ContinueWith(task =>
            {
                var result = task.GetAlreadyCompletedResultOrDefault();
                if (resultValidator(result)) source.TrySetResult(result);
            })).AwaitAll();
#pragma warning restore CS4014

            return source.Task.WithTimeout(timeout, timeoutAction: () => default);
        }

        public static TResult AwaitResultWithoutContext<TResult>(this Task<TResult> task)
        {
            return task.ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().GetResult();
        }

        public static Task OrCompleted(this Task task) => task ?? Task.CompletedTask;

        public static Task<TResult> OrCompleted<TResult>(this Task<TResult> task, TResult result = default)
        {
            return task ?? Task.FromResult(result);
        }
    }
}