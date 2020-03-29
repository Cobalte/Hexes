using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;

public class ScoreMultiplierPanel : MonoBehaviour
{
    public List<GameObject> ComboPrefabs;
    public GameObject ActiveComboObj;
    public int CurrentLevel;
    
    private Image displayImage;

    //--------------------------------------------------------------------------------------------------------
    public void TryToIncrementLevel() {
        if (CurrentLevel < ComboPrefabs.Count - 1)
        {
            CurrentLevel++;
            CreateComboPrefab();
        }
    }

    //--------------------------------------------------------------------------------------------------------
    public void CreateComboPrefab() {
        if (ActiveComboObj) {
            Destroy(ActiveComboObj);
        }

        ActiveComboObj = null;
        ActiveComboObj = Instantiate(
            original: ComboPrefabs[CurrentLevel],
            parent: transform,
            worldPositionStays: true);
        ActiveComboObj.transform.localPosition = Vector3.zero;
        ActiveComboObj.transform.localScale = Vector3.one;
        RectTransform rect = ActiveComboObj.GetComponent<RectTransform>();
        rect.offsetMax = Vector2.zero;
        rect.offsetMin = Vector2.zero;
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void ResetLevel() {
        CurrentLevel = 0;
        CreateComboPrefab();
    }

    //--------------------------------------------------------------------------------------------------------
    public int GetCurrentMultiplier() {
        // returns 2, 4, 8, etc until all sprite are used
        return (int) Math.Pow(2, CurrentLevel);
    }
}