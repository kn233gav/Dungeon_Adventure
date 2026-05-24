using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;

[RequireComponent(typeof(NavMeshAgent))]
public class GolemController : MonoBehaviour, IDamageable
{
    [Header("Характеристики Боса")]
    public float maxHealth = 200f; // Більше здоров'я
    public float currentHealth;
    public float aggroRadius = 20f;
    public float attackRange = 3f; // Більший радіус атаки, бо моделька велика
    public float damage = 25f;     // Сильний удар
    public float attackCooldown = 2.5f; // Повільні атаки

    [Header("Нагорода")]
    public float xpReward = 150f;

    [Header("Візуал та Аудіо")]
    public Animator animator;

    // Назви звуків (Додай їх в AudioManager!)
    private string sfxStep = "Golem_Step";
    private string sfxAttack = "Golem_Attack";
    private string sfxDamage = "Golem_Hit";
    private string sfxDeath = "Golem_Death";

    // Назви анімацій (З твого списку)
    private const string ANIM_IDLE = "Idle_Golem";
    private const string ANIM_WALK = "Walking_Golem";
    private const string ANIM_RUN = "Run_Golem";
    private const string ANIM_ATTACK = "Attack_Golem";
    private const string ANIM_DAMAGE = "Damage_Golem";

    private Transform target;
    private NavMeshAgent agent;
    private float searchTimer = 0f;
    private float lastAttackTime = 0f;
    private bool isDead = false;
    public event Action OnDeath;
    // Сервіси
    private IAnimationService _animService;
    private IAudioService _audioService;

    private string currentAnimState = "";
    private bool isAttacking = false;
    public event Action<float, float> OnHealthChanged;
    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();

        // Робимо Голема важчим
        agent.speed = 2.5f;
        agent.acceleration = 4f;

        if (animator == null) animator = GetComponentInChildren<Animator>();

        // Отримуємо сервіси
        _animService = ServiceLocator.Get<IAnimationService>();
        _audioService = ServiceLocator.Get<IAudioService>();

        FindPlayer();

        // Запускаємо важкі кроки
        StartCoroutine(HeavyFootstepsRoutine());
    }

    void Update()
    {
        if (isDead) return;

        // Логіка руху та анімацій
        if (!isAttacking)
        {
            HandleMovement();
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

    void HandleMovement()
    {
        float speed = agent.velocity.magnitude;
        string newAnim = "";

        // Вибір анімації залежно від швидкості
        if (speed > 4f)
        {
            newAnim = ANIM_RUN;
        }
        else if (speed > 0.1f)
        {
            newAnim = ANIM_WALK;
        }
        else
        {
            newAnim = ANIM_IDLE;
        }

        PlayAnimationInternal(newAnim, 0.2f); // 0.2f для плавнішого переходу, бо голем великий
    }

    void PlayAnimationInternal(string animName, float transition = 0.1f)
    {
        if (currentAnimState == animName) return;

        currentAnimState = animName;
        if (_animService != null && animator != null)
        {
            _animService.PlayAnimation(animator, animName, transition);
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
            StartCoroutine(PerformHeavyAttack());
            lastAttackTime = Time.time;
        }
    }

    IEnumerator PerformHeavyAttack()
    {
        isAttacking = true;
        agent.isStopped = true;

        // Граємо анімацію атаки
        // Force update state, щоб дозволити повторну атаку
        currentAnimState = ANIM_ATTACK;
        if (_animService != null) _animService.PlayAnimation(animator, ANIM_ATTACK, 0.1f);

        // Звук замаху/удару
        if (_audioService != null) _audioService.PlaySFX(sfxAttack, transform.position);

        // Голем замахується довше, ніж скелет
        yield return new WaitForSeconds(0.8f);

        // Перевірка удару
        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= attackRange + 1.5f) // Трішки більший запас дистанції
            {
                IDamageable damageableTarget = target.GetComponent<IDamageable>();
                if (damageableTarget != null)
                {
                    damageableTarget.TakeDamage(damage);
                    // Можна додати ефект тряски камери тут через EventBus, якщо є
                }
            }
        }

        yield return new WaitForSeconds(1.0f); // Відновлення після удару

        ResumeMovement();
        isAttacking = false;
        currentAnimState = "";
    }

    void ResumeMovement()
    {
        if (!isDead && agent != null && agent.isOnNavMesh) agent.isStopped = false;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        // --- СПОВІЩАЄМО UI ---
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        // ---------------------

        if (_audioService != null) _audioService.PlaySFX(sfxDamage, transform.position);

        // ... твоя логіка анімації ...

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void ResetAnimState() => currentAnimState = "";

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Golem Defeated!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterEnemyKill();
            // Тут можна додати логіку перемоги або випадіння крутого луту
            GameManager.Instance.OnBossDefeated();
        }

        agent.isStopped = true;
        agent.enabled = false;
        GetComponent<Collider>().enabled = false;

        // У списку немає окремої анімації смерті, тому використовуємо Damage або останній кадр
        if (_animService != null)
        {
            // Якщо пізніше зробиш анімацію Death_Golem, зміни цей рядок
            _animService.PlayAnimation(animator, ANIM_DAMAGE, 0.2f);
        }

        if (_audioService != null) _audioService.PlaySFX(sfxDeath, transform.position);

        Destroy(gameObject, 5f); // Довше лежить, бо великий
    }

    // Звуки важких кроків
    IEnumerator HeavyFootstepsRoutine()
    {
        while (!isDead)
        {
            if (agent != null && agent.enabled && agent.velocity.magnitude > 0.5f && !isAttacking)
            {
                if (_audioService != null)
                {
                    _audioService.PlaySFX(sfxStep, transform.position);
                }

                // Кроки набагато рідше, ніж у скелета
                float waitTime = agent.velocity.magnitude > 5f ? 0.6f : 1.2f;
                yield return new WaitForSeconds(waitTime);
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