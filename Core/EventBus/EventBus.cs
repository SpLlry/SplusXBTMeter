using System.Collections.Concurrent;

namespace SplusXBTMeter.Core
{
    /// <summary>
    /// 支持主动拉取数据的事件总线（带变化检测）
    /// </summary>
    public static class EventBus
    {
        // 存储订阅者
        private static readonly ConcurrentDictionary<Type, SubscriptionList> _subscriptions = new();
        private static readonly ConcurrentDictionary<Type, SubscriptionList> _onceSubscriptions = new();

        // 存储最新事件数据
        private static readonly ConcurrentDictionary<Type, object> _latestEventData = new();

        // 存储上一次发布的数据（用于变化检测）
        private static readonly ConcurrentDictionary<Type, object> _previousEventData = new();

        // 存储数据提供者
        private static readonly ConcurrentDictionary<Type, Func<object>> _dataProviders = new();

        #region 订阅方法

        public static void Subscribe<TEvent>(Action<TEvent> handler)
        {
            var eventType = typeof(TEvent);
            var subscriptionList = _subscriptions.GetOrAdd(eventType, _ => new SubscriptionList());
            subscriptionList.Add(handler);

            // 如果有最新数据，立即推送给新订阅者
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

        public static void SubscribeOnce<TEvent>(Action<TEvent> handler)
        {
            var eventType = typeof(TEvent);
            var subscriptionList = _onceSubscriptions.GetOrAdd(eventType, _ => new SubscriptionList());
            subscriptionList.Add(handler);
        }

        public static void SubscribeAsync<TEvent>(Func<TEvent, Task> handler)
        {
            var eventType = typeof(TEvent);
            var subscriptionList = _subscriptions.GetOrAdd(eventType, _ => new SubscriptionList());
            subscriptionList.Add(handler);
        }

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

        #region 数据提供者方法

        public static void RegisterDataProvider<TEvent>(Func<TEvent> dataProvider)
        {
            var eventType = typeof(TEvent);
            _dataProviders[eventType] = () => dataProvider()!;
        }

        public static TEvent? Pull<TEvent>()
        {
            var eventType = typeof(TEvent);

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

            if (_latestEventData.TryGetValue(eventType, out var cachedData))
            {
                return (TEvent)cachedData;
            }

            return default;
        }

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

        #region 发布方法（支持强制发布参数）

        /// <summary>
        /// 发布事件（同步）- 支持强制发布
        /// </summary>
        /// <param name="eventData">事件数据</param>
        /// <param name="force">是否强制发布（跳过变化检测）</param>
        public static void Publish<TEvent>(TEvent eventData, bool force = false)
        {
            if (eventData == null) return;

            // 🔥 修复：使用真实类型，保证第二次发布能命中
            var eventType = eventData.GetType();

            // 🔥 如果不是强制发布，检查数据是否变化
            bool skip = ShouldSkipPublish(eventType, eventData);
            Console.WriteLine($"force{force}ShouldSkipPublish{skip}");
            if (!force && skip)
            {
                Console.WriteLine($"📢 跳过发布事件 [{eventType.Name}]：数据未变化");
                return;
            }

            // 更新缓存
            _latestEventData[eventType] = eventData;
            _previousEventData[eventType] = eventData;

            // 处理持久订阅
            PublishToSubscribers(eventType, eventData, _subscriptions);

            // 处理一次性订阅
            PublishToSubscribers(eventType, eventData, _onceSubscriptions);

            // 清除一次性订阅
            _onceSubscriptions.TryRemove(eventType, out _);
        }

        /// <summary>
        /// 发布事件（异步）- 支持强制发布
        /// </summary>
        /// <param name="eventData">事件数据</param>
        /// <param name="force">是否强制发布（跳过变化检测）</param>
        public static async Task PublishAsync<TEvent>(TEvent eventData, bool force = false)
        {
            if (eventData == null) return;

            // 🔥 修复：使用真实类型
            var eventType = eventData.GetType();

            // 🔥 如果不是强制发布，检查数据是否变化
            if (!force && ShouldSkipPublish(eventType, eventData))
            {
                Console.WriteLine($"📢 跳过发布事件 [{eventType.Name}]：数据未变化");
                return;
            }

            // 更新缓存
            _latestEventData[eventType] = eventData;
            _previousEventData[eventType] = eventData;

            // 处理持久订阅
            await PublishToSubscribersAsync(eventType, eventData, _subscriptions);

            // 处理一次性订阅
            await PublishToSubscribersAsync(eventType, eventData, _onceSubscriptions);

            // 清除一次性订阅
            _onceSubscriptions.TryRemove(eventType, out _);
        }

        /// <summary>
        /// 强制发布事件（同步）- 快捷方法
        /// </summary>
        public static void PublishForce<TEvent>(TEvent eventData)
        {
            Publish(eventData, true);
        }

        /// <summary>
        /// 强制发布事件（异步）- 快捷方法
        /// </summary>
        public static Task PublishForceAsync<TEvent>(TEvent eventData)
        {
            return PublishAsync(eventData, true);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 判断是否应该跳过发布（数据未变化）
        /// </summary>
        private static bool ShouldSkipPublish<TEvent>(Type eventType, TEvent eventData)
        {
            // 空数据不跳过
            Console.WriteLine($"{eventData} is eventData");
            if (eventData == null) return false;

            // 检查是否有上一次数据
            if (!_previousEventData.TryGetValue(eventType, out var previousData))
            {
                Console.WriteLine($"检查是否有上一次数据无");
                return false;
            }
            Console.WriteLine($"比较数据是否相同{previousData}{eventData}");
            // 比较数据是否相同
            return IsEqual(previousData, eventData);
        }
        /// <summary>
        /// 比较两个对象是否相等（终极稳定版：序列化后比较哈希，支持所有类 + List + 复杂对象）
        /// </summary>
        private static bool IsEqual<T>(object previousData, T newData)
        {
            try
            {
                if (previousData == null && newData == null)
                    return true;

                if (previousData == null || newData == null)
                    return false;

                // 序列化成字符串后比较哈希（最稳、最简单、永不报错）
                var hash1 = System.Text.Json.JsonSerializer.Serialize(previousData).GetHashCode();
                var hash2 = System.Text.Json.JsonSerializer.Serialize(newData).GetHashCode();

                return hash1 == hash2;
            }
            catch
            {
                // 异常时降级为引用比较，保证不崩溃
                return ReferenceEquals(previousData, newData);
            }
        }

        /// <summary>
        /// 同步发布给订阅者
        /// </summary>
        private static void PublishToSubscribers<TEvent>(Type eventType, TEvent eventData,
            ConcurrentDictionary<Type, SubscriptionList> subscriptions)
        {
            if (!subscriptions.TryGetValue(eventType, out var subscriptionList))
                return;

            var handlers = subscriptionList.GetHandlers().ToList();
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

        /// <summary>
        /// 异步发布给订阅者
        /// </summary>
        private static async Task PublishToSubscribersAsync<TEvent>(Type eventType, TEvent eventData,
            ConcurrentDictionary<Type, SubscriptionList> subscriptions)
        {
            if (!subscriptions.TryGetValue(eventType, out var subscriptionList))
                return;

            var tasks = new List<Task>();
            var handlers = subscriptionList.GetHandlers().ToList();

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

        #endregion

        #region 实用方法

        public static void ClearAll()
        {
            _subscriptions.Clear();
            _onceSubscriptions.Clear();
            _latestEventData.Clear();
            _previousEventData.Clear();
            _dataProviders.Clear();
        }

        public static void ClearCache<TEvent>()
        {
            var eventType = typeof(TEvent);
            _latestEventData.TryRemove(eventType, out _);
            _previousEventData.TryRemove(eventType, out _);
        }

        public static Dictionary<string, int> GetSubscriptionStats()
        {
            var stats = new Dictionary<string, int>();

            foreach (var kvp in _subscriptions)
            {
                stats[kvp.Key.Name] = kvp.Value.Count;
            }

            return stats;
        }

        public static bool HasSubscribers<TEvent>()
        {
            var eventType = typeof(TEvent);
            return _subscriptions.ContainsKey(eventType) || _onceSubscriptions.ContainsKey(eventType);
        }

        #endregion

        #region 辅助类

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
    }
}