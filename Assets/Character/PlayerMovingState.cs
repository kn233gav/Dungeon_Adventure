using UnityEngine;

public class PlayerMovingState : PlayerBaseState
{
    private IAnimationService animService;

    // Переконайтеся, що в Аніматорі стейт називається саме "Walking"
    // Якщо у вас там "Run" або "Move", змініть це слово тут!
    private const string ANIMATION_NAME = "Walking";

    public PlayerMovingState(PlayerController controller) : base(controller)
    {
        // Отримуємо сервіс один раз при створенні, а не кожен раз в Enter
        try
        {
            animService = ServiceLocator.Get<IAnimationService>();
        }
        catch { animService = null; }
    }

    public override void Enter()
    {
        controller.IsRegenPaused = false;
        if (animService != null)
        {
            animService.PlayAnimation(controller.animator, ANIMATION_NAME, 0.1f);
        }
        else if (controller.animator != null) // Розкоментував для надійності
        {
            controller.animator.CrossFade(ANIMATION_NAME, 0.1f);
        }
    }

    public override void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            controller.Jump();
            return;
        }

        if (Input.GetMouseButtonDown(0) && controller.HasStamina(controller.attackStaminaCost))
        {
            controller.ChangeState(controller.combatState);
            return;
        }

        // --- ГОЛОВНИЙ ФІКС ---
        // Було: controller.HasStamina(0) -> Це дозволяло бігти з 0.001 стаміни
        // Стало: controller.HasStamina(5f) -> Тепер треба накопичити хоча б 5 стаміни, щоб знову побігти
        if (Input.GetKey(KeyCode.LeftShift) && controller.HasStamina(5f))
        {
            controller.ChangeState(controller.sprintingState);
            return;
        }

        if (controller.moveDirection.magnitude < 0.1f)
        {
            controller.ChangeState(controller.idleState);
            return;
        }
    }

    public override void Update()
    {
        // Відновлюємо стаміну під час звичайної ходьби
        controller.RegenerateStamina();
    }

    public override void FixedUpdate()
    {
        controller.ApplyMovement(controller.moveSpeed);
        controller.HandleRotation();
    }
}