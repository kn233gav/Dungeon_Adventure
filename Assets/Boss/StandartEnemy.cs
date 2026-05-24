using UnityEngine;
using System.Collections;

public class StandardEnemy : BaseEnemy
{
    [Header("Специфіка Мобів")]
    [Tooltip("Список анімацій для випадкової атаки")]
    public string[] attackAnimations;

    [Header("Лут")]
    [Range(0f, 1f)] public float potionDropChance = 0.25f;
    public GameObject potionPrefab;
    public float dropHeightOffset = 0.5f;

    // Реалізуємо обов'язковий метод атаки
    protected override IEnumerator PerformAttack()
    {
        isAttacking = true;
        agent.isStopped = true;

        // 1. Вибір випадкової анімації
        string chosenAnim = animAttack; // Фолбек
        if (attackAnimations != null && attackAnimations.Length > 0)
        {
            chosenAnim = attackAnimations[Random.Range(0, attackAnimations.Length)];
        }

        // 2. Старт анімації та звуку
        PlayAnimationSafe(chosenAnim, 0.1f);
        if (_audioService != null) _audioService.PlaySFX(sfxAttack, transform.position);

        // 3. Чекаємо моменту удару
        yield return new WaitForSeconds(0.5f);

        // 4. Нанесення шкоди
        if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange + 1f)
        {
            IDamageable damageableTarget = target.GetComponent<IDamageable>();
            if (damageableTarget != null) damageableTarget.TakeDamage(damage);
        }

        // 5. Чекаємо кінця анімації
        yield return new WaitForSeconds(1.0f);

        // 6. Завершення
        if (!isDead && agent != null) agent.isStopped = false;
        isAttacking = false;
        ResetAnimState();
    }

    // Перевизначаємо смерть, щоб додати випадіння зілля
    protected override void Die()
    {
        TryDropPotion();
        base.Die(); // Викликаємо базову логіку смерті (XP, анімація, знищення)
    }

    private void TryDropPotion()
    {
        if (Random.value <= potionDropChance && potionPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * dropHeightOffset;
            Instantiate(potionPrefab, spawnPos, Quaternion.identity);
        }
    }
}