using UnityEngine;

public class PlayerJumpingState : IPlayerState
{
    private PlayerController player;
    private IAnimationService animService;

    private enum JumpStage
    {
        Start, 
        Mid,   
        End    
    }

    private JumpStage currentStage;
    private bool hasJumped;
    private float jumpStartTime;

    public PlayerJumpingState(PlayerController player)
    {
        this.player = player;
        try
        {
            animService = ServiceLocator.Get<IAnimationService>();
        }
        catch
        {
            animService = null;
        }
    }

    public void Enter()
    {
        currentStage = JumpStage.Start;
        hasJumped = false;
        jumpStartTime = Time.time;
        PlaySafe("JumpStart", 0.1f);

        ApplyJumpForce();   
    }

    public void HandleInput()
    {
        //player.ReadMovementInput();
        player.HandleRotation();
    }

    public void Update()
    {
        switch (currentStage)
        {
            case JumpStage.Start:
                if (CheckFinishedSafe("JumpStart"))
                {
                    currentStage = JumpStage.Mid;
                    PlaySafe("JumpMid", 0.1f);
                }
                break;

            case JumpStage.Mid:

                if (Time.time < jumpStartTime + 0.2f) return;

                bool isFalling = player.rb.linearVelocity.y <= 0.1f;

                if (player.isGrounded && isFalling)
                {
                    currentStage = JumpStage.End;
                    PlaySafe("JumpEnd", 0.05f);

                    Vector3 currentVel = player.rb.linearVelocity;
                    player.rb.linearVelocity = new Vector3(0, currentVel.y, 0);
                }
                break;

            case JumpStage.End:
                if (player.moveDirection.magnitude > 0.1f)
                {
                    player.ChangeState(player.movingState);
                    return;
                }

                if (CheckFinishedSafe("JumpEnd"))
                {
                    player.ChangeState(player.idleState);
                }
                break;
        }
    }

    public void FixedUpdate()
    {
        if (currentStage != JumpStage.End)
        {
            player.ApplyMovement(player.moveSpeed, player.airControlFactor);
        }
    }

    public void Exit()
    {
    }

    private void ApplyJumpForce()
    {
        if (!hasJumped)
        {
            Vector3 v = player.rb.linearVelocity;
            player.rb.linearVelocity = new Vector3(v.x, 0, v.z);

            player.rb.AddForce(Vector3.up * player.jumpForce, ForceMode.Impulse);
            hasJumped = true;
        }
    }

    private void PlaySafe(string animName, float fadeTime)
    {
        if (animService != null)
        {
            animService.PlayAnimation(player.animator, animName, fadeTime);
        }
        else if (player.animator != null)
        {
            player.animator.CrossFade(animName, fadeTime);
        }
    }
    private bool CheckFinishedSafe(string animName) // старий метод дл€ стаб≥льност≥
    {
        if (animService != null)
        {
            return animService.IsAnimationFinished(player.animator, animName, 0.9f);
        }
        else if (player.animator != null)
        {
            // —тара лог≥ка (Fallback)
            AnimatorStateInfo info = player.animator.GetCurrentAnimatorStateInfo(0);
            return info.IsName(animName) && info.normalizedTime >= 0.9f && !player.animator.IsInTransition(0);
        }
        return true; // якщо ан≥матору немаЇ, вважаЇмо, що все зак≥нчилось, щоб не зависнути
    }
}