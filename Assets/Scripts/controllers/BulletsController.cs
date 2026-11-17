using TestTaskBall.game;
using TestTaskBall.models;
using TestTaskBall.models.level;
using TestTaskBall.utils;
using UnityEngine;

namespace TestTaskBall.controllers {
    public class BulletsController {
        private PoolModel<BallBullet> _pool;
        private BallBullet _currentBallBullet;
        private ObstaclesController _obstaclesController;
        private float _initialRadius;
        
        public void Init(PoolManager poolManager, BallBullet bulletPrefab, Transform container, ObstaclesController obstaclesController, LevelData levelData) {
            _pool = poolManager.AddPool(bulletPrefab, container);
            _obstaclesController = obstaclesController;
            _initialRadius = levelData.InitialRadius;
        }

        public void SpawnBullet(Vector3 position) {
            _currentBallBullet = _pool.GetOrCreate();
            _currentBallBullet.transform.position = position;
            _currentBallBullet.SetInitialRadius(_initialRadius);
        }

        public void IncreaseBulletRadius(float radius) {
            if (_currentBallBullet != null) {
                _currentBallBullet.IncreaseRadius(radius);
            }
        }

        public void Shoot(Vector3 direction, float maxBulletDistance) {
            if (_currentBallBullet != null) {
                _currentBallBullet.Shoot(direction, OnBulletComplete, maxBulletDistance);
                _currentBallBullet = null;
            }
        }

        public void DestroyBullet() {
            if (_currentBallBullet != null) {
                _currentBallBullet.Release();
                _currentBallBullet = null;
            }
        }

        public void Dispose() {
            _obstaclesController = null;
            _currentBallBullet = null;
            _pool.Dispose();
            _pool = null;
        }

        private void OnBulletComplete(BallBullet ballBullet, ObstacleView obstacleView) {
            _obstaclesController.Infect(obstacleView, ballBullet.Radius);
            ballBullet.Release();
        }
    }
}