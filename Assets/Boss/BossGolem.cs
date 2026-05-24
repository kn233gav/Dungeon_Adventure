using UnityEngine;
using System.Collections;
using System;

public class BossGolem : BaseEnemy
{
    [Header("Специфіка Боса")]
    public string animRun = "Run";

    // Події
    public event Action<float, float> OnHealthChanged; // Для оновлення смужки HP
    public event Action OnDeathEvent; // Нова подія: "Я помер"

    protected override void Start()
    {
        base.Start();

        // Фізика
        agent.speed = 2.5f;
        agent.acceleration = 4f;
    }

    // 1. Коли отримуємо шкоду -> Оновлюємо UI
    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // 2. Коли вмираємо -> Кажемо всім підписникам (Трігеру)
    protected override void Die()
    {
        if (isDead) return;

        Debug.Log("BOSS DEFEATED!");

        // Сповіщаємо Трігер кімнати, що ми померли
        OnDeathEvent?.Invoke();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterEnemyKill();
            // Якщо у GameManager є своя логіка перемоги, можна лишити:
            // GameManager.Instance.OnBossDefeated(); 
        }

        base.Die(); // Стандартна смерть (звук, анімація)
    }

    // --- Логіка руху та атаки без змін ---
    protected override void HandleMovement()
    {
        float speed = agent.velocity.magnitude;
        string newAnim = (speed > 4f) ? animRun : ((speed > 0.1f) ? animWalk : animIdle);
        PlayAnimationSafe(newAnim, 0.2f);
    }

    protected override IEnumerator PerformAttack()
    {
        isAttacking = true;
        agent.isStopped = true;

        PlayAnimationSafe(animAttack, 0.1f);
        if (_audioService != null) _audioService.PlaySFX(sfxAttack, transform.position);

        yield return new WaitForSeconds(0.8f); // Час замаху

        if (target != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);
            if (dist <= attackRange + 1.5f)
            {
                IDamageable hit = target.GetComponent<IDamageable>();
                if (hit != null) hit.TakeDamage(damage);
            }
        }

        yield return new WaitForSeconds(1.0f); // Відновлення

        if (!isDead && agent != null) agent.isStopped = false;
        isAttacking = false;
        ResetAnimState();
    }

    protected override IEnumerator FootstepSoundRoutine()
    {
        while (!isDead)
        {
            if (agent != null && agent.enabled && agent.velocity.magnitude > 0.5f && !isAttacking)
            {
                if (_audioService != null) _audioService.PlaySFX(sfxFootstep, transform.position);
                float waitTime = agent.velocity.magnitude > 5f ? 0.6f : 1.2f;
                yield return new WaitForSeconds(waitTime);
            }
            else yield return null;
        }
    }
}