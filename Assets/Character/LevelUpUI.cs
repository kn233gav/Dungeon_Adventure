using UnityEngine;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button healthButton;
    public Button staminaButton;
    public Button damageButton;

    private PlayerController player;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Initialize(PlayerController playerRef)
    {
        player = playerRef;


        healthButton.onClick.RemoveAllListeners();
        staminaButton.onClick.RemoveAllListeners();
        damageButton.onClick.RemoveAllListeners();

        healthButton.onClick.AddListener(() => SelectUpgrade(UpgradeType.Health));
        staminaButton.onClick.AddListener(() => SelectUpgrade(UpgradeType.Stamina));
        damageButton.onClick.AddListener(() => SelectUpgrade(UpgradeType.Damage));
    }

    void SelectUpgrade(UpgradeType type)
    {
        if (player != null)
        {
            player.ApplyUpgrade(type);

            //gameObject.SetActive(false);
        }
    }
}

public enum UpgradeType
{
    Health,
    Stamina,
    Damage
}