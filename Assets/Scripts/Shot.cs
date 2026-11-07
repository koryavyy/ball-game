using System;
using System.Collections;
using UnityEngine;

public class Shot : MonoBehaviour
{
    public event Action Resolved;

    public float Radius { get { return transform.localScale.x * 0.5f; } }

    Vector3 direction;
    float speed;
    float multiplier;
    float maxDistance;
    float traveled;
    bool flying;

    public void SetArea(float area)
    {
        float radius = Mathf.Sqrt(area / Mathf.PI);
        transform.localScale = Vector3.one * (radius * 2f);
    }

    public void Launch(Vector3 dir, float shotSpeed, float infectionMultiplier, float range)
    {
        direction = new Vector3(dir.x, 0f, dir.z).normalized;
        speed = shotSpeed;
        multiplier = infectionMultiplier;
        maxDistance = range;
        traveled = 0f;
        flying = true;
    }

    void Update()
    {
        if (!flying)
        {
            return;
        }

        float step = speed * Time.deltaTime;
        Vector3 pos = transform.position;

        Obstacle first = null;
        Vector3 hitCenter = pos;

        foreach (var col in Physics.OverlapSphere(pos, Radius))
        {
            var o = col.GetComponent<Obstacle>();

            if (o != null)
            {
                first = o;
                break;
            }
        }

        if (first == null)
        {
            float best = float.MaxValue;

            foreach (var hit in Physics.SphereCastAll(pos, Radius, direction, step))
            {
                var o = hit.collider.GetComponent<Obstacle>();

                if (o != null && hit.distance < best)
                {
                    best = hit.distance;
                    first = o;
                    hitCenter = pos + direction * hit.distance;
                }
            }
        }

        if (first != null)
        {
            Explode(hitCenter);
            return;
        }

        transform.position = pos + direction * step;
        traveled += step;

        if (traveled >= maxDistance)
        {
            flying = false;
            Finish();
        }
    }

    void Explode(Vector3 point)
    {
        flying = false;
        transform.position = point;
        float radius = Radius * multiplier;

        foreach (var col in Physics.OverlapSphere(point, radius))
        {
            var obstacle = col.GetComponent<Obstacle>();

            if (obstacle == null)
            {
                continue;
            }

            float dist = Vector3.Distance(point, col.transform.position);
            obstacle.Infect(Mathf.Clamp01(dist / radius) * 0.25f);
        }

        StartCoroutine(RingRoutine(radius));
    }

    IEnumerator RingRoutine(float radius)
    {
        float from = transform.localScale.x;
        const float duration = 0.2f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.one * Mathf.Lerp(from, radius * 2f, Mathf.Clamp01(t / duration));
            yield return null;
        }

        Finish();
    }

    void Finish()
    {
        if (Resolved != null)
        {
            Resolved();
        }

        Destroy(gameObject);
    }
}
