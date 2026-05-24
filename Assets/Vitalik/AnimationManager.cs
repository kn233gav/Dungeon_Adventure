using UnityEngine;

public class AnimationManager : IAnimationService
{
    public void PlayAnimation(Animator animator, string animationName, float transitionDuration = 0.1f)
    {
        if (animator == null) return;


        AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (currentInfo.IsName(animationName) && !animator.IsInTransition(0))
        {

            return;
        }


        animator.CrossFade(animationName, transitionDuration);
    }

    public bool IsAnimationFinished(Animator animator, string animationName, float completionThreshold = 0.9f)
    {
        if (animator == null) return true;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        bool isCorrectAnimation = stateInfo.IsName(animationName);

        bool isTimeUp = stateInfo.normalizedTime >= completionThreshold;

        bool isNotTransitioning = !animator.IsInTransition(0);

        return isCorrectAnimation && isTimeUp && isNotTransitioning;
    }
}