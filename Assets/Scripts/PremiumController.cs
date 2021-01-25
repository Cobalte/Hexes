using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PremiumController : MonoBehaviour {

    public bool GameIsPremium { get; private set; }
    
    //--------------------------------------------------------------------------------------------------------
    private void Start() {
        GameIsPremium = false;
        VerifyPremiumStatus();
    }

    //--------------------------------------------------------------------------------------------------------
    private void VerifyPremiumStatus() {
        Debug.Log("Game is in premium mode: " + GameIsPremium);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void GoPremium() {
        Debug.Log("TODO: Execute premium purchase.");
        
        GameIsPremium = true;
        VerifyPremiumStatus();
    }
}
