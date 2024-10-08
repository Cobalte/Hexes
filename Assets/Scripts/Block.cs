﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour {

    private const float slideSpeed = 3f;
    private const float tutorialSlideSpeed = 2f;
    private const float arrivalDist = 0.1f;
    private const float pitchPerLevel = 0.05f;

    public int Level;
    public BlockKind Kind;
    public bool SuicideAfterEating;
    public bool SuicideOnArrival;
    public List<Block> BlocksToEat;
    public Image DisplayImage;
    public Animator BlockAnimator;
    public List<GameObject> CombineCelebrationPrefabs;
    public AudioClip combineAudioClip;
    
    public bool IsMoving => swipeDestPos != null;

    private Vector3? swipeDestPos;
    private Hex swipeDestHex;
    private HungryNeko swipeDestNeko;
    private bool isInitialized;
    private GameController gameController;
    private GameObject uiCanvas;
    private BoardDirection swipeDirection;
    private AudioSource audioSource;
    private AudioManager audioManager;
    private float audioDefaultPitch;

    //--------------------------------------------------------------------------------------------------------
    public void Initialize(Hex dropHex, int startLevel, BlockKind blockKind, AudioManager audioMgr) {
        Level = startLevel;
        swipeDestHex = null;
        swipeDestPos = null;
        swipeDestNeko = null;
        Kind = blockKind;
        audioManager = audioMgr;
        audioSource = GetComponent<AudioSource>();
        audioDefaultPitch = audioSource.pitch;
        BlocksToEat = new List<Block>();
        gameController = GameObject.Find("Game Controller").GetComponent<GameController>();
        uiCanvas = GameObject.Find("UI Canvas");
        name = "Block on " + dropHex.name + ", Level " + Level + " (" + Kind + ")";
        UpdateDisplayImage();
        RandomizeSpeed();
        isInitialized = true;
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void Update() {
        if (!isInitialized) {
            return;
        }

        Block food = BlocksToEat.FirstOrDefault( 
            b => Vector3.Distance(transform.position, b.transform.position) < arrivalDist);

        if (food != null) {
            Eat(food);
        }
        
        if (swipeDestPos != null) {
            if (Vector3.Distance(transform.position, (Vector3)swipeDestPos) < arrivalDist) {
                Arrive();
            }
            else {
                // we are sliding
                float speed = (gameController.IsFreshGame ? tutorialSlideSpeed : slideSpeed) * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, (Vector3)swipeDestPos, speed);
            }
        }
    }

    //--------------------------------------------------------------------------------------------------------
    public void SlideTo(Hex destHex, BoardDirection direction) {
        swipeDestHex = destHex;
        swipeDestNeko = null;
        swipeDestPos = destHex.transform.position;
        swipeDirection = direction;
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SlideTo(HungryNeko neko, BoardDirection direction) {
        swipeDestHex = null;
        swipeDestNeko = neko;
        swipeDestPos = neko.transform.position;
        swipeDirection = direction;
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void Arrive() {
        if (swipeDestHex != null) {
            name = "Block on " + swipeDestHex.name + ", Level " + Level + " (" + Kind + ")";
            swipeDestHex = null;    
        }
        
        if (swipeDestNeko != null) {
            swipeDestNeko.Feed(this);
            return;
        }
        
        transform.position = (Vector3)swipeDestPos;
        swipeDestPos = null;
        BlocksToEat.Clear();
        UpdateDisplayImage();
        
        // do we die now?
        if (SuicideAfterEating || SuicideOnArrival) {
            Destroy(gameObject);
            return;
        }

        // play arrival anim
        switch (swipeDirection) {
            case BoardDirection.UpLeft:
                BlockAnimator.Play("Block_NW");
                break;
            case BoardDirection.Up:
                BlockAnimator.Play("Block_N");
                break;
            case BoardDirection.UpRight:
                BlockAnimator.Play("Block_NE");
                break;
            case BoardDirection.DownRight:
                BlockAnimator.Play("Block_SE");
                break;
            case BoardDirection.Down:
                BlockAnimator.Play("Block_S");
                break;
            case BoardDirection.DownLeft:
                BlockAnimator.Play("Block_SW");
                break;
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void Eat(Block food) {
        Destroy(food.gameObject);
        //DestroyImmediate(food.gameObject);

        //Instantiate the correct combo pfx based on the current combo multiplier.
        Celebration celebration = Instantiate(
            original: CombineCelebrationPrefabs[gameController.ScoreMultPanel.CurrentLevel],
            parent: uiCanvas.transform,
            worldPositionStays: false).GetComponent<Celebration>();
        celebration.transform.position = (Vector3)swipeDestPos;

        gameController.ChangeScore(Level * gameController.ScoreMultPanel.GetCurrentMultiplier());
        gameController.ScoreMultPanel.TryToIncrementLevel();
        gameController.SomethingJustPromoted = true;
        RandomizeSpeed();
        BlockAnimator.Play("Block_Birth");

        BlocksToEat.Remove(food);
        gameController.GlobalFood.Remove(food);
        
        // play a celebratory sound
        audioSource.pitch = audioDefaultPitch + (pitchPerLevel * Level);
        audioSource.Play();
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void UpdateDisplayImage() {
        DisplayImage.sprite = Kind == BlockKind.WildCard
            ? gameController.ImageForWildCard
            : gameController.ImageForBlockProgression[Level - 1];
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void RandomizeSpeed() {
        // Fun bit of code that sets a multiplier to be applied to the animator's playback speed.
        // Adds some visual variety.
        float speed = UnityEngine.Random.Range(0.9f, 1.4f);
        BlockAnimator.SetFloat("speedMultiplier", speed);
    }
}
