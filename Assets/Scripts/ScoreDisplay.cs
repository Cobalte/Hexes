using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreDisplay : MonoBehaviour {

    public GameController GameController;
    public TextMeshProUGUI ScoreCounter;
    public float MinAnimationTime;

    private float displayedScore;
    private float newAnimTime;
    private float lastAnimTime;
    
    private void Update() {
        if (Math.Abs(displayedScore - GameController.Score) <= float.Epsilon) {
            return;
        }

        lastAnimTime = newAnimTime;
        newAnimTime = Mathf.Max(Mathf.Abs(displayedScore - GameController.Score), MinAnimationTime, lastAnimTime);
        displayedScore += (newAnimTime * Time.deltaTime) * 1;

        if (displayedScore > GameController.Score) {
            displayedScore = GameController.Score;
        }
            
        ScoreCounter.text = ((int)displayedScore).ToString("D6");
    }
}
