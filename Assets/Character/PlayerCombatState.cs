using UnityEngine;

public class PlayerCombatState : PlayerBaseState
{
    private float attackTimer;

    public PlayerCombatState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        controller.IsRegenPaused = true;
        PerformAttack();
    }

    public override void Update()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0)
        {
            //if (Input.GetMouseButton(0) && controller.HasStamina(controller.attackStaminaCost))
            //{
            //    PerformAttack();
            //}
            //else
            //{
                DecideNextState();
            //}
        }
    }

    public override void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            controller.Jump();
            return;
        }
    }

    public override void FixedUpdate()
    {

        if (controller.isRangedCharacter)
        {

            controller.ApplyMovement(0f);

            controller.HandleRotation();
        }
        else
        {
            controller.ApplyMovement(controller.combatMoveSpeed);
            controller.HandleRotation();
        }
    }

    private void PerformAttack()
    {
        //Якщо це дальній бій — ставимо таймер жорстко під анімацію(наприклад, 1.5 сек)
        if (controller.isRangedCharacter)
        {
            attackTimer = 1.5f; // Підстав сюди точний час своєї анімації
        }
        else
        {
            attackTimer = controller.attackDuration;
        }

        controller.ConsumeStamina(controller.attackStaminaCost);
        controller.StartAttack();
    }

    private void DecideNextState()
    {
        if (controller.moveDirection.magnitude > 0.1f)
        {
            if (Input.GetKey(KeyCode.LeftShift) && controller.HasStamina(0))
                controller.ChangeState(controller.sprintingState);
            else
                controller.ChangeState(controller.movingState);
        }
        else
        {
            controller.ChangeState(controller.idleState);
        }
    }
}