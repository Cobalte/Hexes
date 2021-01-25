using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class UnlockProgressBar : MonoBehaviour {
    
    public GameController GameControllerObj;
    public Slider ProgressSlider;
    public List<LevelReward> LevelRewards;
    public TextMeshProUGUI LevelLabel;
    public int CurrentUnlock;

    private const float sliderMoveSpeed = 30f;
    
    private float scoreFloor;
    private float scoreCeiling;
    private float progress;
    private bool finalLevelReached;

    //--------------------------------------------------------------------------------------------------------
    private void Awake() {
        Reset();
    }

    //--------------------------------------------------------------------------------------------------------
    public void Reset() {
        ProgressSlider.value = 0;
        finalLevelReached = false;
        CurrentUnlock = 0;
        scoreFloor = 0;
        scoreCeiling = LevelRewards[0].Experience;
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void Update() {
        if (finalLevelReached) {
            return;
        }

        progress = (GameControllerObj.Score - scoreFloor) / (scoreCeiling - scoreFloor);

        if (progress < 1f) {
            float scoreDifference = progress - ProgressSlider.value;
            float progressIncrementAmt = scoreDifference / sliderMoveSpeed;
            ProgressSlider.value += progressIncrementAmt * 2;
        }
        else {
            IncrementUnlock();
        }
    }

    //--------------------------------------------------------------------------------------------------------
    private void IncrementUnlock() {
        if (CurrentUnlock < LevelRewards.Count - 1) {
            CurrentUnlock++;
            scoreFloor = scoreCeiling;
            scoreCeiling = LevelRewards[CurrentUnlock].Experience;
            LevelLabel.text = (CurrentUnlock + 1).ToString();

            // if this level actually unlocked something, tell that to the game controller 
            switch (LevelRewards[CurrentUnlock].Type) {
                case LevelRewardType.WildDropChance:
                    GameControllerObj.CurrentWildChance = LevelRewards[CurrentUnlock].Value;
                    break;
                case LevelRewardType.DoubleDropChance:
                    GameControllerObj.CurrentDoubleChance = LevelRewards[CurrentUnlock].Value;
                    break;
                case LevelRewardType.TripleDropChance:
                    GameControllerObj.CurrentTripleChance = LevelRewards[CurrentUnlock].Value;
                    break;
                case LevelRewardType.HungryNekoInterval:
                    GameControllerObj.CurrentHungryNekoInterval = (int)LevelRewards[CurrentUnlock].Value;
                    break;
            }
            
            // if this is the first time a new mechanic is introduced, show its tutorial
            LevelRewardType unlockedType = LevelRewards[CurrentUnlock].Type;
            if (CurrentUnlock == LevelRewards.FindIndex(reward => reward.Type == unlockedType)) {
                switch (unlockedType) {
                    case LevelRewardType.WildDropChance:
                        GameControllerObj.ForceWildCardNextTurn = true;
                        break;
                    case LevelRewardType.DoubleDropChance:
                        break;
                    case LevelRewardType.TripleDropChance:
                        break;
                    case LevelRewardType.HungryNekoInterval:
                        break;
                }
            }
        }
        else {
            // we've unlocked everything!
            finalLevelReached = true;
            LevelLabel.text = (CurrentUnlock + 1).ToString();
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SnapToCurrentScoreWithoutRewards() {
        CurrentUnlock = LevelRewards.FindIndex(lvl => GameControllerObj.Score < lvl.Experience);
        scoreFloor = CurrentUnlock > 0 ? LevelRewards[CurrentUnlock - 1].Experience : 0;  
        scoreCeiling = LevelRewards[CurrentUnlock].Experience;
    }
}