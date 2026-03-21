using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public BallController ball;

    [Header("Reset Timing")]
    public float resetDelay = 2f;
    public float outOfBoundsY = -5f;

    [Header("Timer Mode")]
    public float gameDuration = 60f;

    // Score state
    public int Score { get; private set; }
    public int Streak { get; private set; }
    public int BestStreak { get; private set; }
    public int ShotsTaken { get; private set; }
    public int ShotsMade { get; private set; }

    // Timer state
    public float TimeRemaining { get; private set; }
    public bool GameActive { get; private set; }
    public bool GameOver { get; private set; }

    // Events
    public event Action<int, int, bool> OnScore;
    public event Action OnMiss;
    public event Action OnBallReset;
    public event Action OnGameOver;
    public event Action OnGameStart;
    public event Action OnThrow;
    public event Action OnPityMode;

    [Header("Pity Mode")]
    public Transform hoopTransform;
    public int missesForPity = 3;
    int consecutiveMisses;
    Vector3 originalHoopPos;
    bool pityActive;

    // Internal
    float resetTimer;
    bool waitingForReset;
    bool scoredThisShot;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        TimeRemaining = gameDuration;
        GameActive = false;
        GameOver = false;
        if (hoopTransform != null)
            originalHoopPos = hoopTransform.position;
    }

    void Update()
    {
        if (!GameActive && Input.GetKeyDown(KeyCode.R))
        {
            if (GameOver)
                RestartGame();
            else
                StartGame();
        }

        if (GameActive)
        {
            TimeRemaining -= Time.deltaTime;
            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                EndGame();
            }
        }

        if (waitingForReset)
        {
            resetTimer -= Time.deltaTime;
            if (resetTimer <= 0f)
            {
                DoReset();
            }
        }

        if (ball != null && ball.IsInFlight() && ball.transform.position.y < outOfBoundsY)
        {
            DoReset();
        }
    }

    void StartGame()
    {
        GameActive = true;
        TimeRemaining = gameDuration;
        Score = 0;
        Streak = 0;
        BestStreak = 0;
        ShotsTaken = 0;
        ShotsMade = 0;
        consecutiveMisses = 0;
        pityActive = false;
        if (hoopTransform != null)
            hoopTransform.position = originalHoopPos;
        OnGameStart?.Invoke();
    }

    void EndGame()
    {
        GameActive = false;
        GameOver = true;

        if (ball != null)
            ball.ResetBall();

        OnGameOver?.Invoke();
    }

    public void RestartGame()
    {
        GameOver = false;
        StartGame();
    }

    public void RegisterThrow()
    {
        if (!GameActive) return;
        ShotsTaken++;
        scoredThisShot = false;
        OnThrow?.Invoke();
    }

    public void StartResetTimer()
    {
        if (!waitingForReset)
        {
            waitingForReset = true;
            resetTimer = resetDelay;
        }
    }

    public void RegisterScore(bool wasSwish = false)
    {
        if (scoredThisShot) return;

        scoredThisShot = true;
        ShotsMade++;
        consecutiveMisses = 0;

        int points = wasSwish ? 3 : 2;
        Score += points;
        Streak++;
        if (Streak > BestStreak) BestStreak = Streak;

        OnScore?.Invoke(Score, Streak, wasSwish);

        waitingForReset = true;
        resetTimer = resetDelay;
    }

    void DoReset()
    {
        waitingForReset = false;

        if (!scoredThisShot)
        {
            Streak = 0;
            consecutiveMisses++;
            OnMiss?.Invoke();

            // Pity mode: move hoop closer after consecutive misses
            if (consecutiveMisses >= missesForPity && !pityActive && hoopTransform != null)
            {
                pityActive = true;
                Vector3 newPos = hoopTransform.position;
                newPos.z = 5f;
                hoopTransform.position = newPos;
                if (ball != null) ball.SetPityMode(true);
                OnPityMode?.Invoke();
            }
        }

        // Reset pity mode after camera zoom is done
        if (pityActive && scoredThisShot)
        {
            pityActive = false;
            if (hoopTransform != null)
                hoopTransform.position = originalHoopPos;
            if (ball != null) ball.SetPityMode(false);
        }

        if (ball != null)
        {
            ball.ResetBall();
        }

        OnBallReset?.Invoke();
    }
}
