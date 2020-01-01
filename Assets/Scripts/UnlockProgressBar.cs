using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnlockProgressBar : MonoBehaviour {

    public Image BackgroundImage;
    public Image FillerImage;
    public Image IconImage;
    public GameController GameControllerObj;
    public List<int> UnlockScores;
    public List<Sprite> UnlockIcons;
    
    private RectTransform fillerRect;
    private float fillerMaxSize;
    private float scoreFloor;
    private float scoreCeiling;
    private float progress;
    private int currentUnlock;
    private bool isProgressComplete;

    //--------------------------------------------------------------------------------------------------------
    private void Awake() {
        fillerRect = FillerImage.GetComponent<RectTransform>();
        float backgroundImageWidth = BackgroundImage.GetComponent<RectTransform>().sizeDelta.x;
        float fillerImageOffset = fillerRect.position.x;
        fillerMaxSize = backgroundImageWidth - (2 * fillerImageOffset);

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
            fillerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, progress * fillerMaxSize);
        }
        else {
            IncrementUnlock();
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void IncrementUnlock() {
        if (currentUnlock < UnlockScores.Count - 1) {
            // onto the next unlock!
            currentUnlock++;
            scoreFloor = scoreCeiling;
            scoreCeiling = UnlockScores[currentUnlock];
            IconImage.sprite = UnlockIcons[currentUnlock];
            Debug.Log("Score reward " + currentUnlock + " unlocked!");
        }
        else {
            // we've unlocked everything!
            fillerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fillerMaxSize);
            isProgressComplete = true;
            IconImage.sprite = null;
            Debug.Log("All score rewards unlocked!");
        }
    }
}
