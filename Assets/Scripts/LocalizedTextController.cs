using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LocalizedTextController {

    private static List<LocalizedText> texts;
    private static List<LocalizedFont> fonts;
    
    //--------------------------------------------------------------------------------------------------------
    public static void SetGlobalLanguage(LocalizedLanguage language) {
        texts ??= Resources.FindObjectsOfTypeAll<LocalizedText>().ToList();
        fonts ??= Resources.FindObjectsOfTypeAll<LocalizedFont>().ToList();
        
        foreach (LocalizedText text in texts) {
            text.SetLanguage(language);
        }
        
        foreach (LocalizedFont text in fonts) {
            text.SetLanguage(language);
        }
    }
}
