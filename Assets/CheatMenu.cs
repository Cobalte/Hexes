using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CheatMenu : MonoBehaviour {

    public GameController gameController;
    public TextMeshProUGUI turnCounter;
    public TextMeshProUGUI scoreCounter;
    public float animationTime;

    private float currentScore;
    private float desiredScore;
    private int displayScore;
    private int dropsPerTurn = 1;

    //--------------------------------------------------------------------------------------------------------
    private void Update() {
        turnCounter.text = gameController.TurnCount.ToString();
        desiredScore = gameController.Score;
        
        if (currentScore != desiredScore)
        {
            if (currentScore < desiredScore)
            {
                currentScore += (animationTime * Time.deltaTime) * 1;
                if (currentScore > desiredScore)
                {
                    currentScore = desiredScore;
                }
            }
        }

        displayScore = (int) currentScore;
        scoreCounter.text = displayScore.ToString("D6");
    }
    
    
}
