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
    private GameObject background;

    //--------------------------------------------------------------------------------------------------------
    private void Awake() {
        // the red disc behind the combo badges - only the x1 badge lines up with it exactly
        Transform backgroundTransform = transform.Find("BG");
        if (backgroundTransform != null) {
            background = backgroundTransform.gameObject;
        }
    }

    //--------------------------------------------------------------------------------------------------------
    public void TryToIncrementLevel() {
        if (CurrentLevel < ComboPrefabs.Count - 1) {
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

        // higher badges don't fully cover the disc, so hide it whenever we're above x1
        if (background != null) {
            background.SetActive(CurrentLevel == 0);
        }
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