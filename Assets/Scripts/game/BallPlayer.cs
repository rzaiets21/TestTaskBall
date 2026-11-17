using System;
using TestTaskBall.components;
using TestTaskBall.game.abstraction;
using UnityEngine;

namespace TestTaskBall.game {
    public class BallPlayer : BallBase, ISelectableObject {
        private const float RADIUS_DECREASE_TIME = 6f;
        private const float CRITICAL_SIZE_RATIO = 0.05f;

        public event Action<float> OnRadiusDecreasing;
        public event Action OnCriticalSizeAchieved;

        private float _decreaseSpeed;
        private bool _isSelected;
        private Action _onClickCallback;
        private Action _onRadiusDecreasingCallback;

        public override void SetInitialRadius(float radius) {
            base.SetInitialRadius(radius);
            _decreaseSpeed = radius / RADIUS_DECREASE_TIME;
        }
        
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

        private void Update() {
            if (!_isSelected) {
                return;
            }
            DecreaseRadius();
        }

        private void DecreaseRadius() {
            var decreasingRadius = Time.deltaTime * _decreaseSpeed;
            SetRadius(_radius - decreasingRadius);
            OnRadiusDecreasing?.Invoke(decreasingRadius);

            if (_size <= CRITICAL_SIZE_RATIO) {
                OnCriticalSizeAchieved?.Invoke();
            }
        }
    }
}