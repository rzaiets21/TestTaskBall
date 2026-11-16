using UnityEngine;

namespace TestTaskBall.game.abstraction {
    public abstract class BallBase : MonoBehaviour {
        protected float _radius;

        protected void SetRadius(float radius) {
            _radius = radius;
            UpdateScale();
        }
        
        protected void UpdateScale() {
            transform.localScale = _radius * Vector3.one;
        }
    }
}