using System.Collections.Generic;
using UnityEngine;

public class LevelAnalyzer : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    [SerializeField] private Transform obstaclesParent;
    [SerializeField] private float areaSearchStep = 0.05f;
    [SerializeField] private float maxStartArea = 8f;
    [SerializeField] private int maxShots = 100;

    private struct Circle
    {
        public Vector2 center;
        public float radius;
    }

    public LevelAnalysisResult Analyze(Transform player, Transform goal)
    {
        List<Circle> obstacles = CollectObstacles();
        Vector2 start = ToXZ(player.position);
        Vector2 end = ToXZ(goal.position);

        for (float area = config.CriticalArea + areaSearchStep; area <= maxStartArea; area += areaSearchStep)
        {
            if (!TrySimulate(area, start, end, obstacles, out float spent, out int shots))
            {
                continue;
            }

            if (area - spent * config.reserve > config.CriticalArea)
            {
                return LevelAnalysisResult.Playable(area, shots);
            }
        }

        return LevelAnalysisResult.Unplayable($"No solution with start area up to {maxStartArea:F2}");
    }

    private bool TrySimulate(float area, Vector2 position, Vector2 end,
        List<Circle> allObstacles, out float spent, out int shots)
    {
        var obstacles = new List<Circle>(allObstacles);
        float startArea = area;
        shots = 0;
        spent = 0f;

        for (int i = 0; i <= maxShots; i++)
        {
            if (AdvanceUntilBlocked(ref position, end, Radius(area), obstacles))
            {
                spent = startArea - area;
                return true;
            }

            if (!TryClearPath(position, end, ref area, ref obstacles))
            {
                return false;
            }

            shots++;
        }

        return false;
    }

    private bool AdvanceUntilBlocked(ref Vector2 position, Vector2 end, float radius, List<Circle> obstacles)
    {
        while (true)
        {
            Vector2 toGoal = end - position;
            float distance = toGoal.magnitude;

            if (distance < config.goalReachDistance)
            {
                return true;
            }

            Vector2 next = position + toGoal / distance * Mathf.Min(config.hopLength, distance);

            if (IsBlocked(position, next, radius, obstacles))
            {
                return false;
            }

            position = next;
        }
    }

    private bool TryClearPath(Vector2 position, Vector2 end, ref float area, ref List<Circle> obstacles)
    {
        Vector2 direction = (end - position).normalized;
        Vector2 next = position + direction * Mathf.Min(config.hopLength, Vector2.Distance(position, end));

        for (float shotArea = config.MinShotArea;
             area - shotArea - config.shotSlack > config.CriticalArea;
             shotArea += config.shotStep)
        {
            float shotRadius = Radius(shotArea);

            float ballRadius = Radius(area - shotArea - config.shotSlack);
            Vector2 origin = position + direction * (ballRadius + shotRadius * GameConfig.ShotSpawnOffset);

            if (!TryFindImpact(origin, direction, shotRadius, obstacles, out float travel))
            {
                continue;
            }

            Vector2 impact = origin + direction * travel;
            var survivors = RemoveInfected(obstacles, impact, shotRadius * config.infectionMultiplier);

            if (IsBlocked(position, next, ballRadius, survivors))
            {
                continue;
            }

            area -= shotArea + config.shotSlack;
            obstacles = survivors;
            return true;
        }

        return false;
    }

    private static bool TryFindImpact(Vector2 origin, Vector2 dir, float shotRadius,
        List<Circle> obstacles, out float travel)
    {
        travel = float.MaxValue;
        bool found = false;

        foreach (Circle o in obstacles)
        {
            float combined = shotRadius + o.radius;
            Vector2 m = origin - o.center;
            float c = Vector2.Dot(m, m) - combined * combined;
            float t;

            if (c <= 0f)
            {
                t = 0f;
            }
            else
            {
                float b = Vector2.Dot(m, dir);
                float d = b * b - c;

                if (b > 0f || d < 0f)
                {
                    continue;
                }

                t = -b - Mathf.Sqrt(d);
            }

            if (t < travel)
            {
                travel = t;
                found = true;
            }
        }

        return found;
    }

    private static List<Circle> RemoveInfected(List<Circle> obstacles, Vector2 impact, float blastRadius)
    {
        var survivors = new List<Circle>(obstacles.Count);

        foreach (Circle o in obstacles)
        {
            float reach = blastRadius + o.radius;

            if ((o.center - impact).sqrMagnitude > reach * reach)
            {
                survivors.Add(o);
            }
        }

        return survivors;
    }

    private static bool IsBlocked(Vector2 from, Vector2 to, float playerRadius, List<Circle> obstacles)
    {
        float cast = playerRadius * GameConfig.CastFactor;

        foreach (Circle o in obstacles)
        {
            float reach = cast + o.radius;

            if (SqrDistanceToSegment(o.center, from, to) < reach * reach)
            {
                return true;
            }
        }

        return false;
    }

    private List<Circle> CollectObstacles()
    {
        var result = new List<Circle>();

        foreach (Obstacle o in obstaclesParent.GetComponentsInChildren<Obstacle>())
        {
            result.Add(new Circle
            {
                center = ToXZ(o.transform.position),
                radius = o.transform.lossyScale.x * 0.5f
            });
        }

        return result;
    }

    private static float SqrDistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float len = ab.sqrMagnitude;
        float t = len > 0f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / len) : 0f;
        return (p - (a + ab * t)).sqrMagnitude;
    }

    private static float Radius(float area) => Mathf.Sqrt(Mathf.Max(0f, area) / Mathf.PI);

    private static Vector2 ToXZ(Vector3 v) => new Vector2(v.x, v.z);
}
