﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TinyCacheLib
{

    public class TinyCache
    {
        private TinyCachePolicy defaultPolicy = new TinyCachePolicy();

        public ICacheStorage Storage { get; internal set; } = new MemoryDictionaryCache();
        public ICacheStorage SecondaryStorage { get; internal set; }

        public EventHandler<Exception> OnError;
        public EventHandler<CacheUpdatedEvt> OnUpdate;
        public EventHandler<CacheUpdatedEvt> OnRemove;
        public EventHandler<bool> OnLoadingChange;

        /// <summary>
        /// Timeouts function after specified amout of milliseconds.
        /// </summary>
        /// <returns>Result of running function.</returns>
        /// <param name="task">Task to run.</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <typeparam name="TResult">Type of result when running function</typeparam>
        public async Task<TResult> TimeoutAfter<TResult>(Func<Task<TResult>> task, double timeout)
        {
            OnLoadingChange?.Invoke(task, true);

            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var tsk = task();
                var completedTask = await Task.WhenAny(tsk, Task.Delay((int)timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == tsk)
                {
                    OnLoadingChange?.Invoke(task, false);
                    timeoutCancellationTokenSource.Cancel();
                    return await tsk;  // Very important in order to propagate exceptions
                }

                OnLoadingChange?.Invoke(task, false);
                throw new TimeoutException("The operation has timed out.");
            }
        }

        public void Store(string key, object data)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (data != null)
            {
                Storage.Store(key, data);
                if (SecondaryStorage != null)
                {
                    SecondaryStorage.Store(key, data);
                }
            }
        }

        /// <summary>
        /// Gets from cache storage and return null if not found.
        /// </summary>
        /// <returns>The from storage.</returns>
        /// <param name="key">Cache key.</param>
        /// <typeparam name="T">Type of the stored data.</typeparam>
        public T GetFromStorage<T>(string key)
        {
            var genericType = typeof(T);
            object ret = Storage.Get(key, genericType);
            if (ret == null && SecondaryStorage != null)
            {
                ret = SecondaryStorage.Get(key, genericType);
                if (ret != null)
                    Storage.Store(key, ret);
            }
            return (T)ret;
        }

        /// <summary>
        /// Fetch using policy
        /// </summary>
        /// <returns>The data from function or cache depending on policy.</returns>
        /// <param name="key">Cache key.</param>
        /// <param name="func">Function for populating cache</param>
        /// <param name="policy">Policy.</param>
        /// <param name="onUpdate">Method to call when data is updated in the background.</param>
        /// <typeparam name="T">Return type of function and cache object.</typeparam>
        public async Task<T> RunAsync<T>(string key, Func<Task<T>> func, TinyCachePolicy policy = null, Action<T> onUpdate = null)
        {
            var genericType = typeof(T);
            object ret = Storage.Get(key, genericType);
            if (ret == null && SecondaryStorage != null)
            {
                ret = SecondaryStorage.Get(key, genericType);
            }
            policy = policy ?? defaultPolicy;

            if (ret == null) {
                ret = await func();
                Store(key, ret, policy);
            }
            else if (policy.UseCacheFirstFunction == null || !policy.UseCacheFirstFunction())
            {
                if (policy.Mode == TinyCacheModeEnum.FetchFirst)
                {
                    try
                    {
                        var realFetch = await TimeoutAfter(func, policy.FetchTimeout);
                        if (!(realFetch is T))
                        {
                            ret = realFetch;
                            Store(key, realFetch, policy);
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(policy, ex);
                        policy.ExceptionHandler?.Invoke(ex, true);
                    }
                }
            }
            StartBackgroundFetch(key, func, policy, onUpdate);
            AddLastFetch(key);
            return (ret == null) ? default(T) : (T)ret;
        }

        /// <summary>
        /// Sets the base policy wich will be used if not specified in each request.
        /// </summary>
        /// <param name="tinyCachePolicy">Tiny cache policy.</param>
        public void SetBasePolicy(TinyCachePolicy tinyCachePolicy)
        {
            defaultPolicy = tinyCachePolicy;
        }

        /// <summary>
        /// Sets the permanent (secondary) cache storage type.
        /// </summary>
        /// <param name="store">Storage instance.</param>
        public void SetCacheStore(ICacheStorage store)
        {
            SecondaryStorage = store;
        }

        /// <summary>
        /// Sets the permanent cache storage type.
        /// </summary>
        /// <param name="store">Storage instance.</param>
        public void SetCacheStore(ICacheStorage primary, ICacheStorage secondary)
        {
            Storage = primary;
            SecondaryStorage = secondary;
        }

        /// <summary>
        /// Remove the specified key from the cache store
        /// </summary>
        /// <returns>The remove.</returns>
        /// <param name="key">Key.</param>
        public void Remove(string key)
        {
            Storage.Remove(key);
            OnRemove?.Invoke(key, new CacheUpdatedEvt()
            {
                Key = key,
                Value = null
            });
        }

        private void StartBackgroundFetch<T>(string key, Func<Task<T>> func, TinyCachePolicy policy, Action<T> onUpdate)
        {
            if (policy.UpdateCacheTimeout > 0)
            {
                if (ShouldFetch(policy.UpdateCacheTimeout, key))
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Delay((int)policy.UpdateCacheTimeout).ContinueWith(async (arg) =>
                    {
                        try
                        {
                            var newvalue = await TimeoutAfter<T>(func, policy.BackgroundFetchTimeout);
                            onUpdate?.Invoke(newvalue);
                            Store(key, newvalue, policy);
                        }
                        catch (Exception ex)
                        {
                            if (policy.ReportExceptionsOnBackgroundFetch)
                            {
                                OnError?.Invoke(policy, ex);
                                policy.ExceptionHandler?.Invoke(ex, true);
                            }
                        }
                    });
                }
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private Object thisLock = new Object();
        private void AddLastFetch(string key)
        {
            lock (thisLock)
            {
                if (lastFetch.ContainsKey(key))
                {
                    lastFetch[key] = DateTime.Now;
                }
                else
                {
                    lastFetch.Add(key, DateTime.Now);
                }
            }
        }

        private bool ShouldFetch(double v, string key)
        {
            if (!lastFetch.ContainsKey(key))
            {
                return true;
            }

            var timeDiff = (DateTime.Now - lastFetch[key]).TotalMilliseconds;
            return (timeDiff > v);
        }

        private void Store(string key, object val, TinyCachePolicy policy)
        {
            if (val != null)
            {
                AddLastFetch(key);

                if (Storage.Store(key, val))
                {
                    policy?.UpdateHandler?.Invoke(key, val);
                    OnUpdate?.Invoke(val, new CacheUpdatedEvt(key, val));
                    if (SecondaryStorage != null)
                    {
                        Task.Delay(10).ContinueWith((a) =>
                        {
                            SecondaryStorage.Store(key, val, false);
                        });
                    }
                }
            }
        }

        private Dictionary<string, DateTime> lastFetch = new Dictionary<string, DateTime>();


    }
}
