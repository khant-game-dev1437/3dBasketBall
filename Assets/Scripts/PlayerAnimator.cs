using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayThrow()
    {
        if (animator != null)
            animator.SetTrigger("Throw");
    }

    public void PlayDribble()
    {
        if (animator != null)
            animator.SetTrigger("Reset");
    }
}
