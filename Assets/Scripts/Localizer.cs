using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class Localizer : MonoBehaviour {

    public void SetLanguage(LocalizedLanguage language) {
        LocalizedTextController.SetGlobalLanguage(language);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SetLanguageEnglish() {
        LocalizedTextController.SetGlobalLanguage(LocalizedLanguage.English);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SetLanguageSpanish() {
        LocalizedTextController.SetGlobalLanguage(LocalizedLanguage.Spanish);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SetLanguageGerman() {
        LocalizedTextController.SetGlobalLanguage(LocalizedLanguage.German);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SetLanguageRussian() {
        LocalizedTextController.SetGlobalLanguage(LocalizedLanguage.Russian);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SetLanguageHindi() {
        LocalizedTextController.SetGlobalLanguage(LocalizedLanguage.Hindi);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SetLanguageIndonesian() {
        LocalizedTextController.SetGlobalLanguage(LocalizedLanguage.Indonesian);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SetLanguageChinese() {
        LocalizedTextController.SetGlobalLanguage(LocalizedLanguage.Chinese);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SetLanguageKorean() {
        LocalizedTextController.SetGlobalLanguage(LocalizedLanguage.Korean);
    }
}
