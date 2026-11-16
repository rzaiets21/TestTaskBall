using System;
using TestTaskBall.components;
using UnityEngine;

namespace TestTaskBall.game {
    public class BallPlayer : MonoBehaviour, ISelectableObject {
        private const float RADIUS_DECREASE_SPEED = 0.1f;

        [SerializeField] private float _sizeOnStart = 1f;

        public event Action<float> OnRadiusDecreasing;
        
        private bool _isSelected;
        private float _radius = 1f;
        private Action _onClickCallback;

        public void Init(Action clickAction) {
            _onClickCallback = clickAction; 
        }

        public void OnObjectClicked() {
            _onClickCallback?.Invoke();
            _isSelected = true;
        }

        public void OnObjectReleased() {
            _isSelected = false;
        }

        private void Start() {
            _radius = _sizeOnStart;
        }

        private void Update() {
            if (!_isSelected) {
                return;
            }
            DecreaseRadius();
        }

        private void DecreaseRadius() {
            var decreasingRadius = Time.deltaTime * RADIUS_DECREASE_SPEED;
            _radius -= decreasingRadius;

            UpdateScale();
            OnRadiusDecreasing?.Invoke(decreasingRadius);
        }

        private void UpdateScale() {
            transform.localScale = _radius * Vector3.one;
        }
    }
}