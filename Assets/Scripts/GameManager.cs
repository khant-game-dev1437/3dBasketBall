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

    // Score state
    public int Score { get; private set; }
    public int Streak { get; private set; }
    public int BestStreak { get; private set; }
    public int ShotsTaken { get; private set; }
    public int ShotsMade { get; private set; }

    // Events
    public event Action<int, int> OnScore;
    public event Action OnMiss;
    public event Action OnBallReset;

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

    void Update()
    {
        if (waitingForReset)
        {
            resetTimer -= Time.deltaTime;
            if (resetTimer <= 0f)
            {
                DoReset();
            }
        }

        // Safety reset if ball falls out of world
        if (ball != null && ball.IsInFlight() && ball.transform.position.y < outOfBoundsY)
        {
            DoReset();
        }
    }

    public void RegisterThrow()
    {
        ShotsTaken++;
        scoredThisShot = false;
    }

    public void StartResetTimer()
    {
        if (!waitingForReset)
        {
            waitingForReset = true;
            resetTimer = resetDelay;
        }
    }

    public void RegisterScore()
    {
        if (scoredThisShot) return;

        scoredThisShot = true;
        ShotsMade++;

        Score += 2;
        Streak++;
        if (Streak > BestStreak) BestStreak = Streak;

        Debug.Log("SCORE! Total: " + Score + " Streak: " + Streak);
        OnScore?.Invoke(Score, Streak);

        // Start reset timer after scoring
        waitingForReset = true;
        resetTimer = resetDelay;
    }

    void DoReset()
    {
        waitingForReset = false;

        if (!scoredThisShot)
        {
            Streak = 0;
            OnMiss?.Invoke();
        }

        if (ball != null)
        {
            ball.ResetBall();
        }

        OnBallReset?.Invoke();
    }
}
