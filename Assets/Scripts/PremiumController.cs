using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PremiumController : MonoBehaviour {

    public bool GameIsPremium { get; private set; }
    
    private void Start() {
        GameIsPremium = false;
        UpdatePremiumStatus();
    }

    private void UpdatePremiumStatus() {
        Debug.Log("TODO: Execute premium status query.");
    }
    
    public void GoPremium() {
        Debug.Log("TODO: Execute premium purchase.");
        GameIsPremium = true;
        UpdatePremiumStatus();
    }
}
