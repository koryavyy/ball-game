using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoxCollider spawnArea;
    [SerializeField] private Obstacle obstaclePrefab;
    [SerializeField] private Transform generatedParent;

    [Header("Density")]
    [Range(0.05f, 0.5f)]
    [SerializeField] private float targetCoverage = 0.22f;
    [SerializeField] private float minRadius = 0.16f;
    [SerializeField] private float maxRadius = 0.23f;
    [SerializeField] private float minGap = 0.04f;

    [Header("Clear Zones")]
    [SerializeField] private float playerClearance = 1.2f;
    [SerializeField] private float goalClearance = 1.3f;

    [Header("Random")]
    [SerializeField] private bool randomSeed = true;
    [SerializeField] private int seed = 7;

    [Header("Generation")]
    [SerializeField] private int attemptsPerObstacle = 35;
    [SerializeField] private float obstacleCenterY = 0.5f;

    private struct Placement
    {
        public Vector2 position;
        public float radius;
        public float rotation;
    }

    public void Generate(Transform player, Transform goal)
    {
        ValidateSettings();
        ClearGenerated();

        int actualSeed = randomSeed ? Guid.NewGuid().GetHashCode() : seed;
        var random = new Random(actualSeed);
        Bounds bounds = spawnArea.bounds;

        int targetCount = CalculateTargetCount(bounds);
        var placements = new List<Placement>();

        Vector2 playerPosition = ToXZ(player.position);
        Vector2 goalPosition = ToXZ(goal.position);

        int maxAttempts = targetCount * attemptsPerObstacle;
        int attempts = 0;

        while (placements.Count < targetCount && attempts < maxAttempts)
        {
            attempts++;

            float radius = RandomRange(random, minRadius, maxRadius);
            Vector2 position = new Vector2(
                RandomRange(random, bounds.min.x + radius, bounds.max.x - radius),
                RandomRange(random, bounds.min.z + radius, bounds.max.z - radius));

            if (!IsOutsideClearZones(position, radius, playerPosition, goalPosition))
            {
                continue;
            }

            if (!HasEnoughSpace(position, radius, placements))
            {
                continue;
            }

            placements.Add(new Placement
            {
                position = position,
                radius = radius,
                rotation = RandomRange(random, 0f, 360f)
            });
        }

        foreach (Placement placement in placements)
        {
            Spawn(placement);
        }

        Debug.Log($"Level generated | Seed: {actualSeed} | Obstacles: {placements.Count}/{targetCount}");

        if (placements.Count < targetCount)
        {
            Debug.LogWarning("Could not reach target density. Try reducing Target Coverage or Min Gap.");
        }
    }

    private int CalculateTargetCount(Bounds bounds)
    {
        float fieldArea = bounds.size.x * bounds.size.z;
        float averageRadius = (minRadius + maxRadius) * 0.5f;
        float averageObstacleArea = Mathf.PI * averageRadius * averageRadius;

        return Mathf.RoundToInt(fieldArea * targetCoverage / averageObstacleArea);
    }

    private bool IsOutsideClearZones(
        Vector2 position, float radius, Vector2 playerPosition, Vector2 goalPosition)
    {
        return Vector2.Distance(position, playerPosition) >= playerClearance + radius
            && Vector2.Distance(position, goalPosition) >= goalClearance + radius;
    }

    private bool HasEnoughSpace(Vector2 position, float radius, List<Placement> placements)
    {
        foreach (Placement other in placements)
        {
            float minDistance = radius + other.radius + minGap;

            if ((position - other.position).sqrMagnitude < minDistance * minDistance)
            {
                return false;
            }
        }

        return true;
    }

    private void Spawn(Placement placement)
    {
        var position = new Vector3(placement.position.x, obstacleCenterY, placement.position.y);

        Obstacle obstacle = Instantiate(
            obstaclePrefab,
            position,
            Quaternion.Euler(0f, placement.rotation, 0f),
            generatedParent);

        obstacle.transform.localScale = Vector3.one * (placement.radius * 2f);
    }

    private void ClearGenerated()
    {
        for (int i = generatedParent.childCount - 1; i >= 0; i--)
        {
            GameObject child = generatedParent.GetChild(i).gameObject;

            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }

    private void ValidateSettings()
    {
        if (spawnArea == null || obstaclePrefab == null || generatedParent == null)
        {
            throw new InvalidOperationException("LevelGenerator: assign all references in Inspector.");
        }

        if (minRadius <= 0f || maxRadius < minRadius)
        {
            throw new InvalidOperationException("Invalid radius settings.");
        }
    }

    private static Vector2 ToXZ(Vector3 position)
    {
        return new Vector2(position.x, position.z);
    }

    private static float RandomRange(Random random, float min, float max)
    {
        return min + (float)random.NextDouble() * (max - min);
    }
}
