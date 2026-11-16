using TestTaskBall.components;
using TestTaskBall.game.abstraction;

namespace TestTaskBall.game {
    public class BallBullet : BallBase, IPoolable {
        public void IncreaseRadius(float radius) {
            SetRadius(_radius + radius);
        }
        
        public void Release() {
            gameObject.SetActive(false);
            SetRadius(0f);
        }
    }
}
