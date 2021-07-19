using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalizedFont : MonoBehaviour {
    
    public List<LocalizedFontLine> Lines;

    private TextMeshProUGUI textMesh;
    
    //--------------------------------------------------------------------------------------------------------
    public void Awake() {
        //LocalizedTextController.RegisterText(this);
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    //--------------------------------------------------------------------------------------------------------
    public void SetLanguage(LocalizedLanguage language) {
        textMesh ??= GetComponent<TextMeshProUGUI>();
        
        foreach (LocalizedFontLine line in Lines) {
            if (line.Language == language) {
                bool isActive = gameObject.activeSelf;

                if (isActive) {
                    textMesh.gameObject.SetActive(false);
                }
                textMesh.font = line.FontAsset;
                textMesh.fontMaterial = line.FontMaterial;
                if (isActive) {
                    textMesh.gameObject.SetActive(true);
                }
                return;
            }
        }
        
        Debug.LogWarning($"Localized text for {language} not set on object '{gameObject.name}'.");
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void AddLanguageLine() {
        Lines.Add(new LocalizedFontLine { Language = LocalizedLanguage.English  });
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void RemoveLineAt(int index) {
        Lines.RemoveAt(index);
    }
}

//------------------------------------------------------------------------------------------------------------
[Serializable]
public class LocalizedFontLine {
    public LocalizedLanguage Language;
    public TMP_FontAsset FontAsset;
    public Material FontMaterial;
}
