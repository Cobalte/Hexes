using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnlockProgressBar : MonoBehaviour {
    
    public Image IconImage;
    public GameController GameControllerObj;
    public List<int> UnlockScores;
    public List<Sprite> UnlockIcons;
    public Slider ProgressSlider;
    public Slider ReflectionSlider;

    private const float sliderGrowSpeed = 30f;
    
    private RectTransform fillerRect;
    private float fillerMaxSize;
    private float scoreFloor;
    private float scoreCeiling;
    private float progress;
    private int currentUnlock;
    private bool isProgressComplete;

    //--------------------------------------------------------------------------------------------------------
    private void Awake() {
        ProgressSlider.value = 0;
        currentUnlock = -1;
        IncrementUnlock();
    }

    //--------------------------------------------------------------------------------------------------------
    private void Update() {
        if (isProgressComplete) {
            return;
        }

        progress = (GameControllerObj.Score - scoreFloor) / (scoreCeiling - scoreFloor);

        if (progress < 1f) {
            float scoreDifference = progress - ProgressSlider.value;
            float progressIncrementAmt = scoreDifference / sliderGrowSpeed;
            ProgressSlider.value += progressIncrementAmt * 2;
            ReflectionSlider.value = ProgressSlider.value;
        }
        else {
            IncrementUnlock();
        }
    }

    //--------------------------------------------------------------------------------------------------------
    private void IncrementUnlock()
    {
        if (currentUnlock < UnlockScores.Count - 1)
        {
            // onto the next unlock!
            currentUnlock++;
            scoreFloor = scoreCeiling;
            scoreCeiling = UnlockScores[currentUnlock];
            IconImage.sprite = UnlockIcons[currentUnlock];
            Debug.Log("Score reward " + currentUnlock + " unlocked!");
        }
        else
        {
            // we've unlocked everything!
            isProgressComplete = true;
            IconImage.sprite = null;
            Debug.Log("All score rewards unlocked!");
        }
    }
}