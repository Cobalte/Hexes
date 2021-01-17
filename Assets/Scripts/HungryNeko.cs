using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HungryNeko : MonoBehaviour {

    public Image NekoImage;
    public int BlockLevel;
    public int BlocksLeft;
    public int BlocksIncoming;
    public List<Hex> FeedingHexes;
    public List<BoardDirection> FeedingDirections;

    public bool IsHungry => BlocksLeft - BlocksIncoming > 0;

    //--------------------------------------------------------------------------------------------------------
    private void Awake() {
        
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void Feed(Block sushiBlock) {
        Destroy(sushiBlock.gameObject);
        BlocksLeft--;
        BlocksIncoming--;
    }
    
    //--------------------------------------------------------------------------------------------------------
    
    //--------------------------------------------------------------------------------------------------------
}
