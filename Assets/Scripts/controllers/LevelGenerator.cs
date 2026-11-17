/*
   // This level generator was developed in collaboration with OpenAI's ChatGPT.
   // AI assistance was used for algorithm design, mathematical calculations,
   // implementation, and generation parameter tuning.
*/
using System;
using System.Collections.Generic;
using TestTaskBall.models.level;
using UnityEngine;

namespace TestTaskBall.controllers {
    public class LevelGenerator {
        private const float Epsilon = 0.00001f;
        private readonly LevelGenerationSettings _s;

        public LevelGenerator(LevelGenerationSettings settings) {
            _s = settings ?? throw new ArgumentNullException(nameof(settings));
            ValidateSettings();
        }

        // Same seed and settings produce the same result in the same runtime.
        // No scene objects or physics queries are created during generation.
        public LevelData Generate(int seed) {
            ValidateSettings();
            var random = new System.Random(seed);
            int steps = Mathf.FloorToInt((_s.MaxStartRadius - _s.MinStartRadius)
                / _s.StartRadiusStep + Epsilon);

            for (int attempt = 1; attempt <= _s.MaxLevelAttempts; attempt++) {
                if (!TryScatter(random, out var obstacles))
                    continue;

                // Do not use binary search: solvability is not monotonic in radius.
                for (int i = 0; i <= steps; i++) {
                    float radius = _s.MinStartRadius + i * _s.StartRadiusStep;
                    if (!TrySimulate(obstacles, radius, out var solutionCount, out float spent))
                        continue;

                    if (solutionCount < _s.MinShots)
                        continue;

                    float required = _s.CriticalRadius + spent * (1f + _s.Reserve);
                    if (radius + Epsilon < required)
                        continue;

                    var decorations = GenerateDecorations(seed, obstacles, radius);

                    return new LevelData(obstacles.ToArray(), radius, _s.Length, decorations);
                }
            }

            throw new InvalidOperationException(
                $"No valid level for seed {seed} after {_s.MaxLevelAttempts} attempts. " +
                "Adjust density, field width, shot size or infection multiplier.");
        }

        // Can also be called independently for a hand-authored/baked level.
        // Decoration placement is best-effort: a crowded field is not a gameplay failure.
        public Vector2[] GenerateDecorations(int seed,
            IReadOnlyList<Vector2> gameplayObstacles, float initialRadius) {
            if (gameplayObstacles == null)
                throw new ArgumentNullException(nameof(gameplayObstacles));
            if (float.IsNaN(initialRadius) || float.IsInfinity(initialRadius) || initialRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(initialRadius));

            int count = Mathf.Clamp(_s.DecorativeObstacleCount, 0, 5000);
            if (count == 0) return Array.Empty<Vector2>();

            // All obstacles have the same circular footprint in this example.
            float r = _s.ObstacleRadius;
            float protectedHalfWidth = Mathf.Max(
                initialRadius + _s.Clearance,
                Mathf.Max(_s.MaxShotRadius, _s.HalfFieldWidth + r));

            // This is the allowed interval for the CENTER of a decorative obstacle.
            float minX = protectedHalfWidth + r + _s.DecorationRoadGap;
            float maxX = _s.EnvironmentHalfWidth - r;
            float minZ = r;
            float maxZ = _s.Length - r;

            if (maxX <= minX || maxZ <= minZ)
                return Array.Empty<Vector2>();

            float minDistance = 2f * r + _s.DecorativeObstacleGap;
            float minDistanceSq = minDistance * minDistance;
            float gameplayDistance = 2f * r + Mathf.Max(_s.ObstacleGap, _s.DecorativeObstacleGap);
            float gameplayDistanceSq = gameplayDistance * gameplayDistance;

            // Separate random stream: decorations do not change the gameplay seed/layout.
            var random = new System.Random(seed ^ 0x13579BDF);
            var result = new List<Vector2>(count);

            for (int attempt = 0; attempt < count * 50 && result.Count < count; attempt++) {
                float sign = random.Next(0, 2) == 0 ? -1f : 1f;
                var p = new Vector2(sign * Range(random, minX, maxX),
                    Range(random, minZ, maxZ));

                if (HasOverlap(p, gameplayObstacles, gameplayDistanceSq) ||
                    HasOverlap(p, result, minDistanceSq))
                    continue;

                result.Add(p);
            }

            return result.ToArray();
        }

        private static bool HasOverlap(Vector2 point,
            IReadOnlyList<Vector2> obstacles, float minDistanceSq) {
            for (int i = 0; i < obstacles.Count; i++)
                if ((point - obstacles[i]).sqrMagnitude < minDistanceSq)
                    return true;

            return false;
        }

        private bool TryScatter(System.Random random, out List<Vector2> obstacles) {
            obstacles = new List<Vector2>(_s.ObstacleCount);
            float minZ = _s.MaxStartRadius + _s.ObstacleRadius + _s.Clearance
                         + _s.StopGap + 0.5f;
            float maxZ = _s.Length - 2f;
            float distance = 2f * _s.ObstacleRadius + _s.ObstacleGap;
            float minDistanceSq = distance * distance;

            for (int attempt = 0; attempt < _s.ObstacleCount * 100; attempt++) {
                var p = new Vector2(Range(random, -_s.HalfFieldWidth, _s.HalfFieldWidth),
                    Range(random, minZ, maxZ));
                bool overlaps = false;
                foreach (var other in obstacles) {
                    if ((p - other).sqrMagnitude < minDistanceSq) {
                        overlaps = true;
                        break;
                    }
                }

                if (overlaps) continue;
                obstacles.Add(p);
                if (obstacles.Count == _s.ObstacleCount) return true;
            }

            return false;
        }

        // Greedy reference playthrough. Each accepted shot destroys the current blocker.
        // A failure does not prove no solution exists; the generator may try another radius/layout.
        public bool TrySimulate(IReadOnlyList<Vector2> obstacles, float initialRadius,
            out int solutionCount, out float spent) {
            solutionCount = 0;
            var alive = new List<Vector2>(obstacles);
            spent = 0f;
            float radius = initialRadius;
            float playerZ = 0f;
            if (radius <= _s.CriticalRadius) return false;

            while (true) {
                int blocker = FindFirstHit(alive, playerZ, _s.Length,
                    radius + _s.Clearance, out float entryZ);
                if (blocker < 0) return true;

                playerZ = Mathf.Max(playerZ, entryZ - _s.StopGap);
                if (!TryPickShot(alive, blocker, playerZ, radius,
                        out float shot, out Vector2 center))
                    return false;

                float reach = _s.InfectionRadius(shot);
                int destroyed = alive.RemoveAll(p => InRange(p, center, reach));
                if (destroyed == 0) return false;

                solutionCount++;
                radius -= shot;
                spent += shot;
            }
        }

        private bool TryPickShot(List<Vector2> obstacles, int blocker, float playerZ,
            float playerRadius, out float bestShot, out Vector2 bestCenter) {
            bestShot = 0f;
            bestCenter = default;
            float bestScore = -1f;
            float origin = _s.ShotOriginZ(playerZ, playerRadius);
            float blockingWidth = playerRadius + _s.Clearance + _s.ObstacleRadius;
            float maxShot = Mathf.Min(_s.MaxShotRadius, playerRadius - _s.CriticalRadius);
            int steps = Mathf.FloorToInt((_s.MaxShotRadius - _s.MinShotRadius)
                / _s.ShotRadiusStep + Epsilon);

            for (int i = 0; i <= steps; i++) {
                float shot = _s.MinShotRadius + i * _s.ShotRadiusStep;
                if (shot > maxShot + Epsilon || playerRadius - shot <= _s.CriticalRadius + Epsilon)
                    break;

                int hit = FindFirstHit(obstacles, origin, _s.Length, shot, out _);
                if (hit < 0) continue;

                Vector2 center = obstacles[hit];
                float reach = _s.InfectionRadius(shot);
                if (!InRange(obstacles[blocker], center, reach)) continue;

                // Prefer clearing more potentially blocking obstacles per unit spent.
                int gain = 0;
                foreach (var p in obstacles)
                    if (p.y >= playerZ && Mathf.Abs(p.x) <= blockingWidth && InRange(p, center, reach))
                        gain++;

                float score = gain / shot;
                if (score <= bestScore + Epsilon) continue;
                bestScore = score;
                bestShot = shot;
                bestCenter = center;
            }

            return bestShot > 0f;
        }

        // Analytic sweep along +Z, against circular XZ obstacle footprints.
        // Returns the first center position at which the moving circle touches an obstacle.
        public int FindFirstHit(IReadOnlyList<Vector2> obstacles, float fromZ,
            float toZ, float movingRadius, out float hitZ) {
            int index = -1;
            hitZ = toZ;
            float sum = movingRadius + _s.ObstacleRadius;
            float sumSq = sum * sum;

            for (int i = 0; i < obstacles.Count; i++) {
                Vector2 p = obstacles[i];
                float d = sumSq - p.x * p.x;
                if (d < 0f) continue;
                float halfInterval = Mathf.Sqrt(d);
                if (p.y + halfInterval < fromZ) continue;

                float entry = Mathf.Max(fromZ, p.y - halfInterval);
                if (entry > toZ || (index >= 0 && entry >= hitZ)) continue;
                index = i;
                hitZ = entry;
            }

            return index;
        }

        private static bool InRange(Vector2 p, Vector2 center, float radius) =>
            (p - center).sqrMagnitude <= radius * radius;

        private static float Range(System.Random random, float min, float max) =>
            min + (max - min) * (float)random.NextDouble();

        private void ValidateSettings() {
            if (_s.ObstacleCount <= 0 || _s.ObstacleCount > 500 || _s.MaxLevelAttempts <= 0 ||
                _s.MinShots < 1 || _s.MinShots > _s.ObstacleCount ||
                _s.ObstacleRadius <= 0f || _s.ObstacleGap < 0f || _s.HalfFieldWidth <= 0f ||
                _s.EnvironmentHalfWidth <= 0f || _s.DecorativeObstacleCount < 0 ||
                _s.DecorativeObstacleCount > 5000 || _s.DecorativeObstacleGap < 0f ||
                _s.DecorationRoadGap < 0f ||
                _s.Clearance < 0f || _s.CriticalRadius <= 0f ||
                _s.MinStartRadius <= _s.CriticalRadius || _s.MaxStartRadius < _s.MinStartRadius ||
                _s.StartRadiusStep <= 0f || _s.MinShotRadius <= 0f ||
                _s.MaxShotRadius < _s.MinShotRadius || _s.ShotRadiusStep <= 0f ||
                _s.InfectionMultiplier <= 0f || _s.Reserve < 0f || _s.StopGap <= _s.MaxShotRadius ||
                _s.Length - 2f <= _s.MaxStartRadius + _s.ObstacleRadius + _s.Clearance + _s.StopGap + 0.5f)
                throw new ArgumentException("Invalid level-generation settings.");
        }
    }
}