using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "LocSettings", order = 1)]
public class LocSettings : ScriptableObject {

    public SystemLanguage Locale;
    public string NewGameString;
    public string GoodJobString;
    public string HighScoreString;
    public string PurchasePromptString;

}
