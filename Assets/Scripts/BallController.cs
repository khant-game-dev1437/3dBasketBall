using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Header("References")]
    public DribbleBall dribble;
    public TrajectoryRenderer trajectory;
    public Transform hoopTarget;
    public PlayerAnimator playerAnimator;

    [Header("Arc Settings")]
    [Tooltip("How high the arc peaks above start/target")]
    public float arcHeight = 5f;

    [Header("Aim Settings")]
    [Tooltip("How much mouse movement shifts aim left/right")]
    public float aimSensitivityX = 8f;
    [Tooltip("How much mouse movement shifts aim up/down")]
    public float aimSensitivityY = 6f;
    public float clickRadius = 3f;

    enum State { Dribbling, Aiming, InFlight, Resetting }
    State state = State.Dribbling;

    Rigidbody rb;
    Camera cam;
    Vector2 mouseStartScreen;
    Vector3 launchPos;
    Vector3 launchVelocity;
    bool hitRimThisShot;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
    }

    void Start()
    {
        state = State.Dribbling;
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void Update()
    {
        if (state == State.Dribbling)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                Vector3 toBall = transform.position - ray.origin;
                Vector3 proj = Vector3.Project(toBall, ray.direction);
                float dist = (toBall - proj).magnitude;

                if (dist < clickRadius)
                {
                    EnterAim();
                }
            }
        }
        else if (state == State.Aiming)
        {
            UpdateAim();

            if (Input.GetMouseButtonUp(0))
            {
                Throw();
            }

            if (Input.GetMouseButtonDown(1))
            {
                CancelAim();
            }
        }
    }

    void EnterAim()
    {
        state = State.Aiming;
        hitRimThisShot = false;

        if (dribble != null) dribble.StopDribble();

        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        mouseStartScreen = Input.mousePosition;

        // Hold ball at current position
        launchPos = transform.position + Vector3.up * 0.3f;

        if (trajectory != null) trajectory.AimLineToggle();
    }

    void UpdateAim()
    {
        // Smoothly move to launch position
        transform.position = Vector3.Lerp(transform.position, launchPos, 10f * Time.deltaTime);

        // Mouse offset from click point determines aim adjustment
        Vector2 mouseDelta = (Vector2)Input.mousePosition - mouseStartScreen;
        float dx = mouseDelta.x / Screen.width;
        float dy = mouseDelta.y / Screen.height;

        // Target = hoop position + mouse offset
        Vector3 target;
        if (hoopTarget != null)
        {
            target = hoopTarget.position;
        }
        else
        {
            target = transform.position + Vector3.forward * 15f + Vector3.up * 5f;
        }

        target.x += dx * aimSensitivityX;
        target.y += dy * aimSensitivityY;

        // Calculate arc velocity to hit that target
        launchVelocity = GetArcVelocity(transform.position, target, arcHeight);

        // Update line
        if (trajectory != null)
        {
            trajectory.UpdateTrajectory(transform.position, launchVelocity);
        }
    }

    Vector3 GetArcVelocity(Vector3 start, Vector3 target, float height)
    {
        float g = Mathf.Abs(Physics.gravity.y);

        // Peak must be above both start and target
        float peakY = Mathf.Max(start.y, target.y) + height;

        // Vertical: how long to go up, how long to come down
        float upDist = Mathf.Max(peakY - start.y, 0.1f);
        float downDist = Mathf.Max(peakY - target.y, 0.1f);
        float timeUp = Mathf.Sqrt(2f * upDist / g); //dis = 1/2 g t^2 , so t = sqrt(2*upDist/g)
        float timeDown = Mathf.Sqrt(2f * downDist / g);
        float totalTime = timeUp + timeDown;

        // Initial upward velocity
        float vy = g * timeUp; // 

        // Horizontal velocity to cover XZ distance in totalTime
        Vector3 horizontal = new Vector3(target.x - start.x, 0f, target.z - start.z);
        Vector3 vh = horizontal / totalTime;

        return vh + Vector3.up * vy;
    }


    void Throw()
    {
        state = State.InFlight;

        rb.isKinematic = false;
        rb.drag = 0f;
        rb.velocity = launchVelocity;
        rb.angularVelocity = new Vector3(-launchVelocity.magnitude * 1.5f, 0f, 0f);

        if (trajectory != null) trajectory.Hide();

        if (playerAnimator != null) playerAnimator.PlayThrow();

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterThrow();
    }

    void CancelAim()
    {
        state = State.Dribbling;
        rb.isKinematic = true;
        if (trajectory != null) trajectory.Hide();
        if (dribble != null) dribble.StartDribble();
    }

   
    public void ResetBall()
    {
        state = State.Resetting;
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        if (trajectory != null) trajectory.Hide();
        StartCoroutine(ResetLerp());
    }

    System.Collections.IEnumerator ResetLerp()
    {
        Vector3 startPos = transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 2f; // Speed of lerp
            Vector3 handPos = dribble != null && dribble.handBone != null
                ? dribble.handBone.position
                : startPos + Vector3.up * 2f;

            // Lerp with an arc so it looks like the ball floats back
            Vector3 pos = Vector3.Lerp(startPos, handPos, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * 2f;
            transform.position = pos;
            yield return null;
        }

        state = State.Dribbling;
        if (dribble != null) dribble.StartDribble();
        if (playerAnimator != null) playerAnimator.PlayDribble();
    }


    void OnTriggerEnter(Collider collision)
    {
        if (state != State.InFlight) return;

        if (collision.gameObject.name.Contains("win"))
        {
            bool wasSwish = !hitRimThisShot;

            if (GameManager.Instance != null)
                GameManager.Instance.RegisterScore(wasSwish);

            if (CameraController.Instance != null)
                CameraController.Instance.PlayScoreEffect();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (state != State.InFlight) return;

        if (collision.gameObject.CompareTag("Rim"))
        {
            hitRimThisShot = true;
            Debug.Log("Rim hit!");
        }

        if (GameManager.Instance != null)
            GameManager.Instance.StartResetTimer();
    }

    public bool IsInFlight() => state == State.InFlight;
}
