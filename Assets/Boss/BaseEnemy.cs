using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class BaseEnemy : MonoBehaviour, IDamageable
{
    [Header("Базові Характеристики")]
    public float maxHealth = 50f;
    public float currentHealth;
    public float aggroRadius = 15f;
    public float attackRange = 2f;
    public float damage = 10f;
    public float attackCooldown = 1.5f;
    public float xpReward = 50f;

    [Header("Анімації (Назви)")]
    public string animIdle = "Idle";
    public string animWalk = "Walk";
    public string animDeath = "Death";
    public string animDamage = "Damage";
    public string animAttack = "Attack"; // Базова назва, може перекриватися

    [Header("Аудіо")]
    public string sfxFootstep = "Step";
    public string sfxAttack = "Attack";
    public string sfxDamage = "Damage";
    public string sfxDeath = "Death";

    // protected дозволяє дітям бачити ці змінні
    protected Transform target;
    protected NavMeshAgent agent;
    protected Animator animator;
    protected bool isDead = false;
    protected bool isAttacking = false;
    protected float lastAttackTime = 0f;

    // Сервіси
    protected IAnimationService _animService;
    protected IAudioService _audioService;
    protected string currentAnimState = "";
    private float searchTimer = 0f;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        // Безпечне отримання сервісів
        try { _animService = ServiceLocator.Get<IAnimationService>(); } catch { }
        try { _audioService = ServiceLocator.Get<IAudioService>(); } catch { }

        FindPlayer();
        StartCoroutine(FootstepSoundRoutine());
    }

    protected virtual void Update()
    {
        if (isDead) return;

        if (!isAttacking)
        {
            HandleMovement();
        }

        if (target == null)
        {
            SearchForPlayerLogic();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= aggroRadius)
        {
            agent.SetDestination(target.position);

            // Перевірка дистанції атаки
            if (distance <= agent.stoppingDistance + attackRange)
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    StartCoroutine(PerformAttack());
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    // virtual дозволяє змінити логіку в спадкоємцях (наприклад, для Голема)
    protected virtual void HandleMovement()
    {
        if (agent.velocity.magnitude > 0.1f) agent.isStopped = false;

        float speed = agent.velocity.magnitude;
        string newAnim = (speed > 0.1f) ? animWalk : animIdle;
        PlayAnimationSafe(newAnim);
    }

    // Абстрактний метод змушує спадкоємців реалізувати свою версію атаки
    protected abstract IEnumerator PerformAttack();

    public virtual void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (_audioService != null) _audioService.PlaySFX(sfxDamage, transform.position);

        if (!isAttacking)
        {
            PlayAnimationSafe(animDamage, 0.1f);
            Invoke(nameof(ResetAnimState), 0.5f);
        }

        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        if (GameManager.Instance != null) GameManager.Instance.RegisterEnemyKill();

        // Вимикаємо фізику та AI
        agent.isStopped = true;
        agent.enabled = false;
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        GiveXPReward(); // Викликаємо нагороду

        PlayAnimationSafe(animDeath, 0.1f);
        if (_audioService != null) _audioService.PlaySFX(sfxDeath, transform.position);

        Destroy(gameObject, 4f); // Даємо час на анімацію
    }

    protected void GiveXPReward()
    {
        if (target == null) return;

        float finalXP = xpReward;
        if (RunManager.Instance != null) finalXP *= RunManager.Instance.GetDifficultyXPMultiplier();

        // Тут припускаємо, що у гравця є PlayerController або схожий компонент
        // Краще використовувати інтерфейс, наприклад IExperienceReceiver
        var player = target.GetComponent<PlayerController>();
        if (player != null) player.GainExperience(finalXP);
    }

    protected void PlayAnimationSafe(string animName, float transition = 0.1f)
    {
        if (currentAnimState == animName) return;
        currentAnimState = animName;

        if (_animService != null && animator != null)
            _animService.PlayAnimation(animator, animName, transition);
        else if (animator != null)
            animator.CrossFade(animName, transition);
    }

    private void SearchForPlayerLogic()
    {
        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0)
        {
            FindPlayer();
            searchTimer = 1f;
        }
    }

    protected void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) target = playerObj.transform;
    }

    protected void ResetAnimState() => currentAnimState = "";

    // Віртуальна корутина для звуків кроків (Бос може перевизначити таймінги)
    protected virtual IEnumerator FootstepSoundRoutine()
    {
        while (!isDead)
        {
            if (agent != null && agent.enabled && agent.velocity.magnitude > 0.5f && !isAttacking)
            {
                if (_audioService != null) _audioService.PlaySFX(sfxFootstep, transform.position);
                yield return new WaitForSeconds(0.5f); // Стандартний інтервал
            }
            else
            {
                yield return null;
            }
        }
    }

    // Гізмо малюються однаково для всіх
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}