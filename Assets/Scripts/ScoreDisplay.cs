using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    public GameController GameController;
    public TextMeshProUGUI ScoreCounter;
    public float MinAnimationTime;

    private float displayedScore;
    private float newAnimTime;
    private float lastAnimTime;

    //--------------------------------------------------------------------------------------------------------
    private void Start() {
        //Set the score to 0 when we start
        ScoreCounter.text = ((int) displayedScore).ToString("D6");
    }

    //--------------------------------------------------------------------------------------------------------
    private void Update() {
        if (Math.Abs(displayedScore - GameController.Score) <= float.Epsilon) {
            return;
        }

        lastAnimTime = newAnimTime;
        newAnimTime = Mathf.Max(Mathf.Abs(displayedScore - GameController.Score), MinAnimationTime, lastAnimTime);

        //Take our new score and subtract our current score from it.
        float scoreDifference = GameController.Score - displayedScore;
        //Divide that number by how many frames we want to it to take to count. 30 is a rough guess, feels ok.
        float scoreIncrementAmt = scoreDifference / 30;
        //Add that number to the display score each frame.
        displayedScore += scoreIncrementAmt;

        if (displayedScore > GameController.Score) {
            displayedScore = GameController.Score;
        }

        ScoreCounter.text = ((int) displayedScore).ToString("D6");
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void Snap() {
        displayedScore = GameController.Score;
        ScoreCounter.text = ((int) displayedScore).ToString("D6");
    }
}