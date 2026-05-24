using UnityEngine;

public class ArrowController : MonoBehaviour
{
    public Animator animator;
    public GameObject arrow;

    void Update()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        bool isAttack =
            state.IsName("Attack");
        arrow.SetActive(isAttack);
    }
}