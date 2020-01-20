using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenuController : MonoBehaviour
{
    public GameObject OptionsMenuObj;
    public Animator OptionsMenuAnimator;
    
    // rider says to do this for performance?
    private static readonly int statusHash = Animator.StringToHash("Status");

    private void Start() {
        OptionsMenuAnimator.SetBool(statusHash, false);
        OptionsMenuObj.SetActive(false);
    }

    public void ToggleOptionsMenu() {
        OptionsMenuObj.SetActive(true);
        OptionsMenuAnimator.SetBool(statusHash, !OptionsMenuAnimator.GetBool(statusHash));
    }
}