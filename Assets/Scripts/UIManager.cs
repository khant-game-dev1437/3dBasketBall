using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public Text txtScore;
    public Text txtStreaks;
    public Text txtTotalShots;
    public Text txtPrompt;
    public Text txtTimer;

    [Header("Prompt Settings")]
    public float promptDuration = 1.5f;
    public float shakeIntensity = 10f;
    public float shakeSpeed = 50f;

    Vector3 promptOriginalPos;
    Coroutine promptCoroutine;
    bool gameOverShown;

    void Start()
    {
        if (txtPrompt != null)
        {
            promptOriginalPos = txtPrompt.rectTransform.localPosition;
            txtPrompt.text = "Press R to Start!";
            txtPrompt.fontSize = 60;
            txtPrompt.color = Color.green;
        }
    }

    void OnEnable()
    {
        StartCoroutine(Subscribe());
    }

    IEnumerator Subscribe()
    {
        yield return null;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScore += OnScoreUI;
            GameManager.Instance.OnMiss += UpdateMissUI;
            GameManager.Instance.OnBallReset += UpdateResetUI;
            GameManager.Instance.OnGameOver += OnGameOver;
            GameManager.Instance.OnGameStart += OnGameStart;
            GameManager.Instance.OnThrow += OnThrowUI;
            GameManager.Instance.OnPityMode += OnPityMode;
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScore -= OnScoreUI;
            GameManager.Instance.OnMiss -= UpdateMissUI;
            GameManager.Instance.OnBallReset -= UpdateResetUI;
            GameManager.Instance.OnGameOver -= OnGameOver;
            GameManager.Instance.OnGameStart -= OnGameStart;
            GameManager.Instance.OnThrow -= OnThrowUI;
            GameManager.Instance.OnPityMode -= OnPityMode;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        // Update timer
        if (txtTimer != null && GameManager.Instance.GameActive)
        {
            float t = GameManager.Instance.TimeRemaining;
            int seconds = Mathf.CeilToInt(t);
            txtTimer.text = seconds.ToString();

            if (t <= 3f)
                txtTimer.color = Color.red;
            else
                txtTimer.color = Color.green;
        }

        // Restart on R key after game over
        if (GameManager.Instance.GameOver && Input.GetKeyDown(KeyCode.R))
        {
            GameManager.Instance.RestartGame();
        }
    }

    void OnGameStart()
    {
        gameOverShown = false;
        txtScore.text = "Score: 0";
        txtStreaks.text = "Streaks: 0";
        txtTotalShots.text = "Total Shots: 0";
        if (txtTimer != null)
        {
            txtTimer.text = "60";
            txtTimer.color = Color.green;
        }
        if (txtPrompt != null)
            txtPrompt.text = "";
    }

    void OnGameOver()
    {
        Debug.Log("GameOver called, prompt: " + (txtPrompt != null));
        if (gameOverShown) return;
        gameOverShown = true;

        GameManager gm = GameManager.Instance;
        float pct = gm.ShotsTaken > 0 ? (gm.ShotsMade / (float)gm.ShotsTaken * 100f) : 0f;

        string result = "TIME'S UP!\n\n" +
            "Score: " + gm.Score + "\n" +
            "Shots: " + gm.ShotsMade + "/" + gm.ShotsTaken + " (" + pct.ToString("F0") + "%)\n" +
            "Best Streak: " + gm.BestStreak + "\n\n" +
            "Press R to Restart";

        if (txtPrompt != null)
        {
            txtPrompt.text = result;
            txtPrompt.color = Color.green;
            txtPrompt.fontSize = 60;
        }

        if (txtTimer != null)
            txtTimer.text = "0";
    }

    void OnScoreUI(int score, int streak, bool wasSwish)
    {
        txtScore.text = "Score: " + score;
        txtStreaks.text = "Streaks: " + streak;
        txtTotalShots.text = "Total Shots: " + GameManager.Instance.ShotsTaken;

        string msg;
        if (wasSwish)
        {
            msg = "SWISH!";
        }
        else if (streak >= 3)
        {
            msg = streak + " IN A ROW!";
        }
        else
        {
            string[] msgs = { "NICE!", "BUCKET!", "SCORE!", "CASH!" };
            msg = msgs[Random.Range(0, msgs.Length)];
        }

        ShowPrompt(msg, Color.green, true);
    }

    void UpdateMissUI()
    {
        txtStreaks.text = "Streaks: 0";
        txtTotalShots.text = "Total Shots: " + GameManager.Instance.ShotsTaken;

        string[] msgs = { "NOOB!", "SKILL ISSUE!", "GO PLAY CANDYCRUSH!" };
        ShowPrompt(msgs[Random.Range(0, msgs.Length)], Color.red, false);
    }

    void OnPityMode()
    {
        ShowPrompt("BRUH, STOP", Color.yellow, true);
    }

    void OnThrowUI()
    {
        txtTotalShots.text = "Total Shots: " + GameManager.Instance.ShotsTaken;
    }

    void UpdateResetUI()
    {
        txtTotalShots.text = "Total Shots: " + GameManager.Instance.ShotsTaken;
    }

    void ShowPrompt(string text, Color color, bool shake)
    {
        if (txtPrompt == null) return;
        if (GameManager.Instance != null && GameManager.Instance.GameOver) return;

        if (promptCoroutine != null)
            StopCoroutine(promptCoroutine);

        txtPrompt.text = text;
        txtPrompt.color = color;
        txtPrompt.fontSize = 60;
        promptCoroutine = StartCoroutine(PromptAnimation(shake));
    }

    IEnumerator PromptAnimation(bool shake)
    {
        float timer = promptDuration;

        while (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;

            if (shake)
            {
                float offsetX = Mathf.Sin(Time.unscaledTime * shakeSpeed) * shakeIntensity;
                float offsetY = Mathf.Cos(Time.unscaledTime * shakeSpeed * 0.8f) * shakeIntensity * 0.5f;
                txtPrompt.rectTransform.localPosition = promptOriginalPos + new Vector3(offsetX, offsetY, 0f);
            }

            float alpha = timer < promptDuration * 0.3f ? timer / (promptDuration * 0.3f) : 1f;
            Color c = txtPrompt.color;
            c.a = alpha;
            txtPrompt.color = c;

            yield return null;
        }

        txtPrompt.text = "";
        txtPrompt.rectTransform.localPosition = promptOriginalPos;
    }
}
