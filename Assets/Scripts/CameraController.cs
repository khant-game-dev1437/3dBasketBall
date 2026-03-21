using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    public Transform hoopTarget;
    public float zoomDuration = 1f;
    public float slowMoScale = 0.2f;
    public float zoomFOV = 30f;

    Camera cam;
    Vector3 originalPos;
    Quaternion originalRot;
    float originalFOV;
    bool isZooming;

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
        originalPos = transform.position;
        originalRot = transform.rotation;
        originalFOV = cam.fieldOfView;
    }

    public void PlayScoreEffect()
    {
        if (!isZooming)
            StartCoroutine(ScoreZoom());
    }

    IEnumerator ScoreZoom()
    {
        isZooming = true;

        // Slow mo
        Time.timeScale = slowMoScale;
        Time.fixedDeltaTime = 0.02f * slowMoScale;

        // Zoom toward hoop
        Vector3 targetPos = originalPos;
        if (hoopTarget != null)
        {
            Vector3 toHoop = hoopTarget.position - originalPos;
            float moveDist = Mathf.Min(3f, toHoop.magnitude * 0.4f);
            targetPos = originalPos + toHoop.normalized * moveDist;
        }

        float timer = 0f;
        float halfDuration = zoomDuration * 0.5f;

        // Zoom in
        while (timer < halfDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / halfDuration;
            transform.position = Vector3.Lerp(originalPos, targetPos, t);
            cam.fieldOfView = Mathf.Lerp(originalFOV, zoomFOV, t);
            yield return null;
        }

        // Hold briefly
        yield return new WaitForSecondsRealtime(0.3f);

        // Zoom back
        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / halfDuration;
            transform.position = Vector3.Lerp(targetPos, originalPos, t);
            cam.fieldOfView = Mathf.Lerp(zoomFOV, originalFOV, t);
            yield return null;
        }

        // Restore
        transform.position = originalPos;
        transform.rotation = originalRot;
        cam.fieldOfView = originalFOV;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        isZooming = false;
    }
}
