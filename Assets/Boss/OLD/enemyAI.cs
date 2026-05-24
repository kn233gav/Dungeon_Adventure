using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour, IDamageable
{
    [Header("Характеристики")]
    public float maxHealth = 50f;
    private float currentHealth;
    public float aggroRadius = 15f;
    public float attackRange = 2f;
    public float damage = 10f;
    public float attackCooldown = 1.5f;

    [Header("Нагорода (XP та Лут)")]
    public float xpReward = 50f;
    [Range(0f, 1f)] public float potionDropChance = 0.25f;
    public GameObject potionPrefab;
    public float dropHeightOffset = 0.5f;

    [Header("Візуал та Аудіо")]
    public Animator animator;

    // --- !!! ЗМІНА 1: Універсальні назви для руху !!! ---
    // Ти можеш міняти їх в інспекторі для кожного префабу окремо
    public string animIdle = "Idle";
    public string animWalk = "Walk";
    public string animDeath = "Death";
    public string animDamage = "Damage";

    // --- !!! ЗМІНА 2: Масив для атак замість однієї формули !!! ---
    [Tooltip("Список назв анімацій атак. Скрипт обиратиме одну випадкову з цього списку.")]
    public string[] attackAnimations;

    [Header("Звуки")]
    public string sfxFootstep = "Step";
    public string sfxAttack = "Attack";
    public string sfxDamage = "Damage";
    public string sfxDeath = "Death";

    private Transform target;
    private NavMeshAgent agent;
    private float searchTimer = 0f;
    private float lastAttackTime = 0f;
    private bool isDead = false;

    // Сервіси та стан
    private IAnimationService _animService;
    private IAudioService _audioService;
    private string currentAnimState = "";
    private bool isAttacking = false;

    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();

        if (animator == null) animator = GetComponentInChildren<Animator>();

        try { _animService = ServiceLocator.Get<IAnimationService>(); } catch { }
        try { _audioService = ServiceLocator.Get<IAudioService>(); } catch { }

        FindPlayer();

        if (_audioService != null) StartCoroutine(FootstepSoundRoutine());
    }

    void Update()
    {
        if (isDead) return;

        if (!isAttacking)
        {
            UpdateMovementState();
        }

        if (target == null)
        {
            SearchForPlayer();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= aggroRadius)
        {
            agent.SetDestination(target.position);

            if (distance <= agent.stoppingDistance + attackRange)
            {
                AttackBehavior();
            }
        }
    }

    void UpdateMovementState()
    {
        if (agent.velocity.magnitude > 0.1f) agent.isStopped = false;

        float speed = agent.velocity.magnitude;
        string newAnim = "";

        if (speed > 0.1f)
            newAnim = animWalk;
        else
            newAnim = animIdle;

        PlayAnimationSafe(newAnim);
    }

    void PlayAnimationSafe(string animName, float transition = 0.1f)
    {
        if (currentAnimState == animName) return;
        currentAnimState = animName;

        if (_animService != null && animator != null)
        {
            _animService.PlayAnimation(animator, animName, transition);
        }
        else if (animator != null)
        {
            animator.CrossFade(animName, transition);
        }
    }

    void SearchForPlayer()
    {
        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0)
        {
            FindPlayer();
            searchTimer = 1f;
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) target = playerObj.transform;
    }

    void AttackBehavior()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(PerformAttack());
            lastAttackTime = Time.time;
        }
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        agent.isStopped = true;

        // --- !!! ЗМІНА 3: Розумний вибір атаки !!! ---
        string attackAnimName = "";

        // Перевіряємо, чи є взагалі атаки в списку
        if (attackAnimations != null && attackAnimations.Length > 0)
        {
            // Беремо випадковий індекс від 0 до (кількість атак - 1)
            int randomIndex = Random.Range(0, attackAnimations.Length);
            attackAnimName = attackAnimations[randomIndex];
        }
        else
        {
            Debug.LogWarning($"EnemyAI ({gameObject.name}): Не вказано жодної анімації атаки в інспекторі!");
            // Фолбек, щоб не зависло, якщо забув заповнити масив
            yield return new WaitForSeconds(0.5f);
            isAttacking = false;
            if (!isDead && agent != null) agent.isStopped = false;
            yield break; // Виходимо з корутини
        }

        // Граємо анімацію
        currentAnimState = attackAnimName;
        // Debug.Log("Атакую анімацією: " + attackAnimName);

        animator.Play(attackAnimName); // Або PlayAnimationSafe, якщо хочеш кросфейд

        if (_audioService != null) _audioService.PlaySFX(sfxAttack, transform.position);

        // Чекаємо моменту удару (0.5 сек - краще винести в змінну attackImpactTime, але поки хай так)
        yield return new WaitForSeconds(0.5f);

        // Наносимо шкоду
        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            // Трохи збільшив запас дистанції, щоб ворог не мазав впритул
            if (distance <= attackRange + 2.0f)
            {
                IDamageable damageableTarget = target.GetComponent<IDamageable>();
                if (damageableTarget != null)
                {
                    damageableTarget.TakeDamage(damage);
                }
            }
        }

        // Чекаємо завершення анімації (приблизно)
        yield return new WaitForSeconds(1.0f);

        if (!isDead && agent != null) agent.isStopped = false;
        isAttacking = false;
        currentAnimState = "";
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (_audioService != null) _audioService.PlaySFX(sfxDamage, transform.position);

        if (!isAttacking)
        {
            PlayAnimationSafe(animDamage, 0.05f);
            Invoke(nameof(ResetAnimState), 0.5f);
        }

        if (currentHealth <= 0) Die();
    }

    void ResetAnimState() => currentAnimState = "";

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (GameManager.Instance != null) GameManager.Instance.RegisterEnemyKill();

        agent.isStopped = true;
        agent.enabled = false;
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        GiveRewards();

        PlayAnimationSafe(animDeath, 0.1f);
        if (animator != null) animator.SetTrigger("Die"); // Про всяк випадок

        if (_audioService != null) _audioService.PlaySFX(sfxDeath, transform.position);

        Destroy(gameObject, 3f);
    }

    void GiveRewards()
    {
        if (target == null) return;

        float finalXP = xpReward;
        if (RunManager.Instance != null) finalXP *= RunManager.Instance.GetDifficultyXPMultiplier();

        PlayerController player = target.GetComponent<PlayerController>();
        if (player != null)
        {
            player.GainExperience(finalXP);
            if (Random.value <= potionDropChance)
            {
                if (potionPrefab != null)
                {
                    Vector3 spawnPos = transform.position + Vector3.up * dropHeightOffset;
                    Instantiate(potionPrefab, spawnPos, Quaternion.identity);
                }
            }
        }
    }

    IEnumerator FootstepSoundRoutine()
    {
        while (!isDead)
        {
            if (agent != null && agent.enabled && agent.velocity.magnitude > 0.5f && !isAttacking)
            {
                if (_audioService != null) _audioService.PlaySFX(sfxFootstep, transform.position);
                // Трохи спростив логіку очікування, оскільки бігу немає
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                yield return null;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}