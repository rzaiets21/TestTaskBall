using System;
using TestTaskBall.components;
using TestTaskBall.game.abstraction;
using UnityEngine;

namespace TestTaskBall.game {
    public class BallPlayer : BallBase, ISelectableObject {
        private const float RADIUS_DECREASE_SPEED = 0.1f;

        [SerializeField] private float _sizeOnStart = 1f;

        public event Action<float> OnRadiusDecreasing;
        
        private bool _isSelected;
        private Action _onClickCallback;
        private Action _onRadiusDecreasingCallback;

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
            SetRadius(_sizeOnStart);
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
    }
}