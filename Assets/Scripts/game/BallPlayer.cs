using System;
using DG.Tweening;
using TestTaskBall.components;
using TestTaskBall.game.abstraction;
using UnityEngine;

namespace TestTaskBall.game {
    public class BallPlayer : BallBase, ISelectableObject {
        private const float RADIUS_DECREASE_TIME = 6f;
        private const float CRITICAL_SIZE_RATIO = 0.05f;
        private const float MOVEMENT_DISTANCE_THRESHOLD = 0.001f;
        private const float WIDTH_TRAIL_MULTIPLIER = 0.1f;
        
        public bool IsMoving => _movementTween != null && _movementTween.IsActive();
        public bool IsDead => _size <= CRITICAL_SIZE_RATIO;

        [SerializeField] private Transform _ballVisual;
        [SerializeField] private Transform _ballWidthTrail;
        [SerializeField] private float _movementSpeed;
        [SerializeField] private float _jumpHeight;
        
        public event Action<float> OnRadiusDecreasing;
        public event Action OnCriticalSizeAchieved;

        private Tween _movementTween;
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

        public void ShowTrail(bool state) {
            _ballWidthTrail.gameObject.SetActive(state);
        }

        public void OnObjectClicked() {
            if (IsMoving || IsDead) {
                return;
            }
            _onClickCallback?.Invoke();
            _isSelected = true;
        }

        public void OnObjectReleased() {
            _isSelected = false;
        }

        public void MoveTo(float targetZ, Action onCompleted = null) {
            Stop();

            var startPosition = transform.position;
            var distance = targetZ - startPosition.z;
            if (distance <= MOVEMENT_DISTANCE_THRESHOLD) {
                onCompleted?.Invoke();
                return;
            }

            var duration = distance / _movementSpeed;

            var jumpCount = Mathf.CeilToInt(distance / _jumpHeight);
            var actualJumpLength = distance / jumpCount;
            var height = _jumpHeight * Mathf.Clamp01(actualJumpLength / _jumpHeight);

            _movementTween = DOVirtual.Float(0f, 1f, duration, progress => {
                        var position = startPosition;
                        position.z = Mathf.Lerp(startPosition.z, targetZ, progress);

                        transform.position = position;

                        var phase = Mathf.Repeat(progress * jumpCount, 1f);
                        var y = 4f * height * phase * (1f - phase);

                        _ballVisual.localPosition = Vector3.up * y;
                    })
                .SetEase(Ease.Linear).OnComplete(() => {
                    _ballVisual.localPosition = Vector3.zero;
                    _movementTween = null;

                    onCompleted?.Invoke();
                });
        }
        
        protected override void UpdateScaleAndPosition() {
            _ballVisual.localScale = _size * Vector3.one;
            _ballWidthTrail.localScale = new Vector3(_size * WIDTH_TRAIL_MULTIPLIER, _ballWidthTrail.localScale.y,
                _ballWidthTrail.localScale.z);
            transform.localPosition = new Vector3(transform.localPosition.x, _size / 2f, transform.localPosition.z);
            _ballWidthTrail.localPosition = new Vector3(_ballWidthTrail.localPosition.x, -(_size / 2f) + WIDTH_TRAIL_MULTIPLIER, _ballWidthTrail.localPosition.z);
        }

        private void Stop() {
            _movementTween?.Kill();
            _movementTween = null;

            _ballVisual.localPosition = Vector3.zero;
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

        private void OnDestroy() {
            OnRadiusDecreasing = null;
            OnCriticalSizeAchieved = null;
            _movementTween?.Kill();
            _movementTween = null;
            _onClickCallback = null;
            _onRadiusDecreasingCallback = null;
        }
    }
}