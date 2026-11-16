using TestTaskBall.game;
using TestTaskBall.models;
using TestTaskBall.utils;
using UnityEngine;

namespace TestTaskBall.controllers {
    public class BulletsController {
        private PoolModel<BallBullet> _pool;
        private BallBullet _currentBallBullet;
        
        public void Init(PoolManager poolManager, BallBullet bulletPrefab, Transform container) {
            _pool = poolManager.AddPool(bulletPrefab, container);
        }

        public void SpawnBullet(Vector3 position) {
            _currentBallBullet = _pool.GetOrCreate();
            _currentBallBullet.transform.position = position;
        }

        public void IncreaseBulletRadius(float radius) {
            if (_currentBallBullet != null) {
                _currentBallBullet.IncreaseRadius(radius);
            }
        }

        public void Shoot() {
            if (_currentBallBullet != null) {
                //_currentBallBullet.Shoot();
                _currentBallBullet = null;
            }
        }
    }
}