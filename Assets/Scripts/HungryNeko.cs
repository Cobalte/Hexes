using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
    public List<AudioClip> MeowSounds;

    public bool IsHungry => BlocksLeft - BlocksIncoming > 0;

    private AudioSource audioSource;

    private static int pointsForFeeding = 1000;

    //--------------------------------------------------------------------------------------------------------
    private void Start() {
        audioSource = GetComponent<AudioSource>();
    }

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
    public void GetHungry(int level) {
        NekoAnimator.Play("NekoHungry");
        BlockLevel = level;
        BlocksLeft = 1;

        BlockCounters[0].sprite = GameController.ImageForBlockProgression[level - 1];
        BlockCounters[0].gameObject.SetActive(true);

        foreach (GameObject helperArrow in HelperArrows) {
            helperArrow.SetActive(true);
        }
        
        PlayRandomMeow();
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void GetFull() {
        NekoAnimator.Play("NekoFull");
        GameController.ChangeScore(pointsForFeeding);
        
        foreach (GameObject helperArrow in HelperArrows) {
            helperArrow.SetActive(false);
        }
        
        PlayRandomMeow();
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
    
    //--------------------------------------------------------------------------------------------------------
    public void PlayRandomMeow() {
        if (MeowSounds.Count > 0 && audioSource != null) {
            audioSource.PlayOneShot(MeowSounds[Random.Range(0, MeowSounds.Count)]);
        }
    }
}
