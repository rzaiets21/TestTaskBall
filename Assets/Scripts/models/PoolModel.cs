using System;
using System.Collections.Generic;
using TestTaskBall.components;
using TestTaskBall.utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TestTaskBall.models {
    public class PoolModel<T> : IPoolModel where T : MonoBehaviour, IPoolable {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly List<T> _list;

        public PoolModel(T prefab, Transform parent) {
            _prefab = prefab;
            _parent = parent;
            _list = new List<T>(16);
        }

        public T GetOrCreate() {
            var obj = _list.FirstOrDefault(x => !x.gameObject.activeSelf);
            if (obj == null) {
                obj = Create();
            }
            
            if (obj == null) {
                throw new NullReferenceException();
            }
            
            obj.gameObject.SetActive(true);
            return obj;
        }

        public void ReleaseAll() {
            foreach (var item in _list) {
                item.Release();
            }
        }

        public void Dispose() {
            _list.Clear();
        }

        private T Create() {
            var createdObject = Object.Instantiate(_prefab, _parent);
            _list.Add(createdObject);
            return createdObject;
        }
    }
}