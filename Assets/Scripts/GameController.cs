using System;
using System.Collections.Generic;
using TestTaskBall.components;
using TestTaskBall.game;
using TestTaskBall.utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TestTaskBall {
    public class GameController : MonoBehaviour {
        [SerializeField] private InputActionReference _touchActionReference;

        private InputAction _touchAction;
        private BallPlayer _ballPlayer;
        private Camera _camera;
        private bool _pressed;

        public void Init(BallPlayer ballPlayer) {
            _ballPlayer = ballPlayer;
            _camera = Camera.main;
            if (_ballPlayer != null) {
                _ballPlayer.Init(OnBallSelected);
            }
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
            _ballPlayer = null;
        }

        private void OnBallSelected() {
            _pressed = true;
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