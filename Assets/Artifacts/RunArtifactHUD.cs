using UnityEngine;
using System.Collections.Generic;

public class RunArtifactHUD : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Контейнер, де будуть створюватись іконки (має мати Layout Group)")]
    public Transform iconsContainer;

    [Tooltip("Той самий префаб слоту, що і в меню")]
    public ArtifactSlotUI slotPrefab;

    private void Start()
    {
        if (RunManager.Instance != null)
        {
            DisplayArtifacts();
        }
        else
        {
            Debug.LogError("RunArtifactHUD: RunManager не знайдено!");
        }
    }

    private void DisplayArtifacts()
    {
        foreach (Transform child in iconsContainer)
        {
            Destroy(child.gameObject);
        }

        List<ArtifactSO> currentArtifacts = RunManager.Instance.equippedArtifacts;

        if (currentArtifacts == null || currentArtifacts.Count == 0) return;

        foreach (ArtifactSO art in currentArtifacts)
        {
            ArtifactSlotUI newSlot = Instantiate(slotPrefab, iconsContainer);
            newSlot.SetupForDisplay(art);
        }
    }
}