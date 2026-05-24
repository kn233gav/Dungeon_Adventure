using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        controller.IsRegenPaused = false;
        IAnimationService animService = null;

        try
        {
            animService = ServiceLocator.Get<IAnimationService>();
        }
        catch
        {
        }
        if (animService != null)
        {
            animService.PlayAnimation(controller.animator, "Idle", 0.1f);
        }
        else // краще видалити, залишилось для стабільності
        {

            if (controller.animator != null)
            {
                controller.animator.CrossFade("Idle", 0.1f);
            }
        }
    }

    public override void HandleInput()
    {

        // Стрибок
        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            controller.Jump();
            return;
        }

        // Атака
        if (Input.GetMouseButtonDown(0) && controller.HasStamina(controller.attackStaminaCost))
        {
            controller.ChangeState(controller.combatState);
            return;
        }

        // Рух
        if (controller.moveDirection.magnitude > 0.1f)
        {
            if (Input.GetKey(KeyCode.LeftShift) && controller.HasStamina(0))
            {
                controller.ChangeState(controller.sprintingState);
            }
            else
            {
                controller.ChangeState(controller.movingState);
            }
            return;
        }
    }

    public override void Update()
    {
        controller.RegenerateStamina();
    }

    public override void FixedUpdate()
    {
        controller.ApplyDeceleration();
        controller.HandleRotation();
    }
}