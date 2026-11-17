using System;
using DG.Tweening;
using UnityEngine;

namespace TestTaskBall.game {
    public class FinishPoint : MonoBehaviour {
        private const float DISTANCE_TO_OPEN_GATES = 5f;
        private const float OPEN_GATES_DURATION = 0.75f;
        private const float OPEN_GATES_OFFSET = 1.25f;
        private const float DEFAULT_GATES_OFFSET = 0.5f;

        [SerializeField] private Transform _leftDoor;
        [SerializeField] private Transform _rightDoor;
        
        private Tween _openDoorTween;
        private Transform _player;
        private bool _isOpened;
        private bool _isActive;
        
        public void SetPlayer(Transform player) {
            _player = player;
        }

        public void Init(Vector3 position) {
            transform.position = position;
            _isActive = true;
        }

        private void PlayOpenAnimation() {
            _openDoorTween?.Kill();
            _openDoorTween = DOVirtual.Float(DEFAULT_GATES_OFFSET, OPEN_GATES_OFFSET, OPEN_GATES_DURATION, progress => {
                _leftDoor.localPosition = new Vector3(-progress, _leftDoor.localPosition.y, 0f);
                _rightDoor.localPosition = new Vector3(progress, _rightDoor.localPosition.y, 0f);
            });
        }

        private void Update() {
            if (_isOpened || _player == null || !_isActive) {
                return;
            }
            
            if (transform.position.z - _player.position.z <= DISTANCE_TO_OPEN_GATES) {
                _isOpened = true;
                PlayOpenAnimation();
            }
        }

        private void OnDestroy() {
            _openDoorTween?.Kill();
            _openDoorTween = null;
            _player = null;
        }
    }
}