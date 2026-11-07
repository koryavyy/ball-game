using System;
using System.Collections;
using UnityEngine;

public class PlayerBall : MonoBehaviour
{
    public const float CastHeight = 0.5f;

    [SerializeField] GameConfig config;

    public float Area { get; private set; }
    public float Radius { get { return Mathf.Sqrt(Area / Mathf.PI); } }

    public void Init(float area)
    {
        SetArea(area);
    }

    public bool CanSpend(float amount)
    {
        return Area - amount > config.CriticalArea;
    }

    public void Spend(float amount)
    {
        SetArea(Area - amount);
    }

    void SetArea(float area)
    {
        Area = Mathf.Max(0f, area);
        transform.localScale = Vector3.one * (Radius * 2f);

        Vector3 p = transform.position;
        p.y = Radius;
        transform.position = p;
    }

    public IEnumerator Advance(Vector3 goal, Action<bool> done)
    {
        while (true)
        {
            Vector3 pos = transform.position;
            Vector3 to = goal - pos;
            to.y = 0f;
            float dist = to.magnitude;

            if (dist < config.goalReachDistance)
            {
                done(true);
                yield break;
            }

            Vector3 dir = to / dist;
            float step = Mathf.Min(config.hopLength, dist);

            if (IsBlocked(pos, dir, step))
            {
                done(false);
                yield break;
            }

            yield return Hop(pos, pos + dir * step);
        }
    }

    bool IsBlocked(Vector3 pos, Vector3 dir, float distance)
    {
        Vector3 origin = new Vector3(pos.x, CastHeight, pos.z);
        float radius = Radius * GameConfig.CastFactor;

        foreach (var col in Physics.OverlapSphere(origin, radius))
        {
            if (col.GetComponent<Obstacle>() != null)
            {
                return true;
            }
        }

        foreach (var hit in Physics.SphereCastAll(origin, radius, dir, distance))
        {
            if (hit.collider.GetComponent<Obstacle>() != null)
            {
                return true;
            }
        }

        return false;
    }

    IEnumerator Hop(Vector3 from, Vector3 to)
    {
        float baseScale = Radius * 2f;
        float t = 0f;

        while (t < config.hopTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / config.hopTime);

            float scale = baseScale * (1f + 0.3f * Mathf.Sin(k * Mathf.PI));
            Vector3 p = Vector3.Lerp(from, to, k);
            p.y = scale * 0.5f;

            transform.position = p;
            transform.localScale = Vector3.one * scale;
            yield return null;
        }

        transform.position = to;
        SetArea(Area); 
    }
}
