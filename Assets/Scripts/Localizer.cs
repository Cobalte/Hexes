using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Localizer : MonoBehaviour {

    public List<LocSettings> LocaleSettings;

    public TextMeshProUGUI NewGameText;
    public TextMeshProUGUI HighScoreText;
    public TextMeshProUGUI GoodJobText;
    public TextMeshProUGUI PurchasePromptText;

    //--------------------------------------------------------------------------------------------------------
    private void Start() {
        //LoadLocale(SystemLanguage.Spanish);
        LoadLocale(Application.systemLanguage);
    }

    //--------------------------------------------------------------------------------------------------------
    private void LoadLocale(SystemLanguage lang) {
        foreach (LocSettings locale in LocaleSettings) {
            if (locale.Locale == lang) {
                NewGameText.text = locale.NewGameString;
                HighScoreText.text = locale.HighScoreString;
                GoodJobText.text = locale.GoodJobString;
                PurchasePromptText.text = locale.PurchasePromptString;
                return;
            }
        }
        
        // if we've gotten here, that means the system's locale was not found - default back to enUS
        LoadLocale(SystemLanguage.English);
    }
}
