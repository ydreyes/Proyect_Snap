using System;
using System.Collections.Generic;

namespace SnapClone.Core.Services
{
    public class GameServices
    {
        private Dictionary<Type, object> _services = new Dictionary<Type, object>();
        
        public void Register<T>(T service) where T : class
        {
            _services[typeof(T)] = service;
        }
        
        public T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out object service))
            {
                return service as T;
            }
            throw new Exception($"Service of type {typeof(T).Name} not registered");
        }
        
        public bool Has<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }
    }
}
