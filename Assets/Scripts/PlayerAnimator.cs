using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    Animator animator;
    public BallController ball;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (animator != null)
        {
            animator.ResetTrigger("Throw");
            animator.ResetTrigger("Reset");
        }
    }

    public void PlayThrow()
    {
        if (animator != null)
            animator.SetTrigger("Throw");
    }

    public void PlayDribble()
    {
        if (animator != null)
        {
            animator.ResetTrigger("Throw");
            animator.ResetTrigger("Reset");
            animator.Play("Dribble", 0, 0f);
        }
    }

    // Called by Animation Event on the Throw clip
    public void ReleaseBall()
    {
        if (ball != null)
            ball.ReleaseBall();
    }
}
