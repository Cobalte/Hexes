using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnlockProgressBar : MonoBehaviour {
    
    //public Image IconImage;
    public GameController GameControllerObj;
    public Slider ProgressSlider;
    public Slider ReflectionSlider;
    public List<LevelReward> LevelRewards;
    public TextMeshProUGUI levelLabel;
    public int currentUnlock;

    private const float sliderGrowSpeed = 30f;
    
    private RectTransform fillerRect;
    private float fillerMaxSize;
    private float scoreFloor;
    private float scoreCeiling;
    private float progress;
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
        if (currentUnlock < LevelRewards.Count - 1) {
            if (currentUnlock >= 0) {
                switch (LevelRewards[currentUnlock].Type) {
                    case LevelRewardType.WildDropChance:
                        GameControllerObj.CurrentWildChance = LevelRewards[currentUnlock].Chance;
                        break;
                    case LevelRewardType.DoubleDropChance:
                        GameControllerObj.CurrentDoubleChance = LevelRewards[currentUnlock].Chance;
                        break;
                    case LevelRewardType.TripleDropChance:
                        GameControllerObj.CurrentTripleChance = LevelRewards[currentUnlock].Chance;
                        break;
                }
            }
            
            // onto the next unlock!
            currentUnlock++;
            scoreFloor = scoreCeiling;
            scoreCeiling = LevelRewards[currentUnlock].Experience;
            levelLabel.text = (currentUnlock + 1).ToString();
        }
        else {
            // we've unlocked everything!
            isProgressComplete = true;
            levelLabel.text = (currentUnlock + 1).ToString();
        }
    }
}