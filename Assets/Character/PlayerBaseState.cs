using UnityEngine;

public abstract class PlayerBaseState : IPlayerState // Припускаю, що у вас є такий абстрактний клас або інтерфейс
{
    protected PlayerController controller;
    protected IAnimationService animService; // << ДОДАНО

    protected PlayerBaseState(PlayerController controller)
    {
        this.controller = controller;
        // Отримуємо посилання на сервіс один раз при створенні стану
        this.animService = ServiceLocator.Get<IAnimationService>();
    }

    public abstract void Enter();
    public abstract void HandleInput();
    public abstract void Update();
    public abstract void FixedUpdate();
    public virtual void Exit() { }
}