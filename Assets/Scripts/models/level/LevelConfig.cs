using UnityEngine;

namespace TestTaskBall.models.level {
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/Level Config")]
    public sealed class LevelConfig : ScriptableObject {
        [SerializeField] private LevelGenerationSettings _settings = new();

        [SerializeField] private int _seed = 42;

        [SerializeField] private LevelData _data;

        public LevelGenerationSettings Settings => _settings;
        public LevelData Data => _data;
        public int Seed => _seed;

        public void SetData(LevelData data) {
            _data = data;
        }

        public void SetSeed(int seed) {
            _seed = seed;
        }
    }
}