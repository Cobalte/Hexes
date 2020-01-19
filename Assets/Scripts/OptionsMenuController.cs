using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenuController : MonoBehaviour
{
    public GameObject optionsMenuObj;
    public bool optionsMenuStatus;
    
    // Start is called before the first frame update
    void Start()
    {
        if (optionsMenuObj != null)
        {
            optionsMenuObj.SetActive(false);
            optionsMenuStatus = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleOptionsMenu()
    {
        optionsMenuObj.SetActive(!optionsMenuStatus);
        optionsMenuStatus = !optionsMenuStatus;
    }
    
}
