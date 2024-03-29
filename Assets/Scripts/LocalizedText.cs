using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour {

    public List<LocalizedTextLine> Lines;

    private TextMeshProUGUI textMesh;
    
    //--------------------------------------------------------------------------------------------------------
    public void Awake() {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    //--------------------------------------------------------------------------------------------------------
    public void SetLanguage(LocalizedLanguage language) {
        textMesh ??= GetComponent<TextMeshProUGUI>();
        
        foreach (LocalizedTextLine line in Lines) {
            if (line.Language == language) {
                bool isActive = gameObject.activeSelf;

                if (isActive) {
                    textMesh.gameObject.SetActive(false);
                }
                textMesh.text = line.Text;
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
        Lines.Add(new LocalizedTextLine { Language = LocalizedLanguage.English, Text = "New String" });
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void RemoveLineAt(int index) {
        Lines.RemoveAt(index);
    }
}

//------------------------------------------------------------------------------------------------------------
[Serializable]
public class LocalizedTextLine {
    public LocalizedLanguage Language;
    public string Text;
    public TMP_FontAsset FontAsset;
    public Material FontMaterial;
}
