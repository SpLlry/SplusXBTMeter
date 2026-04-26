using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SplusXBTMeter.core
{
    /// <summary>
    /// 支持主动拉取数据的事件总线
    /// </summary>
    public static class EventBus
    {
        // 存储订阅者
        private static readonly ConcurrentDictionary<Type, SubscriptionList> _subscriptions = new();
        private static readonly ConcurrentDictionary<Type, SubscriptionList> _onceSubscriptions = new();

        // 🔥 新增：存储最新事件数据（支持主动拉取）
        private static readonly ConcurrentDictionary<Type, object> _latestEventData = new();

        // 🔥 新增：存储数据提供者（用于主动拉取）
        private static readonly ConcurrentDictionary<Type, Func<object>> _dataProviders = new();

        #region 订阅方法

        /// <summary>
        /// 订阅事件（持久订阅）
        /// </summary>
        public static void Subscribe<TEvent>(Action<TEvent> handler)
        {
            var eventType = typeof(TEvent);
            var subscriptionList = _subscriptions.GetOrAdd(eventType, _ => new SubscriptionList());
            subscriptionList.Add(handler);

            // 🔥 如果有最新数据，立即推送给新订阅者
            if (_latestEventData.TryGetValue(eventType, out var latestData))
            {
                try
                {
                    if (latestData is TEvent typedData)
                    {
                        handler.Invoke(typedData);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"推送最新数据失败 [{eventType.Name}]: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 订阅事件（一次性订阅）
        /// </summary>
        public static void SubscribeOnce<TEvent>(Action<TEvent> handler)
        {
            var eventType = typeof(TEvent);
            var subscriptionList = _onceSubscriptions.GetOrAdd(eventType, _ => new SubscriptionList());
            subscriptionList.Add(handler);
        }

        /// <summary>
        /// 异步订阅事件
        /// </summary>
        public static void SubscribeAsync<TEvent>(Func<TEvent, Task> handler)
        {
            var eventType = typeof(TEvent);
            var subscriptionList = _subscriptions.GetOrAdd(eventType, _ => new SubscriptionList());
            subscriptionList.Add(handler);
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        public static void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            var eventType = typeof(TEvent);
            if (_subscriptions.TryGetValue(eventType, out var subscriptionList))
            {
                subscriptionList.Remove(handler);
                if (subscriptionList.Count == 0)
                {
                    _subscriptions.TryRemove(eventType, out _);
                }
            }

            if (_onceSubscriptions.TryGetValue(eventType, out var onceSubscriptionList))
            {
                onceSubscriptionList.Remove(handler);
                if (onceSubscriptionList.Count == 0)
                {
                    _onceSubscriptions.TryRemove(eventType, out _);
                }
            }
        }

        /// <summary>
        /// 取消异步订阅
        /// </summary>
        public static void UnsubscribeAsync<TEvent>(Func<TEvent, Task> handler)
        {
            var eventType = typeof(TEvent);
            if (_subscriptions.TryGetValue(eventType, out var subscriptionList))
            {
                subscriptionList.Remove(handler);
                if (subscriptionList.Count == 0)
                {
                    _subscriptions.TryRemove(eventType, out _);
                }
            }
        }

        #endregion

        #region 数据提供者方法（新增）

        /// <summary>
        /// 注册数据提供者（用于主动拉取数据）
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="dataProvider">数据提供者函数</param>
        public static void RegisterDataProvider<TEvent>(Func<TEvent> dataProvider)
        {
            var eventType = typeof(TEvent);
            _dataProviders[eventType] = () => dataProvider()!;
        }

        /// <summary>
        /// 主动拉取数据
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <returns>事件数据</returns>
        public static TEvent? Pull<TEvent>()
        {
            var eventType = typeof(TEvent);

            // 1. 首先尝试从数据提供者获取
            if (_dataProviders.TryGetValue(eventType, out var dataProvider))
            {
                try
                {
                    return (TEvent)dataProvider.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"数据提供者获取失败 [{eventType.Name}]: {ex.Message}");
                }
            }

            // 2. 如果没有数据提供者，返回缓存的最新数据
            if (_latestEventData.TryGetValue(eventType, out var cachedData))
            {
                return (TEvent)cachedData;
            }

            return default;
        }

        /// <summary>
        /// 获取最新缓存的数据（不触发数据提供者）
        /// </summary>
        public static TEvent? GetLatest<TEvent>()
        {
            var eventType = typeof(TEvent);
            if (_latestEventData.TryGetValue(eventType, out var cachedData))
            {
                return (TEvent)cachedData;
            }
            return default;
        }

        #endregion

        #region 发布方法

        /// <summary>
        /// 发布事件（同步）
        /// </summary>
        public static void Publish<TEvent>(TEvent eventData)
        {
            var eventType = typeof(TEvent);

            // 🔥 缓存最新数据
            _latestEventData[eventType] = eventData!;

            // 处理持久订阅
            if (_subscriptions.TryGetValue(eventType, out var subscriptions))
            {
                var handlers = subscriptions.GetHandlers().ToList();
                foreach (var handler in handlers)
                {
                    try
                    {
                        if (handler is Action<TEvent> syncHandler)
                        {
                            syncHandler.Invoke(eventData);
                        }
                        else if (handler is Func<TEvent, Task> asyncHandler)
                        {
                            _ = Task.Run(async () => await asyncHandler(eventData));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"事件处理失败 [{eventType.Name}]: {ex.Message}");
                    }
                }
            }

            // 处理一次性订阅
            if (_onceSubscriptions.TryGetValue(eventType, out var onceSubscriptions))
            {
                var handlers = onceSubscriptions.GetHandlers().ToList();
                foreach (var handler in handlers)
                {
                    try
                    {
                        if (handler is Action<TEvent> syncHandler)
                        {
                            syncHandler.Invoke(eventData);
                        }
                        else if (handler is Func<TEvent, Task> asyncHandler)
                        {
                            _ = Task.Run(async () => await asyncHandler(eventData));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"一次性事件处理失败 [{eventType.Name}]: {ex.Message}");
                    }
                }

                // 清除一次性订阅
                _onceSubscriptions.TryRemove(eventType, out _);
            }
        }

        /// <summary>
        /// 发布事件（异步）
        /// </summary>
        public static async Task PublishAsync<TEvent>(TEvent eventData)
        {
            var eventType = typeof(TEvent);

            // 🔥 缓存最新数据
            _latestEventData[eventType] = eventData!;

            // 处理持久订阅
            if (_subscriptions.TryGetValue(eventType, out var subscriptions))
            {
                var tasks = new List<Task>();
                var handlers = subscriptions.GetHandlers().ToList();

                foreach (var handler in handlers)
                {
                    try
                    {
                        if (handler is Action<TEvent> syncHandler)
                        {
                            syncHandler.Invoke(eventData);
                        }
                        else if (handler is Func<TEvent, Task> asyncHandler)
                        {
                            tasks.Add(asyncHandler(eventData));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"事件处理失败 [{eventType.Name}]: {ex.Message}");
                    }
                }

                if (tasks.Any())
                {
                    await Task.WhenAll(tasks);
                }
            }

            // 处理一次性订阅
            if (_onceSubscriptions.TryGetValue(eventType, out var onceSubscriptions))
            {
                var tasks = new List<Task>();
                var handlers = onceSubscriptions.GetHandlers().ToList();

                foreach (var handler in handlers)
                {
                    try
                    {
                        if (handler is Action<TEvent> syncHandler)
                        {
                            syncHandler.Invoke(eventData);
                        }
                        else if (handler is Func<TEvent, Task> asyncHandler)
                        {
                            tasks.Add(asyncHandler(eventData));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"一次性事件处理失败 [{eventType.Name}]: {ex.Message}");
                    }
                }

                if (tasks.Any())
                {
                    await Task.WhenAll(tasks);
                }

                // 清除一次性订阅
                _onceSubscriptions.TryRemove(eventType, out _);
            }
        }

        #endregion

        #region 辅助类

        /// <summary>
        /// 订阅列表（线程安全）
        /// </summary>
        private class SubscriptionList
        {
            private readonly List<Delegate> _handlers = new();
            private readonly ReaderWriterLockSlim _lock = new();

            public int Count
            {
                get
                {
                    _lock.EnterReadLock();
                    try
                    {
                        return _handlers.Count;
                    }
                    finally
                    {
                        _lock.ExitReadLock();
                    }
                }
            }

            public void Add(Delegate handler)
            {
                _lock.EnterWriteLock();
                try
                {
                    if (!_handlers.Contains(handler))
                    {
                        _handlers.Add(handler);
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }

            public void Remove(Delegate handler)
            {
                _lock.EnterWriteLock();
                try
                {
                    _handlers.Remove(handler);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }

            public IEnumerable<Delegate> GetHandlers()
            {
                _lock.EnterReadLock();
                try
                {
                    return _handlers.ToList();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        #endregion

        #region 实用方法

        /// <summary>
        /// 清除所有订阅和数据
        /// </summary>
        public static void ClearAll()
        {
            _subscriptions.Clear();
            _onceSubscriptions.Clear();
            _latestEventData.Clear();
            _dataProviders.Clear();
        }

        /// <summary>
        /// 清除缓存的数据
        /// </summary>
        public static void ClearCache<TEvent>()
        {
            var eventType = typeof(TEvent);
            _latestEventData.TryRemove(eventType, out _);
        }

        /// <summary>
        /// 获取订阅统计信息
        /// </summary>
        public static Dictionary<string, int> GetSubscriptionStats()
        {
            var stats = new Dictionary<string, int>();

            foreach (var kvp in _subscriptions)
            {
                stats[kvp.Key.Name] = kvp.Value.Count;
            }

            return stats;
        }

        /// <summary>
        /// 检查是否有订阅者
        /// </summary>
        public static bool HasSubscribers<TEvent>()
        {
            var eventType = typeof(TEvent);
            return _subscriptions.ContainsKey(eventType) || _onceSubscriptions.ContainsKey(eventType);
        }

        #endregion
    }
}