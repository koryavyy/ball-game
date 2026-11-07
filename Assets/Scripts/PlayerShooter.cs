using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] GameConfig config;
    [SerializeField] PlayerBall player;
    [SerializeField] Shot shotPrefab;
    [SerializeField] Transform goal;

    public bool CanAim { get; set; }

    public event Action ShotResolved;
    public event Action Overcharged;

    Shot shot;
    bool charging;
    float shotArea;
    Vector3 direction;

    void Update()
    {
        if (charging)
        {
            if (Input.GetMouseButton(0))
            {
                Charge();
            }
            else
            {
                Release();
            }
        }
        else if (CanAim && Time.timeScale > 0f && Input.GetMouseButtonDown(0)
            && !IsPointerOverUI())
        {
            Begin();
        }
    }

    static bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }

        return EventSystem.current.IsPointerOverGameObject();
    }

    void Begin()
    {
        if (!player.CanSpend(config.MinShotArea))
        {
            return;
        }

        direction = goal.position - player.transform.position;
        direction.y = 0f;
        direction.Normalize();

        shot = Instantiate(shotPrefab, player.transform.position, Quaternion.identity);
        shotArea = 0f;
        charging = true;
        CanAim = false;

        Grow(config.MinShotArea);
    }

    void Charge()
    {
        float amount = config.growRate * Time.deltaTime;

        if (!player.CanSpend(amount))
        {
            Destroy(shot.gameObject);
            charging = false;
            Overcharged?.Invoke();
            return;
        }

        Grow(amount);
    }

    void Grow(float amount)
    {
        player.Spend(amount);
        shotArea += amount;
        shot.SetArea(shotArea);

        Vector3 pos = player.transform.position
        + direction * (player.Radius + shot.Radius * GameConfig.ShotSpawnOffset);
        pos.y = PlayerBall.CastHeight;
        shot.transform.position = pos;
    }

    void Release()
    {
        charging = false;

        Vector3 toGoal = goal.position - shot.transform.position;
        toGoal.y = 0f;
        float range = toGoal.magnitude + 1f;

        shot.Resolved += () => ShotResolved?.Invoke();
        shot.Launch(direction, config.shotSpeed, config.infectionMultiplier, range);
        shot = null;
    }
}
