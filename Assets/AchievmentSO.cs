using UnityEngine;

public enum AchievementType
{
    RunCondition,   // Наприклад: пройти без втрати HP, S-ранг (перевіряється в кінці забігу)
    GlobalStat      // Наприклад: вбити 1000 ворогів сумарно (перевіряється накопичувально)
}

[CreateAssetMenu(fileName = "New Achievement", menuName = "Game/Achievement")]
public class AchievementSO : ScriptableObject
{
    [Header("General Info")]
    public string id;                   // Унікальний ID: "kill_100_enemies"
    public string displayName;          // "М'ясник"
    [TextArea] public string description; // "Вбити 100 ворогів"
    public Sprite icon;                 // Іконка ачівки

    [Header("Requirements")]
    public AchievementType type;
    public string conditionID;          // Для RunCondition: "rank_s", "no_damage"
    public int targetValue;             // Для GlobalStat: 100, 500, 1000

    [Header("Reward (Optional)")]
    public ArtifactSO rewardArtifact;   // Якщо пусте — це просто ачівка для престижу
}