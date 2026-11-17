using System;
using UnityEngine;

namespace TestTaskBall.controllers {
    public class CameraController : MonoBehaviour {
        [SerializeField] private Vector3 _offset = new(0f, 6f, -8f);
        [SerializeField] private float _followSharpness = 6f;

        private Transform _target;
        private bool _following;

        public void SetTarget(Transform target) {
            if (target == null) {
                return;
            }
            
            _target = target;
            SnapToTarget();
        }

        public void StartFollow(bool state) {
            _following = state;
        }

        public void SnapToTarget() {
            if (_target != null)
                transform.position = _target.position + _offset;
        }

        private void LateUpdate() {
            if (!_following || _target == null)
                return;

            var desiredPosition = _target.position + _offset;
            var t = 1f - Mathf.Exp(-_followSharpness * Time.deltaTime);

            transform.position = Vector3.Lerp(transform.position, desiredPosition, t);
        }

        private void OnDestroy() {
            _target = null;
        }
    }
}