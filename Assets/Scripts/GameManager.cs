using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private LevelAnalyzer levelAnalyzer;
    [SerializeField] private GameConfig config;
    [SerializeField] private PlayerBall player;
    [SerializeField] private PlayerShooter shooter;
    [SerializeField] private Transform goal;

    [Header("Generation")]
    [Tooltip("Скільки разів перегенерувати рівень, якщо він непрохідний")]
    [SerializeField] private int maxGenerationAttempts = 10;

    public event Action<string> GameEnded;
    public static bool SkipStartScreen { get; private set; }

    private float startArea;
    private bool gameStarted;
    private bool gameEnded;

    public float SizeNormalized
    {
        get
        {
            float usable = startArea - config.CriticalArea;

            if (usable <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01((player.Area - config.CriticalArea) / usable);
        }
    }

    private void Start()
    {
        shooter.CanAim = false;

        if (SkipStartScreen)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        SkipStartScreen = false;
        StartCoroutine(InitializeGame());
    }

    private IEnumerator InitializeGame()
    {
        shooter.CanAim = false;

        LevelAnalysisResult result = null;

        for (int attempt = 1; attempt <= maxGenerationAttempts; attempt++)
        {
            levelGenerator.Generate(player.transform, goal);

            yield return null;

            result = levelAnalyzer.Analyze(player.transform, goal);

            if (result.IsPlayable)
            {
                break;
            }

            Debug.LogWarning($"Attempt {attempt}/{maxGenerationAttempts}: level is not playable ({result.ErrorMessage})");
        }

        if (result == null || !result.IsPlayable)
        {
            Debug.LogError("Level generation failed: " + result?.ErrorMessage);
            yield break;
        }

        startArea = result.RequiredStartArea;
        player.Init(startArea);

        shooter.ShotResolved += OnShotResolved;
        shooter.Overcharged += OnOvercharged;

        gameStarted = true;
        shooter.CanAim = true;

        Debug.Log(
            $"Game initialized. Start area: {startArea:F2}, " +
            $"min shots: {result.ShotsNeeded}");
    }

    private void OnShotResolved()
    {
        if (!gameStarted || gameEnded)
        {
            return;
        }

        StartCoroutine(AdvanceRoutine());
    }

    private IEnumerator AdvanceRoutine()
    {
        shooter.CanAim = false;

        yield return new WaitForSeconds(config.settleDelay);

        bool reached = false;

        yield return player.Advance(goal.position, result => reached = result);

        if (reached)
        {
            End("Перемога!");
        }
        else if (!player.CanSpend(config.MinShotArea))
        {
            End("Програш: не вистачило розміру кулі");
        }
        else
        {
            shooter.CanAim = true;
        }
    }

    private void OnOvercharged()
    {
        End("Програш: куля повністю перекачалась у постріл");
    }

    private void End(string message)
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;
        gameStarted = false;
        shooter.CanAim = false;

        Debug.Log(message);
        GameEnded?.Invoke(message);
    }

    public void Restart()
    {
        SkipStartScreen = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        if (shooter == null)
        {
            return;
        }

        shooter.ShotResolved -= OnShotResolved;
        shooter.Overcharged -= OnOvercharged;
    }
}
