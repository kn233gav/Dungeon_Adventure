using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ArtifactModifier
{
    public StatType statType;
    public ModifierMode mode;
    public float value;
}

// Тип розблокування: за замовчуванням, за ачівку, або дроп з боса
public enum ArtifactUnlockType { Default, Achievement, BossDrop }

[CreateAssetMenu(fileName = "NewArtifact", menuName = "Game/Artifact")]
public class ArtifactSO : ScriptableObject
{
    [Header("System ID")]
    // Унікальний ідентифікатор (наприклад: "boots_speed"). НЕ ЗМІНЮЙ ЙОГО після релізу гри!
    public string artifactID;
    [Header("Для меню ачівок")]
    [TextArea] public string unlockConditionDescription; // Наприклад: "Пройти гру на Легкій складності" або "Випадає з Боса"
    [Header("Unlock Settings")]
    public ArtifactUnlockType unlockType;
    // Якщо Achievement — тут пишемо ID ачівки (наприклад: "rank_s_floor2")
    // Якщо BossDrop — це поле ігнорується, скрипт сам обере випадковий
    public string achievementConditionID;
    [Range(0, 100)] public float dropChance = 10f; // Шанс випадіння (якщо це BossDrop)

    [Header("UI Info")]
    public string artifactName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Bonuses")]
    public List<ArtifactModifier> modifiers;
}