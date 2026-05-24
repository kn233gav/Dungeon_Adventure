using UnityEngine;
using System;

public class PlayerStatsManager : MonoBehaviour, IDamageable
{
    public event Action OnDeath;
    public event Action<float> OnDamageTaken;
    public event Action OnLevelUp;
    public event Action OnStatsChanged;
    public event Action OnPotionUsed;

    [Header("Base Data")]
    [SerializeField] private CharacterStatsSO _baseStatsSO;

    [Header("Level Up Bonuses")]
    [SerializeField] private float _bonusHealthFromUpgrades = 0f;
    [SerializeField] private float _bonusStaminaFromUpgrades = 0f;
    [SerializeField] private float _bonusDamageFromUpgrades = 0f;

    [Header("Final Calculated Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    public float maxStamina = 100f;
    public float currentStamina;

    // --- НОВІ ОБРАХОВАНІ СТАТИ ---
    public float staminaRegenRate = 15f;
    public float currentMoveSpeed = 6f;       // Для передачі в Controller
    public float currentAttackDuration = 0.6f; // Чим вища AttackSpeed, тим менше це число

    public bool isRegenPaused = false;

    [Header("Combat Stats")]
    public float attackDamage = 10f;
    public float attackStaminaCost = 10f;
    public bool isInvulnerable = false;

    [Header("XP System")]
    public int currentLevel = 1;
    public float currentXP = 0;
    public float xpToNextLevel = 100f;

    [Header("Inventory")]
    public int potionCount = 2;

    private bool _isInitialized = false; // Додай цю змінну

    private void Start()
    {
        // Перевіряємо, чи вже була ініціалізація ззовні (через SpawnPlayerAt)
        if (!_isInitialized && _baseStatsSO != null)
        {
            InitStats(_baseStatsSO);
        }
        NotifyUI();
    }

    private void Update()
    {
        if (!isRegenPaused && currentStamina < maxStamina)
        {
            // Використовуємо staminaRegenRate, який тепер може буститись артефактами
            currentStamina += staminaRegenRate * Time.deltaTime;

            if (currentStamina > maxStamina) currentStamina = maxStamina;
            NotifyUI();
        }
    }
    // У класі PlayerStatsManager додаємо:

    // 1. Запаковуємо поточні дані
    public PlayerRunData GetSaveData()
    {
        PlayerRunData data = new PlayerRunData();
        data.currentHealth = currentHealth;
        data.currentLevel = currentLevel;
        data.currentXP = currentXP;
        data.xpToNextLevel = xpToNextLevel;
        data.potionCount = potionCount;

        data.bonusHealth = _bonusHealthFromUpgrades;
        data.bonusStamina = _bonusStaminaFromUpgrades;
        data.bonusDamage = _bonusDamageFromUpgrades;

        return data;
    }

    // 2. Розпаковуємо дані (замість InitStats)
    public void LoadFromSaveData(CharacterStatsSO baseSO, PlayerRunData data)
    {
        _isInitialized = true; 

        _baseStatsSO = baseSO;

        _bonusHealthFromUpgrades = data.bonusHealth;
        _bonusStaminaFromUpgrades = data.bonusStamina;
        _bonusDamageFromUpgrades = data.bonusDamage;

        currentLevel = data.currentLevel;
        currentXP = data.currentXP;
        xpToNextLevel = data.xpToNextLevel;
        potionCount = data.potionCount;

        RecalculateStats();

        currentHealth = Mathf.Min(data.currentHealth, maxHealth);

        currentStamina = maxStamina;

        NotifyUI();
    }
    public void InitStats(CharacterStatsSO stats)
    {
        _isInitialized = true; 
        _baseStatsSO = stats;

        _bonusHealthFromUpgrades = 0f;
        _bonusStaminaFromUpgrades = 0f;
        _bonusDamageFromUpgrades = 0f;

        currentLevel = 1;
        currentXP = 0f;
        xpToNextLevel = 100f;

        RecalculateStats();

        currentHealth = maxHealth;
        currentStamina = maxStamina;

        NotifyUI();
    }

    public void RecalculateStats()
    {
        if (_baseStatsSO == null) return;

        float baseH = _baseStatsSO.maxHealth + _bonusHealthFromUpgrades;
        float baseS = _baseStatsSO.maxStamina + _bonusStaminaFromUpgrades;
        float baseD = 10f + _bonusDamageFromUpgrades; 

        float baseRegen = 15f;
        float baseMove = _baseStatsSO.moveSpeed;
        float baseAtkDuration = _baseStatsSO.attackDuration;

        if (GameManager.Instance != null)
        {
            maxHealth = GameManager.Instance.CalculateStat(baseH, StatType.MaxHealth);
            maxStamina = GameManager.Instance.CalculateStat(baseS, StatType.MaxStamina);

            attackDamage = GameManager.Instance.CalculateStat(baseD, StatType.AttackDamage);


            staminaRegenRate = GameManager.Instance.CalculateStat(baseRegen, StatType.StaminaRegen);

            currentMoveSpeed = GameManager.Instance.CalculateStat(baseMove, StatType.MoveSpeed);

            float speedMult = GameManager.Instance.CalculateStat(1f, StatType.AttackSpeed);

            currentAttackDuration = baseAtkDuration / Mathf.Max(0.1f, speedMult);
        }
        else
        {
            maxHealth = baseH;
            maxStamina = baseS;
            attackDamage = baseD;
            staminaRegenRate = baseRegen;
            currentMoveSpeed = baseMove;
            currentAttackDuration = baseAtkDuration;
        }
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        if (currentStamina > maxStamina) currentStamina = maxStamina;
    }

    public void GainExperience(float amount)
    {
        float multiplier = 1f;
        if (GameManager.Instance != null)
            multiplier = GameManager.Instance.GetTotalXPMultiplier();

        currentXP += amount * multiplier;
        if (currentXP >= xpToNextLevel) LevelUp();
        NotifyUI();
    }

    public void ApplyUpgradeStat(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Health:
                _bonusHealthFromUpgrades += 20f;
                Heal(20f);
                break;
            case UpgradeType.Stamina:
                _bonusStaminaFromUpgrades += 20f;
                currentStamina += 20f;
                break;
            case UpgradeType.Damage:
                _bonusDamageFromUpgrades += 5f;
                break;
        }
        RecalculateStats();
        NotifyUI();
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0 || isInvulnerable) return;
        currentHealth -= amount;
        OnDamageTaken?.Invoke(amount);
        NotifyUI();
        if (currentHealth <= 0) { currentHealth = 0; OnDeath?.Invoke(); }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        NotifyUI();
    }

    public bool HasStamina(float amount) => currentStamina >= amount;

    public void ConsumeStamina(float amount)
    {
        currentStamina = Mathf.Max(0, currentStamina - amount);
        NotifyUI();
    }
    public void LevelUp()
    {
        currentXP -= xpToNextLevel;
        currentLevel++;
        xpToNextLevel *= 1.2f; 
        OnLevelUp?.Invoke();
        NotifyUI();
    }

    public void AddPotion(int amount) { potionCount += amount; OnPotionUsed?.Invoke(); }

    public bool TryUsePotion()
    {
        if (potionCount > 0 && currentHealth < maxHealth)
        {
            potionCount--;
            Heal(maxHealth * 0.2f);
            OnPotionUsed?.Invoke();
            return true;
        }
        return false;
    }

    private void NotifyUI() => OnStatsChanged?.Invoke();
}