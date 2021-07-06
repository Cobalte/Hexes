using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;

public class Localizer : MonoBehaviour {

    public LocalizedLanguage CurrentLanguage { get; private set; }

    private GameController gameController;
    
    //--------------------------------------------------------------------------------------------------------
    private void Awake() {
        gameController = GetComponent<GameController>();
    }

    //--------------------------------------------------------------------------------------------------------
    public void SetLanguage(LocalizedLanguage language) {
        LocalizedTextController.SetGlobalLanguage(language);
        CurrentLanguage = language;
        gameController.SaveGameState();
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void ResetLanguageToDeviceLanguage() {
        switch (Application.systemLanguage) {
            case SystemLanguage.Chinese:
                SetLanguageChinese();
                break;
            case SystemLanguage.ChineseSimplified:
                SetLanguageChinese();
                break;
            case SystemLanguage.ChineseTraditional:
                SetLanguageChinese();
                break;
            case SystemLanguage.German:
                SetLanguageGerman();
                break;
            case SystemLanguage.Indonesian:
                SetLanguageIndonesian();
                break;
            case SystemLanguage.Korean:
                SetLanguageKorean();
                break;
            case SystemLanguage.Russian:
                SetLanguageRussian();
                break;
            case SystemLanguage.Spanish:
                SetLanguageSpanish();
                break;
            case SystemLanguage.Unknown:
                // is this hindi?
                try {
                    if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "hi") {
                        SetLanguageHindi();
                    }
                    else {
                        SetLanguageEnglish();
                    }
                }
                catch {
                    SetLanguageEnglish();
                }
                break;
            default:
                SetLanguageEnglish();
                break;
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SetLanguageEnglish() {
        SetLanguage(LocalizedLanguage.English);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SetLanguageSpanish() {
        SetLanguage(LocalizedLanguage.Spanish);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SetLanguageGerman() {
        SetLanguage(LocalizedLanguage.German);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SetLanguageRussian() {
        SetLanguage(LocalizedLanguage.Russian);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SetLanguageHindi() {
        SetLanguage(LocalizedLanguage.Hindi);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SetLanguageIndonesian() {
        SetLanguage(LocalizedLanguage.Indonesian);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SetLanguageChinese() {
        SetLanguage(LocalizedLanguage.Chinese);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SetLanguageKorean() {
        SetLanguage(LocalizedLanguage.Korean);
    }
}
