using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactMenuManager : MonoBehaviour
{
    [Header("Налаштування")]
    public GameObject slotPrefab;
    public Transform gridContainer;
    public List<ArtifactSO> allArtifacts; 

    [Header("UI Активних слотів (Верхні 3)")]
    public Image[] activeSlotsUI;
    public Sprite emptySlotSprite;

    private List<ArtifactSlotUI> spawnedSlots = new List<ArtifactSlotUI>();

    void Start()
    {
        GenerateGrid();
        UpdateUI();
    }
    private void OnEnable()
    {
        if (spawnedSlots.Count > 0) UpdateUI();
    }

    void GenerateGrid()
    {
        foreach (Transform child in gridContainer) Destroy(child.gameObject);
        spawnedSlots.Clear();

        foreach (var artifact in allArtifacts)
        {
            GameObject newSlot = Instantiate(slotPrefab, gridContainer);
            ArtifactSlotUI slotScript = newSlot.GetComponent<ArtifactSlotUI>();

            bool unlocked = GlobalProgressionManager.Instance.IsArtifactUnlocked(artifact.artifactID);
            slotScript.Setup(artifact, OnArtifactClicked, unlocked);

            spawnedSlots.Add(slotScript);
        }
    }

    void OnArtifactClicked(ArtifactSO clickedArtifact)
    {
        if (!GlobalProgressionManager.Instance.IsArtifactUnlocked(clickedArtifact.artifactID))
        {
            Debug.Log($"Артефакт {clickedArtifact.artifactName} ще заблоковано!");
            return;
        }

        List<ArtifactSO> equipped = GameManager.Instance.equippedArtifacts;

        if (equipped.Contains(clickedArtifact))
        {
            equipped.Remove(clickedArtifact); 
        }
        else
        {
            if (equipped.Count < 3)
            {
                equipped.Add(clickedArtifact); 
            }
            else
            {
                Debug.Log("Слоти заповнені! Зніміть щось спочатку.");
            }
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        List<ArtifactSO> equipped = GameManager.Instance.equippedArtifacts;
        for (int i = 0; i < activeSlotsUI.Length; i++)
        {
            if (i < equipped.Count)
            {
                activeSlotsUI[i].sprite = equipped[i].icon;
                activeSlotsUI[i].enabled = true;
            }
            else
            {
                activeSlotsUI[i].sprite = emptySlotSprite;
                activeSlotsUI[i].enabled = (emptySlotSprite != null);
            }
        }

        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            bool isSelected = equipped.Contains(allArtifacts[i]);
            spawnedSlots[i].UpdateSelectionVisual(isSelected);
        }
    }
}