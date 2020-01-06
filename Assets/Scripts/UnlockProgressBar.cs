using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnlockProgressBar : MonoBehaviour
{
    public Image IconImage;
    public GameController GameControllerObj;
    public List<int> UnlockScores;
    public List<Sprite> UnlockIcons;
    public Slider mProgressSlider;
    public Slider mReflectionSlider;

    private RectTransform fillerRect;
    private float fillerMaxSize;
    private float scoreFloor;
    private float scoreCeiling;
    private float progress;
    private int currentUnlock;
    private bool isProgressComplete;

    //--------------------------------------------------------------------------------------------------------
    private void Awake()
    {
        //Set the value to 0 when we start.
        mProgressSlider.value = 0;
        currentUnlock = -1;
        IncrementUnlock();
    }

    //--------------------------------------------------------------------------------------------------------
    private void Update()
    {
        if (isProgressComplete)
        {
            return;
        }

        progress = (GameControllerObj.Score - scoreFloor) / (scoreCeiling - scoreFloor);

        if (progress < 1f)
        {
            //Take our new score and subtract our current score from it.
            float scoreDifference = progress - mProgressSlider.value;
            //Divide that number by how many frames we want to it to take to count. 30 is a rough guess, feels ok.
            float progressIncrementAmt = scoreDifference / 30;
            //Add that number to the display score each frame. *2 to make it feel faster. Can increase this.
            mProgressSlider.value += progressIncrementAmt * 2;
            
            //Extra line here to sync the reflection and the main sliders together.
            mReflectionSlider.value = mProgressSlider.value;
        }
        else
        {
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