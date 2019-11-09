using UnityEngine;
using UnityEngine.UI;

public class CheatMenu : MonoBehaviour {

    public GameController gameController;
    public Text turnCounter;
    public Text scoreCounter;
    
    private int dropsPerTurn = 1;

    //--------------------------------------------------------------------------------------------------------
    private void Update() {
        turnCounter.text = gameController.TurnCount.ToString();
        scoreCounter.text = gameController.Score.ToString();
    }
}
