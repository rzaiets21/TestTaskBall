using System;
using System.Collections.Generic;
using TestTaskBall.components;
using TestTaskBall.game;
using TestTaskBall.models.level;
using TestTaskBall.utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TestTaskBall.controllers {
    public class GameController : MonoBehaviour {
        private const float MOVEMENT_DISTANCE_THRESHOLD = 0.1f;
        
        [SerializeField] private LayerMask _obstacleMask;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private InputActionReference _touchActionReference;
        [SerializeField] private LevelConfig _levelConfig;
        [SerializeField] private Transform _bulletsContainer;
        [SerializeField] private Transform _obstaclesContainer;

        public event Action<bool> OnGameFinished;
        
        private Vector3 _direction;
        private Vector3 _playerStartPosition;
        private InputAction _touchAction;
        private BallPlayer _ballPlayer;
        private FinishPoint _finishPoint;
        private BulletsController _bulletsController;
        private ObstaclesController _obstaclesController;
        private PoolManager _poolManager;
        private Camera _camera;
        private bool _pressed;
        private bool _complete;

        public void Init(BallPlayer ballPlayer, BallBullet bulletPrefab, ObstacleView obstaclePrefab, FinishPoint finishPoint) {
            _ballPlayer = ballPlayer;
            _finishPoint = finishPoint;
            _camera = Camera.main;
            if (_ballPlayer != null) {
                var playerTransform = _ballPlayer.transform;
                _ballPlayer.Init(OnBallSelected);
                _ballPlayer.OnRadiusDecreasing += OnPlayerRadiusDecreasing;
                _ballPlayer.OnCriticalSizeAchieved += OnCriticalSizeAchieved;
                _finishPoint.SetPlayer(playerTransform);
                _cameraController.SetTarget(playerTransform);
                _playerStartPosition = playerTransform.position;
            }

            _poolManager = new PoolManager();
            _bulletsController = new BulletsController();
            _obstaclesController = new ObstaclesController();

            _obstaclesController.Init(_poolManager, obstaclePrefab, _obstaclesContainer, _levelConfig.Data.InfectionMultiplier);
            _bulletsController.Init(_poolManager, bulletPrefab, _bulletsContainer, _obstaclesController, _levelConfig.Data);
            
            _obstaclesController.SubscribeOnInfectionComplete(OnInfectionComplete);
            
            LoadLevel();
        }

        public void Restart() {
            _ballPlayer.transform.position = _playerStartPosition;
            _obstaclesController.Reset();
            _bulletsController.Reset();
            _finishPoint.ResetState();
            _cameraController.SnapToTarget();
            LoadLevel();
        }

        private void LoadLevel() {
            var levelData = _levelConfig.Data;
            _obstaclesController.SpawnObstacles(levelData);
            
            _finishPoint.Init(_ballPlayer.transform.position + Vector3.forward * levelData.FinishPointDistance);
            _ballPlayer.SetInitialRadius(levelData.InitialRadius);
            _ballPlayer.SetRadius(levelData.InitialRadius);
            
            _direction = (_finishPoint.transform.position - _ballPlayer.transform.position).normalized;
        }

        private void Awake() {
            _touchAction = _touchActionReference.action;
        }

        private void Start() {
            _touchAction.performed += OnPressed;
            _touchAction.canceled += OnReleased;
            _touchAction.Enable();
        }

        private void OnDestroy() {
            _touchAction.performed -= OnPressed;
            _touchAction.canceled -= OnReleased;
            _touchAction.Disable();
            _touchAction = null;
            if (_ballPlayer != null) {
                _ballPlayer.OnCriticalSizeAchieved -= OnCriticalSizeAchieved;
                _ballPlayer.OnRadiusDecreasing -= OnPlayerRadiusDecreasing;
                _ballPlayer = null;
            }
            _finishPoint = null;
            _bulletsController.Dispose();
            _bulletsController = null;
            _obstaclesController.SubscribeOnInfectionComplete(OnInfectionComplete);
            _obstaclesController.Dispose();
            _obstaclesController = null;
            _poolManager.Dispose();
            _poolManager = null;
            _camera = null;
            OnGameFinished = null;
        }

        private void OnCriticalSizeAchieved() {
            _pressed = false;
            _ballPlayer.OnObjectReleased();
            _bulletsController.DestroyBullet();
            OnGameFinished?.Invoke(false);
        }

        private void OnInfectionComplete() {
            var targetZ = CalculateTargetZ();
            if (targetZ - _ballPlayer.transform.position.z >= MOVEMENT_DISTANCE_THRESHOLD) {
                _ballPlayer.MoveTo(targetZ, OnPlayerMovementComplete);
                _ballPlayer.ShowTrail(false);
            }
        }
        
        private float CalculateTargetZ() {
            var playerPosition = _ballPlayer.transform.position;
            var currentZ = playerPosition.z;
            var finishZ = _finishPoint.transform.position.z;
            var distance = finishZ - currentZ;

            if (distance <= 0f) {
                return currentZ;
            }

            if (Physics.SphereCast(_ballPlayer.transform.position, _ballPlayer.Radius/4f, Vector3.forward, out RaycastHit hit, distance, _obstacleMask)) {
                return currentZ + Mathf.Max(0f, hit.distance - _levelConfig.Settings.StopGap);
            }

            return finishZ;
        }

        private void OnPlayerMovementComplete() {
            if (_finishPoint.transform.position.z - _ballPlayer.transform.position.z <= MOVEMENT_DISTANCE_THRESHOLD) {
                _complete = true;
                OnGameFinished?.Invoke(true);
                return;
            }

            _cameraController.StartFollow(true);
            _ballPlayer.ShowTrail(true);
        }

        private void OnBallSelected() {
            _pressed = true;
            _bulletsController.SpawnBullet(_ballPlayer.transform.position + _direction * _levelConfig.Settings.StopGap);
        }

        private void OnPressed(InputAction.CallbackContext context) {
            if (_complete) {
                return;
            }
            
            if(context.control.device is Pointer pointer) {
                var ray = _camera.ScreenPointToRay(pointer.position.ReadValue());
                if (Physics.Raycast(ray, out var hit)) {
                    var clickableObject = hit.transform.GetComponent<ISelectableObject>();
                    if (clickableObject != null) {
                        clickableObject.OnObjectClicked();
                    }
                } else {
                    var clickableObjects2D = GetRaycastColliders2D(ray);
                    if (!clickableObjects2D.IsNullOrEmpty()) {
                        if (clickableObjects2D.Count >= 1) {
                            clickableObjects2D[0].OnObjectClicked();
                        }
                    }
                }
            }
        }

        private void OnReleased(InputAction.CallbackContext obj) {
            if (!_pressed) {
                return;
            }
            
            _pressed = false;
            _ballPlayer.OnObjectReleased();
            _bulletsController.Shoot(_direction, _finishPoint.transform.position.z - _ballPlayer.transform.position.z);
            _cameraController.StartFollow(false);
        }

        private void OnPlayerRadiusDecreasing(float radius) {
            if (_bulletsController != null) {
                _bulletsController.IncreaseBulletRadius(radius);
            }
        }

        private static List<ISelectableObject> GetRaycastColliders2D(Ray ray) {
            return ProcessRaycastColliders2D(Physics2D.OverlapPointAll(ray.origin));
        }
        
        private static List<ISelectableObject> ProcessRaycastColliders2D(Collider2D[] colliders) {
            var clickableObjects2D = new List<ISelectableObject>();
            for (var i = 0; i < colliders.Length; i++) {
                var clickableObject = colliders[i].transform.GetComponent<ISelectableObject>();
                if (clickableObject != null) {
                    clickableObjects2D.Add(clickableObject);
                }
            }
            return clickableObjects2D;
        }
    }
}