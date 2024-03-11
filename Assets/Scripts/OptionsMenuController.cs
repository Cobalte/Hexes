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
    
    // rider says to do this for performance?
    private static readonly int statusHash = Animator.StringToHash("Status");

    //--------------------------------------------------------------------------------------------------------
    private void Start() {
        UpdatePremiumText();
        OptionsMenuAnimator.SetBool(statusHash, false);
        OptionsMenuObj.SetActive(false);
    }

    //--------------------------------------------------------------------------------------------------------
    public void ToggleOptionsMenu() {
        OptionsMenuObj.SetActive(true);

        bool isVisible = OptionsMenuAnimator.GetBool(statusHash);
        OptionsMenuAnimator.SetBool(statusHash, !isVisible);

        if (isVisible) {
            UpdatePremiumText();
        }
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