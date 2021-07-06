using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LocalizedTextController {

    private static List<LocalizedText> texts;

    //--------------------------------------------------------------------------------------------------------
    public static void RegisterText(LocalizedText newText) {
        texts ??= new List<LocalizedText>();
        texts.Add(newText);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public static void SetGlobalLanguage(LocalizedLanguage language) {
        texts ??= Resources.FindObjectsOfTypeAll<LocalizedText>().ToList();
        
        foreach (LocalizedText text in texts) {
            text.SetLanguage(language);
        }
    }
}
