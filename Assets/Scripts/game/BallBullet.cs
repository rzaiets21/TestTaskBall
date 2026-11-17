using System;
using TestTaskBall.components;
using TestTaskBall.game.abstraction;
using UnityEngine;

namespace TestTaskBall.game {
    public class BallBullet : BallBase, IPoolable {
        private const float DISTANCE_DESTINATION = 0.01f;
        private const float DEFAULT_SPEED = 1f;
        private const float MAX_SPEED = 2.5f;
        
        [SerializeField] private LayerMask _collisionMask;
        
        private float _speed = 1f;

        private bool _isActive;
        private ObstacleView _hitObstacle;
        private Action<BallBullet, ObstacleView> _onCompleted;
        private Vector3 _targetPosition;

        public void IncreaseRadius(float radius) {
            SetRadius(_radius + radius);
        }
        
        public void Release() {
            _hitObstacle = null;
            _onCompleted = null;
            _isActive = false;
            gameObject.SetActive(false);
            SetRadius(0f);
        }

        public void Shoot(Vector3 direction, Action<BallBullet, ObstacleView> onCompleted, float maxDistance) {
            _isActive = true;
            _onCompleted = onCompleted;

            var hitObstacle = Physics.SphereCast(transform.position, _size, direction, out RaycastHit hit, maxDistance,
                _collisionMask);
            _targetPosition = transform.position + direction * (hitObstacle ? hit.distance : maxDistance);
            if (hitObstacle && hit.collider != null && hit.collider.TryGetComponent<ObstacleView>(out var obstacleView)) {
                _hitObstacle = obstacleView;
            }

            _speed = Mathf.Clamp(DEFAULT_SPEED / _size, DEFAULT_SPEED, MAX_SPEED);
        }

        private void Update() {
            if (!_isActive) {
                return;
            }
            
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _speed * Time.deltaTime);

            if ((_targetPosition - transform.position).magnitude <= DISTANCE_DESTINATION) {
                _onCompleted?.Invoke(this, _hitObstacle);
            }
        }
    }
}
