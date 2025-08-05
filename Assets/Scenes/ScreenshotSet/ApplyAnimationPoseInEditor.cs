using UnityEngine;

[ExecuteInEditMode]
public class ApplyAnimationPoseInEditor : MonoBehaviour
{
    public Animator animator;
    public string animationName = "Idle";
    [Range(0f, 1f)] public float normalizedTime = 0.5f;

    void Update()
    {
        if (!Application.isPlaying && animator)
        {
            animator.Play(animationName, 0, normalizedTime);
            animator.Update(0);
            animator.speed = 0;
        }
    }
}
