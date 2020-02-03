using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class Localizer : MonoBehaviour {

    public List<LocSettings> LocaleSettings;

    public TextMeshProUGUI NewGameText1;
    public TextMeshProUGUI NewGameText2;
    public TextMeshProUGUI HighScoreText;
    public TextMeshProUGUI GoodJobText;
    public TextMeshProUGUI PurchasePromptText;
    public TextMeshProUGUI LevelText;

    //--------------------------------------------------------------------------------------------------------
    private void Start() {
        //LoadLocale(Application.systemLanguage);
        LoadLocale(SystemLanguage.Spanish);
    }

    //--------------------------------------------------------------------------------------------------------
    private void LoadLocale(SystemLanguage lang) {
        foreach (LocSettings locale in LocaleSettings) {
            if (locale.Locale == lang) {
                NewGameText1.text = locale.NewGameString;
                NewGameText2.text = locale.NewGameString;
                HighScoreText.text = locale.HighScoreString;
                GoodJobText.text = locale.GoodJobString;
                PurchasePromptText.text = locale.PurchasePromptString;
                LevelText.text = locale.LevelString;
                return;
            }
        }
        
        // if we've gotten here, that means the system's locale was not found - default back to enUS
        LoadLocale(SystemLanguage.English);
    }
}
