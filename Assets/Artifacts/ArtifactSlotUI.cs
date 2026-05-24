using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ArtifactSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image iconImage;       
    public Image borderImage;     
    public GameObject lockIcon;   
    public Button button;

    private ArtifactSO myArtifact;
    private bool isUnlocked;
    private UnityAction<ArtifactSO> onSlotClicked;

    public void Setup(ArtifactSO artifact, UnityAction<ArtifactSO> clickCallback, bool unlocked)
    {
        myArtifact = artifact;
        onSlotClicked = clickCallback;
        isUnlocked = unlocked;

        if (myArtifact != null)
        {
            iconImage.sprite = myArtifact.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }

        if (isUnlocked)
        {
            iconImage.color = Color.white;
            if (lockIcon != null) lockIcon.SetActive(false);
            button.interactable = true;
        }
        else
        {
            iconImage.color = Color.black;
            if (lockIcon != null) lockIcon.SetActive(true);
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onSlotClicked.Invoke(myArtifact));
    }

    public void UpdateSelectionVisual(bool isSelected)
    {
        if (borderImage != null)
        {
            if (!isUnlocked)
            {
                borderImage.color = new Color(0, 0, 0, 0); 
                return;
            }

            borderImage.color = isSelected ? Color.green : new Color(1, 1, 1, 0f);
        }
    }

    public void SetupForDisplay(ArtifactSO artifact)
    {
        myArtifact = artifact;
        isUnlocked = true; 

        if (myArtifact != null)
        {
            iconImage.sprite = myArtifact.icon;
            iconImage.enabled = true;
        }

        if (lockIcon != null) lockIcon.SetActive(false);
        if (borderImage != null) borderImage.color = new Color(0, 0, 0, 0); 

        if (button != null)
        {
            button.interactable = false;
        }
        iconImage.color = Color.white;
    }
}