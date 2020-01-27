using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanelController : MonoBehaviour
{
    public int ScoreResult;
    public bool IsHighScore;
    public GameObject ResultMenuObj;
    public Animator ResultsMenuAnimator;
    public GameObject SushiBoatResults;
    public GameObject GridAnchor;
    public TextMeshProUGUI ScoreResultsLabel;
    public GameObject HighScoreIndicator;

    private GameObject oldSushiBoat;
    
    // rider says to do this for performance?
    private static readonly int statusHash = Animator.StringToHash("Status");

    //--------------------------------------------------------------------------------------------------------
    private void Start() {
        ResultsMenuAnimator.SetBool(statusHash, false);
        ResultMenuObj.SetActive(false);
    }

    //--------------------------------------------------------------------------------------------------------
    public void ToggleResultsMenu() {
        ResultMenuObj.SetActive(true);
        ResultsMenuAnimator.SetBool(statusHash, !ResultsMenuAnimator.GetBool(statusHash));
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void GameIsOver() {
        //If the player has seen the results page before, we need to clean up the instantiated results object.
        Destroy(oldSushiBoat);
        oldSushiBoat = null;
        
        GameObject sushiBoat = Instantiate(SushiBoatResults, GridAnchor.transform);
        sushiBoat.transform.localPosition = Vector3.zero;
        
        foreach (Transform child in sushiBoat.GetComponentsInChildren<Transform>()) {
            if (child == sushiBoat.transform) {
                continue;
            }
            
            //Need to set the speed of the animators to not-0.
            if (child.GetComponent<Animator>()) {
                Animator animator = child.GetComponent<Animator>();
                float speed = UnityEngine.Random.Range(0.9f, 1.4f);
                animator.SetFloat("speedMultiplier", speed);
            }
            
            //Need to turn of sorting override so the blocks appear in the pop up window sort.
            if (child.GetComponent<Canvas>()) {
                Canvas canvas = child.GetComponent<Canvas>();
                canvas.overrideSorting = false;
            }
        }

        HighScoreIndicator.SetActive(IsHighScore);
        ScoreResultsLabel.text = "<mspace=100>" + ((int) ScoreResult).ToString("D6") + "</mspace>";

        oldSushiBoat = sushiBoat;
    }
}
