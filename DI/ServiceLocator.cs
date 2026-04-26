using System;
using System.Collections.Generic;
using SplusXBTMeter.Services;
using SplusXBTMeter.Services.Interfaces;

namespace SplusXBTMeter.DI
{
    public class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        public static void Initialize()
        {
            RegisterServices();
        }

        private static void RegisterServices()
        {
            // 注册服务
            _services[typeof(IBluetoothService)] = new BluetoothService();
        }

        public static T Get<T>()
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }

            throw new InvalidOperationException($"Service of type {typeof(T).Name} not registered.");
        }

        public static T Create<T>() where T : class
        {
            return Activator.CreateInstance(typeof(T)) as T ?? throw new InvalidOperationException($"Failed to create instance of {typeof(T).Name}.");
        }
    }
}