using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject confirmationPopup; 

    [Header("Buttons")]
    public Button resetProgressButton;
    public Button confirmYesButton;
    public Button confirmNoButton;

    private void Start()
    {
        if (confirmationPopup != null) confirmationPopup.SetActive(false);
    }

    public void OnResetParamsClicked()
    {

        confirmationPopup.SetActive(true);
    }

    public void OnConfirmResetClicked()
    {
        if (GlobalProgressionManager.Instance != null)
        {
            GlobalProgressionManager.Instance.ResetGlobalProgress();
        }
        confirmationPopup.SetActive(false);
    }
    public void OnCancelResetClicked()
    {
        confirmationPopup.SetActive(false);
    }
}