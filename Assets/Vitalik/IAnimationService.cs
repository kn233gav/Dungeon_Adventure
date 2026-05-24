using UnityEngine;

public interface IAnimationService
{

    void PlayAnimation(Animator animator, string animationName, float transitionDuration = 0.1f);

    bool IsAnimationFinished(Animator animator, string animationName, float completionThreshold = 0.9f);
}