using System;
using System.Collections.Generic;
using TestTaskBall.components;
using TestTaskBall.models;
using UnityEngine;

namespace TestTaskBall.utils {
    public class PoolManager {
        private readonly Dictionary<Type, IPoolModel> _pools = new();

        public PoolModel<T> GetPool<T>() where T : MonoBehaviour, IPoolable {
            if (_pools.TryGetValue(typeof(T), out var pool) && pool is PoolModel<T> targetPool) {
                return targetPool;
            }
            
            throw new NullReferenceException("Pool doesn't contain any pool model of type = " + typeof(T));
        }
        
        public T Get<T>() where T : MonoBehaviour, IPoolable {
            var pool = GetPool<T>();
            if (pool != null) {
                return pool.GetOrCreate();
            }

            throw new NullReferenceException("Pool doesn't contain any pool model of type = " + typeof(T));
        }
        
        public PoolModel<T> AddPool<T>(T prefab, Transform parent) where T : MonoBehaviour, IPoolable {
            var type = typeof(T);
            if (_pools.TryGetValue(type, out var pool) && pool is PoolModel<T> targetPool) {
                Debug.LogWarning("Pool already contains a pool of this prefab");
                return targetPool;
            }
            
            var poolModel = new PoolModel<T>(prefab, parent);
            _pools.Add(type, poolModel);
            return poolModel;
        }

        public void Dispose() {
            _pools.Clear();
        }
    }
}