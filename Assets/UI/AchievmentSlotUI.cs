using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public GameObject checkMark;
    public GameObject lockIcon;
    public Image backgroundImage;
    public GameObject rewardIcon; // Маленька іконка подарунка, якщо є артефакт

    public void Setup(AchievementSO achievement, bool isCompleted)
    {
        iconImage.sprite = achievement.icon;
        nameText.text = achievement.displayName;
        descText.text = achievement.description;

        // Показуємо, чи є нагорода за цю ачівку
        if (rewardIcon) rewardIcon.SetActive(achievement.rewardArtifact != null);

        if (isCompleted)
        {
            nameText.color = Color.white;
            descText.color = Color.green;
            iconImage.color = Color.white;

            if (checkMark) checkMark.SetActive(true);
            if (lockIcon) lockIcon.SetActive(false);
            if (backgroundImage) backgroundImage.color = new Color(0, 0, 0, 0.5f);
        }
        else
        {
            nameText.color = Color.gray;
            descText.color = Color.gray;
            iconImage.color = Color.black; // Силует

            if (checkMark) checkMark.SetActive(false);
            if (lockIcon) lockIcon.SetActive(true);
            if (backgroundImage) backgroundImage.color = new Color(0, 0, 0, 0.8f);
        }
    }
}