using UnityEngine;

namespace TestTaskBall.game.abstraction {
    public abstract class BallBase : MonoBehaviour {
        public float Radius => _radius;
        
        protected float _initialRadius;
        protected float _radius;
        protected float _size;

        public virtual void SetInitialRadius(float radius) {
            _initialRadius = radius;
        }
        
        public void SetRadius(float radius) {
            _radius = radius;
            _size = _radius / _initialRadius;
            UpdateScaleAndPosition();
        }
        
        protected void UpdateScaleAndPosition() {
            transform.localScale = (_radius / _initialRadius) * Vector3.one;
            var position = transform.localPosition;
            transform.localPosition = new Vector3(position.x, _size / 2f, position.z);
        }
    }
}