using System;

namespace TestTaskBall.models.level {
    [Serializable]
    public sealed class LevelGenerationSettings
    {
        public float Length = 36f;
        public float HalfFieldWidth = 1.25f;
        public int ObstacleCount = 30;

        // Visual field around the playable corridor. Width is measured from X = 0.
        public float EnvironmentHalfWidth = 7f;
        public float DecorativeObstacleGap = 0.15f;
        public float DecorationRoadGap = 0.15f;
        public float ObstacleRadius = 0.2f;
        public float ObstacleGap = 0.08f;
        public float Clearance = 0.04f;
        public float CriticalRadius = 0.15f;
        public float MinStartRadius = 0.4f;
        public float MaxStartRadius = 3f;
        public float StartRadiusStep = 0.05f;
        public float MinShotRadius = 0.1f;
        public float MaxShotRadius = 1.1f;
        public float ShotRadiusStep = 0.025f;
        public float InfectionMultiplier = 6f;
        public float StopGap = 1.2f;
        public float Reserve = 0.2f;
        public int MinShots = 3;
        public int MaxLevelAttempts = 64;
        public int DecorativeObstacleCount = 100;

        public float InfectionRadius(float shotRadius) => shotRadius * InfectionMultiplier;
        public float RoadWidth(float playerRadius) => 2f * (playerRadius + Clearance);
        public float ShotOriginZ(float playerZ, float radiusBeforeShot) => playerZ + radiusBeforeShot;
    }
}