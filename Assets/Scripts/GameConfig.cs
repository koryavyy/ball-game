using UnityEngine;

[CreateAssetMenu(menuName = "Prototype/Game Config", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Постріл")]
    [Tooltip("Площа, що переходить з гравця в постріл за секунду утримання")]
    public float growRate = 1f;
    [Tooltip("Радіус зараження = радіус пострілу * це число")]
    public float infectionMultiplier = 4f;
    public float minShotRadius = 0.15f;
    public float shotSpeed = 14f;

    [Header("Гравець")]
    [Tooltip("Менший радіус - програш")]
    public float criticalRadius = 0.25f;
    [Tooltip("Запас розміру на проходження (1.2 = 20%)")]
    public float reserve = 1.2f;
    public float hopLength = 1.2f;
    public float hopTime = 0.28f;
    public float goalReachDistance = 0.35f;
    public float settleDelay = 0.3f;

    [Header("Аналіз рівня (поправка на людину)")]
    [Tooltip("Крок розміру пострілу, який реально може вибрати гравець (~100 мс утримання)")]
    public float shotStep = 0.1f;
    [Tooltip("Скільки гравець в середньому 'перекачає' зверху")]
    public float shotSlack = 0.05f;

    public const float CastFactor = 0.98f;
    public const float ShotSpawnOffset = 0.3f;

    [Header("Двері")]
    public float doorOpenDistance = 5f;

    public float MinShotArea { get { return Mathf.PI * minShotRadius * minShotRadius; } }
    public float CriticalArea { get { return Mathf.PI * criticalRadius * criticalRadius; } }
}
