using UnityEngine;

public class PlayerSprintingState : PlayerBaseState
{
    private IAnimationService animService;

    public PlayerSprintingState(PlayerController controller) : base(controller)
    {
        try
        {
            animService = ServiceLocator.Get<IAnimationService>();
        }
        catch
        {
            animService = null;
        }
    }

    public override void Enter()
    {
        controller.IsRegenPaused = true;
        if (animService != null)
        {
            animService.PlayAnimation(controller.animator, "Sprint", 0.1f);
        }
        else if (controller.animator != null) // старий метод
        {

            controller.animator.CrossFade("Sprint", 0.1f);
        }
    }

    public override void HandleInput()
    {
        bool isVerticalStable = Mathf.Abs(controller.rb.linearVelocity.y) < 1f;

        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded && isVerticalStable)
        {
            controller.Jump();
            return;
        }

        if (Input.GetMouseButtonDown(0) && controller.HasStamina(controller.attackStaminaCost))
        {
            controller.ChangeState(controller.combatState);
            return;
        }

        if (!Input.GetKey(KeyCode.LeftShift) || controller.currentStamina <= 0f || controller.moveDirection.magnitude < 0.1f)
        {
            controller.ChangeState(controller.movingState);
            return;
        }
    }

    public override void Update()
    {

        if (!controller.isGrounded)
        {
            controller.ChangeState(controller.movingState);
            return;
        }

        controller.ConsumeStamina(controller.sprintStaminaCost * Time.deltaTime);
        if (controller.currentStamina <= 0f)
        {
            controller.ChangeState(controller.movingState);
        }
    }

    public override void FixedUpdate()
    {
        controller.ApplyMovement(controller.sprintSpeed);
        controller.HandleRotation();
    }
}