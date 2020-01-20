using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenuController : MonoBehaviour
{
    public GameObject optionsMenuObj;
    public bool optionsMenuStatus;
    public Animator optionsMenuAnimator;
    
    // Start is called before the first frame update
    void Start()
    {
        if (optionsMenuObj != null)
        {
            optionsMenuAnimator.SetBool("Status", false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleOptionsMenu()
    {
        optionsMenuAnimator.SetBool("Status", !optionsMenuAnimator.GetBool("Status"));
    }
    
}
