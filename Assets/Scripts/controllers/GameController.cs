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
        private const float BULLET_SPAWN_OFFSET = 3f;
        
        [SerializeField] private InputActionReference _touchActionReference;
        [SerializeField] private LevelConfig _levelConfig;
        [SerializeField] private Transform _bulletsContainer;
        [SerializeField] private Transform _obstaclesContainer;

        private Vector3 _direction;
        private InputAction _touchAction;
        private BallPlayer _ballPlayer;
        private FinishPoint _finishPoint;
        private BulletsController _bulletsController;
        private ObstaclesController _obstaclesController;
        private PoolManager _poolManager;
        private Camera _camera;
        private bool _pressed;

        public void Init(BallPlayer ballPlayer, BallBullet bulletPrefab, ObstacleView obstaclePrefab, FinishPoint finishPoint) {
            _ballPlayer = ballPlayer;
            _finishPoint = finishPoint;
            _camera = Camera.main;
            if (_ballPlayer != null) {
                _ballPlayer.Init(OnBallSelected);
                _ballPlayer.OnRadiusDecreasing += OnPlayerRadiusDecreasing;
                _direction = (_finishPoint.transform.position - _ballPlayer.transform.position).normalized;
            }

            _poolManager = new PoolManager();
            _bulletsController = new BulletsController();
            _obstaclesController = new ObstaclesController();

            _obstaclesController.Init(_poolManager, obstaclePrefab, _obstaclesContainer, _levelConfig.Data.InfectionMultiplier);
            _bulletsController.Init(_poolManager, bulletPrefab, _bulletsContainer, _obstaclesController, _levelConfig.Data);
            
            LoadLevel();
        }

        private void LoadLevel() {
            var levelData = _levelConfig.Data;
            _obstaclesController.SpawnObstacles(levelData);
            
            _finishPoint.transform.position = _ballPlayer.transform.position + Vector3.forward * levelData.FinishPointDistance;
            _ballPlayer.SetInitialRadius(levelData.InitialRadius);
            _ballPlayer.SetRadius(levelData.InitialRadius);
        }

        private void Awake() {
            _touchAction = _touchActionReference.action;
        }

        private void OnEnable() {
            _touchAction.performed += OnPressed;
            _touchAction.canceled += OnReleased;
            _touchAction.Enable();
        }

        private void OnDisable() {
            _touchAction.performed -= OnPressed;
            _touchAction.canceled -= OnReleased;
            _touchAction.Disable();
            _touchAction = null;
            if (_ballPlayer != null) {
                _ballPlayer.OnRadiusDecreasing -= OnPlayerRadiusDecreasing;
                _ballPlayer = null;
            }
        }

        private void OnBallSelected() {
            _pressed = true;
            _bulletsController.SpawnBullet(_ballPlayer.transform.position + _direction * BULLET_SPAWN_OFFSET);
        }

        private void OnPressed(InputAction.CallbackContext context) {
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
            _bulletsController.Shoot(_direction);
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