﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Linq.Expressions;
namespace SqlSugar
{
    public class ReflectionInoCache : ICacheService
    {
        private static ReflectionInoCache _instance = null;
        private static readonly object _instanceLock = new object();
        public static ReflectionInoCache GetInstance()
        {
            if (_instance == null)
                lock (_instanceLock)
                    if (_instance == null)
                    {
                        _instance = new ReflectionInoCache();
                    }
            return _instance;
        }
        public void Add<V>(string key, V value)
        {
            ReflectionInoCache<V>.GetInstance().Add(key,value);
        }
        public void Add<V>(string key, V value, int cacheDurationInSeconds)
        {
            ReflectionInoCache<V>.GetInstance().Add(key, value,cacheDurationInSeconds);
        }

        public bool ContainsKey<V>(string key)
        {
           return ReflectionInoCache<V>.GetInstance().ContainsKey(key);
        }

        public V Get<V>(string key)
        {
            return ReflectionInoCache<V>.GetInstance().Get(key);
        }

        public IEnumerable<string> GetAllKey<V>()
        {
            return ReflectionInoCache<V>.GetInstance().GetAllKey();
        }

        public V GetOrCreate<V>(string cacheKey, Func<V> create)
        {
            return ReflectionInoCache<V>.GetInstance().GetOrCreate(cacheKey, create);
        }

        public void Remove<V>(string key)
        {
            ReflectionInoCache<V>.GetInstance().Remove(key);
        }
    }
    public class ReflectionInoCache<V>  
    {
        readonly System.Collections.Concurrent.ConcurrentDictionary<string, V> InstanceCache = new System.Collections.Concurrent.ConcurrentDictionary<string, V>();
        private static ReflectionInoCache<V> _instance = null;
        private static readonly object _instanceLock = new object();
        private ReflectionInoCache() { }

        public V this[string key]
        {
            get
            {
                return this.Get(key);
            }
        }

        public bool ContainsKey(string key)
        {
            return this.InstanceCache.ContainsKey(key);
        }

        public V Get(string key)
        {
            if (this.ContainsKey(key))
                return this.InstanceCache[key];
            else
                return default(V);
        }

        public static ReflectionInoCache<V> GetInstance()
        {
            if (_instance == null)
                lock (_instanceLock)
                    if (_instance == null)
                    {
                        _instance = new ReflectionInoCache<V>();
                        Action addItem =()=> { ReflectionInoCache<V>.GetInstance().RemoveAllCache(); };
                        CacheHelper.AddRemoveFunc(addItem);
                    }
            return _instance;
        }

        public void Add(string key, V value)
        {
            this.InstanceCache.GetOrAdd(key, value);
        }

        public void Add(string key, V value, int cacheDurationInSeconds)
        {
            Check.ThrowNotSupportedException("ReflectionInoCache.Add(string key, V value, int cacheDurationInSeconds)");
        }

        public void Remove(string key)
        {
            V val;
            this.InstanceCache.TryRemove(key, out val);
        }

        public void RemoveAllCache()
        {
            foreach (var key in GetAllKey())
            {
                this.Remove(key);
            }
        }

        public IEnumerable<string> GetAllKey()
        {
            return this.InstanceCache.Keys;
        }

        public V GetOrCreate(string cacheKey, Func<V> create)
        {
            if (this.ContainsKey(cacheKey)) return Get(cacheKey);
            else
            {
                var reval = create();
                this.Add(cacheKey, reval);
                return reval;
            }
        }
    }
    internal static class CacheHelper
    {
        private static List<Action> removeActions = new List<Action>();
        internal static void AddRemoveFunc(Action removeAction)
        {
            removeActions.Add(removeAction);
        }

        public static void RemoveAllCache()
        {
            lock (removeActions)
            {
                foreach (var item in removeActions)
                {
                    item();
                }
            }
        }
    }
}
