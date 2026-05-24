using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(PlayerStatsManager))]
public class PlayerController : MonoBehaviour
{
    public PlayerStatsManager stats { get; private set; }
    public Rigidbody rb { get; private set; }
    public Transform cam { get; private set; }
    public Animator animator;

    private IAudioService _audioService;
    private IEventService _eventService;

    [Header("Settings - Movement")]
    public float moveSpeed = 6f;
    public float sprintSpeed = 10f;
    public float combatMoveSpeed = 4f;
    public float acceleration = 10f;
    public float deceleration = 8f;
    public float jumpForce = 8f;

    [Header("Settings - Combat")]
    public bool isRangedCharacter = false;
    public Transform attackPoint;
    public float meleeAttackRadius = 1.5f;
    public LayerMask enemyLayers;
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;
    public float bulletRange = 50f;
    public float attackDuration = 0.6f;
    public float iFrameDuration = 1.5f;
    public GameObject attackTrailVFX;

    [Header("Physics Checks")]
    public float groundCheckDistance = 1.1f;
    public LayerMask groundMask;
    public float airControlFactor = 0.5f;

    [Header("Audio Names")]
    public string walkSoundName = "Step_Human";
    public string runSoundName = "Run_Human";
    public string deathSoundName = "Death_Human";
    public string attackSoundName = "Attack_Default";

    [Header("UI Linking")]
    public LevelUpUI levelUpScreen;
    public Slider healthBar;
    public Slider staminaBar;
    public Slider xpSlider;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI potionUI;
    public TextMeshProUGUI stateDebugText;

    private IPlayerState currentState;
    public PlayerIdleState idleState;
    public PlayerMovingState movingState;
    public PlayerSprintingState sprintingState;
    public PlayerCombatState combatState;
    public PlayerJumpingState jumpingState;
    public PlayerLevelUpState levelUpState;
    public PlayerPauseState pauseState;

    [HideInInspector] public Vector3 moveDirection;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool jumpInput;
    [HideInInspector] public bool attackInput;
    [HideInInspector] public bool sprintInput;

    private bool isDead = false;
    public bool IsRegenPaused
    {
        get => stats.isRegenPaused;
        set => stats.isRegenPaused = value;
    }

    [Header("Movement Costs")]
    public float sprintStaminaCost = 20f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        cam = Camera.main.transform;
        stats = GetComponent<PlayerStatsManager>();

        idleState = new PlayerIdleState(this);
        movingState = new PlayerMovingState(this);
        sprintingState = new PlayerSprintingState(this);
        combatState = new PlayerCombatState(this);
        jumpingState = new PlayerJumpingState(this);
        levelUpState = new PlayerLevelUpState(this);
        pauseState = new PlayerPauseState(this);
    }

    void Start()
    {
        try
        {
            _audioService = ServiceLocator.Get<IAudioService>();
            _eventService = ServiceLocator.Get<IEventService>();
        }
        catch { }

        if (levelUpScreen == null) levelUpScreen = FindFirstObjectByType<LevelUpUI>();

        stats.OnDeath += HandleDeath;
        stats.OnDamageTaken += HandleDamage;
        stats.OnLevelUp += HandleLevelUp;
        stats.OnStatsChanged += UpdateUI;
        stats.OnPotionUsed += UpdatePotionUI;

        if (healthBar == null)
        {
            HUDManager hud = FindFirstObjectByType<HUDManager>();
            if (hud != null)
            {
                healthBar = hud.healthBar;
                staminaBar = hud.staminaBar;
                xpSlider = hud.xpSlider;
                levelText = hud.levelText;
                if (hud.potionCountText != null) potionUI = hud.potionCountText;
                if (hud.stateDebug != null) stateDebugText = hud.stateDebug;
            }
        }
        if (levelUpScreen != null) levelUpScreen.Initialize(this);
        if (attackTrailVFX != null) attackTrailVFX.SetActive(false);

        UpdateUI();
        UpdatePotionUI();
        ChangeState(idleState);
    }

    public void Initialize(CharacterStatsSO data)
    {
        moveSpeed = data.moveSpeed;
        sprintSpeed = data.sprintSpeed;
        combatMoveSpeed = data.combatMoveSpeed;
        attackDuration = data.attackDuration;
        bulletPrefab = data.projectilePrefab;
        bulletSpeed = data.projectileSpeed;
        bulletRange = data.attackRange;
        stats.InitStats(data);
    }

    void Update()
    {
        if (isDead) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 forward = cam.forward;
        Vector3 right = cam.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();
        moveDirection = (forward * v + right * h).normalized;

        jumpInput = Input.GetKeyDown(KeyCode.Space);
        attackInput = Input.GetMouseButtonDown(0);
        sprintInput = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKeyDown(KeyCode.E)) stats.TryUsePotion();

        if (Input.GetKeyDown(KeyCode.Escape) && !(currentState is PlayerLevelUpState))
        {
            if (currentState is PlayerPauseState) ChangeState(idleState);
            else ChangeState(pauseState);
            return;
        }

        CheckGround();
        currentState.HandleInput();
        currentState.Update();
    }

    void FixedUpdate()
    {
        if (isDead) return;
        currentState.FixedUpdate();
    }
    public void ApplyUpgrade(UpgradeType type)
    {
        stats.ApplyUpgradeStat(type);
        if (stats.currentXP >= stats.xpToNextLevel)
        {
            stats.LevelUp();
        }
        else
        {
            CloseLevelUpMenu();
        }
    }

    private void CloseLevelUpMenu()
    {
        if (levelUpScreen != null)
            levelUpScreen.gameObject.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        ChangeState(idleState);
    }
    public void ApplyMovement(float speed, float controlFactor = 1f)
    {
        Vector3 targetVelocity = moveDirection * speed;
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 newHorizontalVelocity = Vector3.Lerp(
            new Vector3(currentVelocity.x, 0, currentVelocity.z),
            targetVelocity,
            acceleration * controlFactor * Time.fixedDeltaTime
        );
        rb.linearVelocity = new Vector3(newHorizontalVelocity.x, currentVelocity.y, newHorizontalVelocity.z);
    }

    public void ApplyDeceleration()
    {
        if (moveDirection.magnitude < 0.1f)
        {
            Vector3 currentVelocity = rb.linearVelocity;
            rb.linearVelocity = new Vector3(
                Mathf.Lerp(currentVelocity.x, 0, deceleration * Time.fixedDeltaTime),
                currentVelocity.y,
                Mathf.Lerp(currentVelocity.z, 0, deceleration * Time.fixedDeltaTime)
            );
        }
    }

    public void HandleRotation()
    {
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 15f * Time.fixedDeltaTime));
        }
    }

    public void Jump() => ChangeState(jumpingState);

    private void CheckGround()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);
    }

    public void StartAttack()
    {
        if (animator != null) animator.CrossFade("Attack", 0.1f);
        if (attackTrailVFX != null && !isRangedCharacter) attackTrailVFX.SetActive(true);
        if (!isRangedCharacter) StartCoroutine(MeleeAttackDelay(0.3f));
    }

    private IEnumerator MeleeAttackDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnAttackEvent();
        yield return new WaitForSeconds(0.3f);
        if (attackTrailVFX != null) attackTrailVFX.SetActive(false);
    }

    public void OnFootstep()
    {
        if (!isGrounded || isDead || _audioService == null) return;
        string soundToPlay = walkSoundName;
        if (currentState is PlayerSprintingState) soundToPlay = runSoundName;
        _audioService.PlaySFX(soundToPlay, transform.position);
    }

    public void OnAttackEvent()
    {
        if (_audioService != null) _audioService.PlaySFX(attackSoundName, transform.position);
        if (isRangedCharacter) FireProjectile();
        else MeleeStrike();
    }

    private void FireProjectile()
    {
        if (bulletPrefab != null && attackPoint != null)
        {
            GameObject proj = PoolManager.Instance != null
                ? PoolManager.Instance.Spawn(bulletPrefab, attackPoint.position, transform.rotation)
                : Instantiate(bulletPrefab, attackPoint.position, transform.rotation);
            Projectile pScript = proj.GetComponent<Projectile>();
            if (pScript) pScript.Setup(bulletSpeed, bulletRange, stats.attackDamage);
        }
    }

    private void MeleeStrike()
    {
        if (attackPoint != null)
        {
            Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, meleeAttackRadius, enemyLayers);
            foreach (Collider enemy in hitEnemies)
            {
                IDamageable dmg = enemy.GetComponent<IDamageable>() ?? enemy.GetComponentInParent<IDamageable>();
                dmg?.TakeDamage(stats.attackDamage);
            }
        }
    }

    private void HandleDamage(float amount)
    {
        if (_eventService != null) _eventService.TriggerPlayerTookDamage(amount);
        StartCoroutine(InvulnerabilityRoutine());
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        stats.isInvulnerable = true;
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            float elapsed = 0f;
            while (elapsed < iFrameDuration)
            {
                rend.enabled = !rend.enabled;
                yield return new WaitForSeconds(0.15f);
                elapsed += 0.15f;
            }
            rend.enabled = true;
        }
        stats.isInvulnerable = false;
    }

    private void HandleDeath()
    {
        isDead = true;
        if (_audioService != null) _audioService.PlaySFX(deathSoundName, transform.position);
        if (_eventService != null) _eventService.TriggerPlayerDied();
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        if (animator != null) animator.CrossFade("Death", 0.1f);
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void HandleLevelUp() => ChangeState(levelUpState);

    public void UpdateUI()
    {
        if (healthBar) healthBar.value = stats.currentHealth / stats.maxHealth;
        if (staminaBar) staminaBar.value = stats.currentStamina / stats.maxStamina;
        if (xpSlider)
        {
            xpSlider.maxValue = stats.xpToNextLevel;
            xpSlider.value = stats.currentXP;
        }
        if (levelText) levelText.text = "Lvl " + stats.currentLevel;
    }

    private void UpdatePotionUI()
    {
        if (HUDManager.Instance != null && HUDManager.Instance.potionCountText != null)
        {
            HUDManager.Instance.potionCountText.text = stats.potionCount.ToString();
            return;
        }
        GameObject go = GameObject.Find("PotionUI");
        if (go != null)
        {
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.text = stats.potionCount.ToString();
        }
    }

    public void GainExperience(float amount) => stats.GainExperience(amount);
    public void AddPotion(int amount) => stats.AddPotion(amount);

    public bool HasStamina(float amount) => stats.HasStamina(amount);
    public float currentStamina => stats.currentStamina;
    public float attackStaminaCost => stats.attackStaminaCost;

    public void ConsumeStamina(float amount) => stats.ConsumeStamina(amount);
    public void RegenerateStamina() { }

    public void ChangeState(IPlayerState newState)
    {
        currentState?.Exit();
        currentState = newState;
        if (stateDebugText) stateDebugText.text = $"State: {newState.GetType().Name}";
        currentState.Enter();
    }
}