using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UI;

public class ScoreMultiplierPanel : MonoBehaviour {

    public List<Sprite> Sprites;

    private int currentLevel;
    private Image displayImage;

    //--------------------------------------------------------------------------------------------------------
    private void Start() {
        displayImage = GetComponent<Image>();
        currentLevel = 0;
    }

    //--------------------------------------------------------------------------------------------------------
    public void TryToIncrementLevel() {
        if (currentLevel < Sprites.Count - 1) {
            currentLevel++;
            displayImage.sprite = Sprites[currentLevel];
        }
    }

    //--------------------------------------------------------------------------------------------------------
    public void ResetLevel() {
        currentLevel = 0;
        displayImage.sprite = Sprites[currentLevel];
    }
    
    //--------------------------------------------------------------------------------------------------------
    public int GetCurrentMultiplier() {
        // returns 2, 4, 8, etc until all sprite are used
        return (int)Math.Pow(2, currentLevel);
    }
}
