using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public Text txtScore;
    public Text txtStreaks;
    public Text txtTotalShots;
    public Text txtPrompt;

    [Header("Prompt Settings")]
    public float promptDuration = 1.5f;
    public float shakeIntensity = 10f;
    public float shakeSpeed = 50f;

    Vector3 promptOriginalPos;
    Coroutine promptCoroutine;

    void Start()
    {
        if (txtPrompt != null)
        {
            promptOriginalPos = txtPrompt.rectTransform.localPosition;
            txtPrompt.text = "";
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
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScore -= OnScoreUI;
            GameManager.Instance.OnMiss -= UpdateMissUI;
            GameManager.Instance.OnBallReset -= UpdateResetUI;
        }
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

        string[] msgs = { "BRICK!", "NOPE!", "AIR BALL!" };
        ShowPrompt(msgs[Random.Range(0, msgs.Length)], Color.red, false);
    }

    void UpdateResetUI()
    {
        txtTotalShots.text = "Total Shots: " + GameManager.Instance.ShotsTaken;
    }

    void ShowPrompt(string text, Color color, bool shake)
    {
        if (txtPrompt == null) return;

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
