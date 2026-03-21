using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Clips")]
    public AudioClip scoreSound;
    public AudioClip missSound;
    public AudioClip crowdCheer;
    public AudioClip crowdLaugh;

    AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
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
            GameManager.Instance.OnScore += PlayScoreSound;
            GameManager.Instance.OnMiss += PlayMissSound;
            GameManager.Instance.OnPityMode += PlayLaugh;
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScore -= PlayScoreSound;
            GameManager.Instance.OnMiss -= PlayMissSound;
            GameManager.Instance.OnPityMode -= PlayLaugh;
        }
    }

    void PlayScoreSound(int score, int streak, bool wasSwish)
    {
        if (scoreSound != null)
            audioSource.PlayOneShot(scoreSound);

        if (crowdCheer != null)
            audioSource.PlayOneShot(crowdCheer, 0.7f);
    }

    void PlayMissSound()
    {
        if (missSound != null)
            audioSource.PlayOneShot(missSound);
    }

    void PlayLaugh()
    {
        if (crowdLaugh != null)
            audioSource.PlayOneShot(crowdLaugh);
    }
}
