using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenuController : MonoBehaviour
{
    public GameObject OptionsMenuObj;
    public Animator OptionsMenuAnimator;
    public PremiumController PremiumController;
    public GameObject PurchasePromptText;
    public GameObject PurchasePromptButton;
    public GameObject PurchaseConfirmText;
    public GameObject PrivacyOptionsButton;

    // when the privacy options button is shown, the settings panel stretches by one row to fit it
    [Tooltip("Height the privacy options row adds to the settings panel (button + spacing).")]
    public float PrivacyRowHeight = 85f;
    [Tooltip("How much of that extra height the settings panel grows upward. The rest grows downward.")]
    public float SettingsGrowUpward = 40f;
    public RectTransform SettingsPanel;
    [Tooltip("Rows inside the settings panel that move up to sit above the privacy options button.")]
    public List<RectTransform> RaiseWhenPrivacyShown;
    [Tooltip("Panels below the settings panel that move down to make room.")]
    public List<RectTransform> LowerWhenPrivacyShown;

    // rider says to do this for performance?
    private static readonly int statusHash = Animator.StringToHash("Status");

    private Vector2 settingsPanelPosition;
    private Vector2 settingsPanelSize;
    private readonly List<Vector2> raisedRowPositions = new List<Vector2>();
    private readonly List<Vector2> loweredPanelPositions = new List<Vector2>();

    //--------------------------------------------------------------------------------------------------------
    private void Awake() {
        // remember the layout without the privacy options row so we can switch between the two
        if (SettingsPanel != null) {
            settingsPanelPosition = SettingsPanel.anchoredPosition;
            settingsPanelSize = SettingsPanel.sizeDelta;
        }
        foreach (RectTransform row in RaiseWhenPrivacyShown) {
            raisedRowPositions.Add(row.anchoredPosition);
        }
        foreach (RectTransform panel in LowerWhenPrivacyShown) {
            loweredPanelPositions.Add(panel.anchoredPosition);
        }
    }

    //--------------------------------------------------------------------------------------------------------
    private void Start() {
        UpdatePremiumText();
        UpdatePrivacyOptionsButton();
        OptionsMenuAnimator.SetBool(statusHash, false);
        OptionsMenuObj.SetActive(false);
    }

    //--------------------------------------------------------------------------------------------------------
    public void ToggleOptionsMenu() {
        OptionsMenuObj.SetActive(true);
        AnimatorSleeper.WakeUp(OptionsMenuAnimator);

        bool isVisible = OptionsMenuAnimator.GetBool(statusHash);
        OptionsMenuAnimator.SetBool(statusHash, !isVisible);

        if (isVisible) {
            UpdatePremiumText();
        }
        else {
            UpdatePrivacyOptionsButton();
        }
    }

    //--------------------------------------------------------------------------------------------------------
    private void UpdatePrivacyOptionsButton() {
        // only players covered by privacy rules (e.g. in the EEA/UK) get the button
        if (PrivacyOptionsButton == null) {
            return;
        }

        bool showButton = PremiumController.IsPrivacyOptionsRequired;
        PrivacyOptionsButton.SetActive(showButton);

        if (SettingsPanel == null) {
            return;
        }

        // stretch the settings panel by one row - partly upward, partly downward
        float grow = showButton ? PrivacyRowHeight : 0f;
        float growUp = showButton ? SettingsGrowUpward : 0f;
        float growDown = grow - growUp;

        SettingsPanel.sizeDelta = settingsPanelSize + new Vector2(0f, grow);
        SettingsPanel.anchoredPosition = settingsPanelPosition + new Vector2(0f, (growUp - growDown) / 2f);

        // the privacy policy/sound row moves up and the privacy options button takes its old spot
        for (int i = 0; i < RaiseWhenPrivacyShown.Count; i++) {
            RaiseWhenPrivacyShown[i].anchoredPosition = raisedRowPositions[i] + new Vector2(0f, grow);
        }

        // everything below the settings panel moves down by however much it grew downward
        for (int i = 0; i < LowerWhenPrivacyShown.Count; i++) {
            LowerWhenPrivacyShown[i].anchoredPosition = loweredPanelPositions[i] - new Vector2(0f, growDown);
        }
    }

    //--------------------------------------------------------------------------------------------------------
    public void OpenPrivacyOptions() {
        PremiumController.ShowPrivacyOptions();
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void UpdatePremiumText() {
        // update the premium window depending on whether the player has paid us or not
        PurchaseConfirmText.SetActive(PremiumController.IsGamePremium);
        PurchasePromptText.SetActive(!PremiumController.IsGamePremium);
        PurchasePromptButton.SetActive(!PremiumController.IsGamePremium);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void OpenPrivacyPolicy() {
        Application.OpenURL("https://www.cloakquillgames.com/privacy-policy");
    }
}