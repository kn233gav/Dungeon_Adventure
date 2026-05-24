public enum StatType
{
    MaxHealth,
    MaxStamina,
    StaminaRegen,
    MoveSpeed,
    AttackDamage,
    AttackSpeed, // Впливатиме на attackDuration (швидше = менша тривалість)
    ExperienceGain
}

public enum ModifierMode
{
    PercentAdd, // Наприклад 0.10 це +10%
    FlatAdd     // Наприклад 10 це +10 одиниць
}