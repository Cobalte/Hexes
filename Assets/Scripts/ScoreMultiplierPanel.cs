using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;

public class ScoreMultiplierPanel : MonoBehaviour {
    
    public List<GameObject> ComboPrefabs;
    public GameObject ActiveComboObj;

    private int currentLevel;
    private Image displayImage;

    //--------------------------------------------------------------------------------------------------------
    private void Start() {
        currentLevel = 0;
        ActiveComboObj = null;
        CreateComboPrefab();
    }

    //--------------------------------------------------------------------------------------------------------
    public void TryToIncrementLevel() {
        if (currentLevel < ComboPrefabs.Count - 1) {
            currentLevel++;
            CreateComboPrefab();
        }
    }

    //--------------------------------------------------------------------------------------------------------
    private void CreateComboPrefab() {
        if (ActiveComboObj) {
            Destroy(ActiveComboObj);
        }
        
        ActiveComboObj = null;
        ActiveComboObj = Instantiate(
            original: ComboPrefabs[currentLevel],
            parent: transform,
            worldPositionStays: true);
        ActiveComboObj.transform.localPosition = Vector3.zero;
        ActiveComboObj.transform.localScale = Vector3.one;
        Debug.Log(ComboPrefabs[currentLevel] + " created.");
    }

    //--------------------------------------------------------------------------------------------------------
    public void ResetLevel() {
        currentLevel = 0;
        CreateComboPrefab();
    }

    //--------------------------------------------------------------------------------------------------------
    public int GetCurrentMultiplier() {
        // returns 2, 4, 8, etc until all sprite are used
        return (int) Math.Pow(2, currentLevel);
    }
}