using System;
using System.Collections.Generic;
using TestTaskBall.game;
using TestTaskBall.models;
using TestTaskBall.models.level;
using TestTaskBall.utils;
using UnityEngine;

namespace TestTaskBall.controllers {
    public class ObstaclesController {
        private PoolModel<ObstacleView> _pool;
        
        private List<ObstacleView> _activeObstacles;
        private float _infectionMultiplier;
        
        private event Action _onInfectionComplete;
        
        public void Init(PoolManager poolManager, ObstacleView obstaclePrefab, Transform container, float infectionMultiplier) {
            _pool = poolManager.AddPool(obstaclePrefab, container);
            _activeObstacles = new List<ObstacleView>();
            _infectionMultiplier = infectionMultiplier;
        }

        public void SubscribeOnInfectionComplete(Action onInfectionComplete) {
            _onInfectionComplete += onInfectionComplete;
        }

        public void UnsubscribeOnInfectionComplete(Action onInfectionComplete) {
            _onInfectionComplete -= onInfectionComplete;
        }
        
        public void SpawnObstacles(LevelData levelData) {
            for (int i = 0; i < levelData.Obstacles.Length; i++) {
                var obstacle = _pool.GetOrCreate();
                obstacle.transform.position = new Vector3(levelData.Obstacles[i].x, obstacle.transform.position.y, levelData.Obstacles[i].y);
                _activeObstacles.Add(obstacle);
            }
            for (int i = 0; i < levelData.DecorativeObstacles.Length; i++) {
                var obstacle = _pool.GetOrCreate();
                obstacle.transform.position = new Vector3(levelData.DecorativeObstacles[i].x, obstacle.transform.position.y, levelData.DecorativeObstacles[i].y);
            }
        }

        public void Infect(ObstacleView obstacleView, float radius) {
            if (obstacleView == null) {
                _onInfectionComplete?.Invoke();
                return;
            }
            
            var point = obstacleView.transform.position;
            var listToRemove = new List<ObstacleView>();
            foreach (var obstacle in _activeObstacles) {
                var offset = obstacle.transform.position - point;
                offset.y = 0f;

                if (offset.sqrMagnitude <= _infectionMultiplier * radius) {
                    obstacle.Infect();
                    listToRemove.Add(obstacle);
                }
            }
            
            foreach (var view in listToRemove) {
                _activeObstacles.Remove(view);
            }
            
            _onInfectionComplete?.Invoke();
        }

        public void Dispose() {
            _pool.Dispose();
            _pool = null;
            _activeObstacles?.Clear();
            _activeObstacles = null;
            _onInfectionComplete = null;
        }
    }
}