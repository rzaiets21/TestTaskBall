using UnityEngine;

namespace TestTaskBall.game.abstraction {
    public abstract class BallBase : MonoBehaviour {
        protected float _initialRadius;
        protected float _radius;
        protected float _size;

        public virtual void SetInitialRadius(float radius) {
            _initialRadius = radius;
        }
        
        public void SetRadius(float radius) {
            _radius = radius;
            _size = _radius / _initialRadius;
            UpdateScale();
        }
        
        protected void UpdateScale() {
            transform.localScale = (_radius / _initialRadius) * Vector3.one;
        }
    }
}