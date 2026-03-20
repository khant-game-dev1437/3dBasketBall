using UnityEngine;

public class DribbleBall : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The right hand bone transform from the character rig")]
    public Transform handBone;
    public AudioClip bounceSound;

    [Header("Dribble Settings")]
    [Tooltip("Height below which the ball detaches and bounces")]
    public float releaseHeight = 1.0f;
    [Tooltip("Height at which the ball snaps back to the hand")]
    public float catchHeight = 0.9f;
    [Tooltip("Ground Y position for bounce (floor top + ball radius)")]
    public float groundY = 0.22f;
    [Tooltip("How fast the ball falls")]
    public float gravity = 20f;
    [Tooltip("Fixed upward speed after bounce")]
    public float bounceUpSpeed = 6f;
    [Tooltip("World-space offset below the hand (Y is down)")]
    public Vector3 handOffset = new Vector3(0f, -0.25f, 0f);
    [Tooltip("How fast the ball lerps back to hand on catch")]
    public float catchSmoothing = 15f;

    enum DribbleState { InHand, Falling, Rising, Catching }
    DribbleState state = DribbleState.InHand;

    float velocityY;
    Vector3 bounceXZ;
    Rigidbody rb;
    AudioSource audioSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    Vector3 GetHandTarget()
    {
        return handBone.position + handOffset;
    }

    void LateUpdate()
    {
        if (handBone == null) return;

        // Keep rigidbody kinematic while dribbling so it doesn't fight us
        if (rb != null && !rb.isKinematic)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Vector3 handTarget = GetHandTarget();

        switch (state)
        {
            case DribbleState.InHand:
                transform.position = handTarget;

                // Release when hand pushes down low enough
                if (handTarget.y < releaseHeight)
                {
                    state = DribbleState.Falling;
                    velocityY = -2f;
                    bounceXZ = new Vector3(handTarget.x, 0f, handTarget.z);
                }
                break;

            case DribbleState.Falling:
                velocityY -= gravity * Time.deltaTime;
                Vector3 fallPos = transform.position;
                fallPos.y += velocityY * Time.deltaTime;
                // Gently track hand XZ so ball stays under the hand
                fallPos.x = Mathf.Lerp(fallPos.x, handTarget.x, Time.deltaTime * 5f);
                fallPos.z = Mathf.Lerp(fallPos.z, handTarget.z, Time.deltaTime * 5f);

                if (fallPos.y <= groundY)
                {
                    fallPos.y = groundY;
                    velocityY = bounceUpSpeed;
                    state = DribbleState.Rising;

                    Debug.Log("Bounce! sound:" + (bounceSound != null) + " audio:" + (audioSource != null));
                    if (bounceSound != null)
                        audioSource.PlayOneShot(bounceSound);
                }

                transform.position = fallPos;
                break;

            case DribbleState.Rising:
                velocityY -= gravity * Time.deltaTime;
                Vector3 risePos = transform.position;
                risePos.y += velocityY * Time.deltaTime;
                risePos.x = Mathf.Lerp(risePos.x, handTarget.x, Time.deltaTime * 10f);
                risePos.z = Mathf.Lerp(risePos.z, handTarget.z, Time.deltaTime * 10f);

                // When ball is near hand height and hand is high enough, start catching
                if (risePos.y >= catchHeight && handTarget.y >= catchHeight)
                {
                    state = DribbleState.Catching;
                }

                // If velocity goes negative before catch, it's falling again
                if (velocityY < 0f)
                {
                    // If still above ground, let it fall
                    if (risePos.y > groundY + 0.05f)
                    {
                        state = DribbleState.Falling;
                    }
                }

                transform.position = risePos;
                break;

            case DribbleState.Catching:
                // Smoothly lerp to hand 
                transform.position = Vector3.Lerp(transform.position, handTarget, catchSmoothing * Time.deltaTime);

                // Close enough  fully in hand
                if (Vector3.Distance(transform.position, handTarget) < 0.05f)
                {
                    state = DribbleState.InHand;
                    transform.position = handTarget;
                }
                break;
        }

        // Spin the ball for visual flair
        transform.Rotate(Vector3.right, 300f * Time.deltaTime, Space.World);
    }

    public void StopDribble()
    {
        enabled = false;
    }

    public void StartDribble()
    {
        state = DribbleState.InHand;
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        enabled = true;
    }
}
