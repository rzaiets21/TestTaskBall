using TestTaskBall.components;
using UnityEngine;

namespace TestTaskBall.game {
    public class ObstacleView : MonoBehaviour, IPoolable {
        public void Release() {
            gameObject.SetActive(false);
        }

        public void Infect() {
            Release();
        }
    }
}