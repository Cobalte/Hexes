using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnlockProgressBar : MonoBehaviour {

    public Image BackgroundImage;
    public Image FillerImage;
    public GameController GameControllerObj;

    private RectTransform fillerRect;
    private float fillerMaxSize;
    private float scoreStart;
    private float scoreGoal;
    private float progress;

    //--------------------------------------------------------------------------------------------------------
    private void Awake() {
        float backgroundImageWidth = BackgroundImage.GetComponent<RectTransform>().sizeDelta.x;
        fillerRect = FillerImage.GetComponent<RectTransform>();
        float fillerImageOffset = fillerRect.position.x;
        fillerMaxSize = backgroundImageWidth - (2 * fillerImageOffset);

        scoreStart = 0;
        scoreGoal = 1000;
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void Update() {
        progress = (GameControllerObj.Score - scoreStart) / (scoreGoal - scoreStart);

        if (progress >= 1f) {
            scoreStart = scoreGoal;
            //GameControllerObj.UnlockProgress(out scoreGoal);
            GameControllerObj.UnlockProgress();
            scoreGoal = scoreStart + 1000;
            progress = (GameControllerObj.Score - scoreStart) / (scoreGoal - scoreStart);
        }
        
        fillerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, progress * fillerMaxSize);    
    }
}
