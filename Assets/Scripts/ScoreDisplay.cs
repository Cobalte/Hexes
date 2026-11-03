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
    private int shownScore = -1;

    //--------------------------------------------------------------------------------------------------------
    private void Start() {
        //Set the score to 0 when we start
        UpdateCounterText();
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

        //Snap once we're close. Otherwise float rounding can leave us stuck a hair below the score forever,
        //rebuilding the text every frame.
        if (displayedScore > GameController.Score || Mathf.Abs(GameController.Score - displayedScore) < 0.5f) {
            displayedScore = GameController.Score;
        }

        UpdateCounterText();
    }

    //--------------------------------------------------------------------------------------------------------
    public void Snap() {
        displayedScore = GameController.Score;
        UpdateCounterText();
    }

    //--------------------------------------------------------------------------------------------------------
    private void UpdateCounterText() {
        //Only rebuild the string when the number on screen actually changes.
        int score = (int) displayedScore;
        if (score == shownScore) {
            return;
        }

        shownScore = score;
        ScoreCounter.text = "<mspace=110>" + score.ToString("D6") + "</mspace>";
    }
}