using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text txtScore;
    public Text txtStreaks;
    public Text txtTotalShots;

    void OnEnable()
    {
        StartCoroutine(Subscribe());
    }

    System.Collections.IEnumerator Subscribe()
    {
        yield return null;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScore += UpdateScoreUI;
            GameManager.Instance.OnMiss += UpdateMissUI;
            GameManager.Instance.OnBallReset += UpdateResetUI;
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScore -= UpdateScoreUI;
            GameManager.Instance.OnMiss -= UpdateMissUI;
            GameManager.Instance.OnBallReset -= UpdateResetUI;
        }
    }

    void UpdateScoreUI(int score, int streak)
    {
        txtScore.text = "Score: " + score;
        txtStreaks.text = "Streaks: " + streak;
        txtTotalShots.text = "Total Shots: " + GameManager.Instance.ShotsTaken;
    }

    void UpdateMissUI()
    {
        txtStreaks.text = "Streaks: 0";
        txtTotalShots.text = "Total Shots: " + GameManager.Instance.ShotsTaken;
    }

    void UpdateResetUI()
    {
        txtTotalShots.text = "Total Shots: " + GameManager.Instance.ShotsTaken;
    }
}
