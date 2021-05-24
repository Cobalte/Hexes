using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HungryNeko : MonoBehaviour {

    public GameController GameController;
    public Animator NekoAnimator;
    public List<Image> BlockCounters;
    public int BlockLevel;
    public int BlocksLeft;
    public int BlocksIncoming;
    public List<Hex> FeedingHexes;
    public List<BoardDirection> FeedingDirections;
    public List<GameObject> HelperArrows;

    public bool IsHungry => BlocksLeft - BlocksIncoming > 0;

    private static int pointsForFeeding = 1000;

    //--------------------------------------------------------------------------------------------------------
    public void Feed(Block sushiBlock) {
        Destroy(sushiBlock.gameObject);
        BlocksLeft--;
        BlocksIncoming--;
        BlockCounters[BlocksLeft].gameObject.SetActive(false);
        
        if (!IsHungry) {
            GetFull();
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void GetHungry(int level, int count) {
        NekoAnimator.Play("NekoHungry");
        BlockLevel = level;
        BlocksLeft = count;

        for (int i = 0; i < BlockCounters.Count; i++) {
            BlockCounters[i].sprite = GameController.ImageForBlockProgression[level - 1];
            BlockCounters[i].gameObject.SetActive(i < count);
        }

        foreach (GameObject helperArrow in HelperArrows) {
            helperArrow.SetActive(true);
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void GetFull() {
        NekoAnimator.Play("NekoFull");
        GameController.ChangeScore(pointsForFeeding);
        
        foreach (GameObject helperArrow in HelperArrows) {
            helperArrow.SetActive(false);
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void Reset() {
        NekoAnimator.Play("NekoNotHungry");
        BlockLevel = 0;
        BlocksLeft = 0;
        BlocksIncoming = 0;
        
        foreach (Image counterImage in BlockCounters) {
            counterImage.gameObject.SetActive(false);
        }
        
        foreach (GameObject helperArrow in HelperArrows) {
            helperArrow.SetActive(false);
        }
    }
}
