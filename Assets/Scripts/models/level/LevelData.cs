using System;
using UnityEngine;

namespace TestTaskBall.models.level {
    [Serializable]
    public class LevelData {
        public Vector2[] Obstacles => _obstacles;
        public Vector2[] DecorativeObstacles => _decorativeObstacles;
        public float InitialRadius => _initialRadius;
        public float FinishPointDistance => _finishPointDistance;
        public float InfectionMultiplier => _infectionMultiplier;
        
        [SerializeField] private Vector2[] _obstacles;
        [SerializeField] private Vector2[] _decorativeObstacles;
        [SerializeField] private float _initialRadius;
        [SerializeField] private float _finishPointDistance;
        [SerializeField] private float _infectionMultiplier;

        public LevelData(Vector2[] obstacles, float initialRadius, float finishPointDistance, Vector2[] decorativeObstacles = null) {
            _obstacles = obstacles;
            _decorativeObstacles = decorativeObstacles ?? Array.Empty<Vector2>();
            _initialRadius = initialRadius;
            _finishPointDistance = finishPointDistance;
        }
    }
}