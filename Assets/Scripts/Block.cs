using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour {

    private const float slideSpeed = 3f;
    private const float arrivalDist = 0.1f;

    public int Level;
    public BlockKind Kind;
    public bool SuicideAfterEating;
    public bool SuicideOnArrival;
    public bool IsMoving => swipeDestPos != null;
    public List<Block> BlocksToEat;
    public Image DisplayImage;
    public Animator BlockAnimator;

    private Vector3? swipeDestPos;
    private Hex swipeDestHex;
    private bool isInitialized;
    private GameController gameController;
    private GameObject uiCanvas;
    private BoardDirection swipeDirection;

    //--------------------------------------------------------------------------------------------------------
    public void Initialize(Hex dropHex, int startLevel, BlockKind blockKind) {
        Level = startLevel;
        swipeDestHex = null;
        swipeDestPos = null;
        Kind = blockKind;
        BlocksToEat = new List<Block>();
        gameController = GameObject.Find("Game Controller").GetComponent<GameController>();
        uiCanvas = GameObject.Find("UI Canvas");
        name = "Block on " + dropHex.name + ", Level " + Level + " (" + Kind + ")";
        UpdateDisplayImage();
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
            return;
        }
        
        if (swipeDestPos != null) {
            if (Vector3.Distance(transform.position, (Vector3)swipeDestPos) < arrivalDist) {
                Arrive();
            }
            else {
                // we are sliding
                float speed = slideSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, (Vector3)swipeDestPos, speed);
            }
        }
    }

    //--------------------------------------------------------------------------------------------------------
    public void SlideTo(Hex destHex, BoardDirection direction) {
        swipeDestHex = destHex;
        swipeDestPos = destHex.transform.position;
        swipeDirection = direction;
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SlideTo(Vector3 destUiPos, BoardDirection direction) {
        swipeDestHex = null;
        swipeDestPos = destUiPos;
        swipeDirection = direction;
    }
   
    //--------------------------------------------------------------------------------------------------------
    private void Arrive() {
        if (swipeDestHex != null) {
            name = "Block on " + swipeDestHex.name + ", Level " + Level + " (" + Kind + ")";
            swipeDestHex = null;    
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
                BlockAnimator.Play("Block_SW");
                break;
            case BoardDirection.Down:
                BlockAnimator.Play("Block_S");
                break;
            case BoardDirection.DownLeft:
                BlockAnimator.Play("Block_SE");
                break;
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void Eat(Block food) {
        Debug.Log(name + " ate " + food.name);
        BlocksToEat.Remove(food);
        Destroy(food.gameObject);

        Celebration celebration = Instantiate(
            original: (Kind == BlockKind.Anvil || Kind == BlockKind.Plant)
                ? gameController.DestroyCelebrationPrefab
                : gameController.CombineCelebrationPrefab ,
            parent: uiCanvas.transform,
            worldPositionStays: false).GetComponent<Celebration>();
        celebration.transform.position = transform.position + Vector3.back;
        celebration.SetSprite(gameController.ImageForBlockProgression[Level - 1]);

        if (Kind == BlockKind.Anvil || Kind == BlockKind.Plant) {
            gameController.Score -= food.Level;
        }
        else {
            gameController.Score += Level * gameController.ScoreMultPanel.GetCurrentMultiplier();
            gameController.ScoreMultPanel.TryToIncrementLevel();
            gameController.SomethingJustPromoted = true;
            BlockAnimator.Play("Block_Birth");
        }
        
        if (Kind == BlockKind.Plant) {
            Destroy(gameObject);
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void UpdateDisplayImage() {
        switch (Kind) {
            case BlockKind.WildCard:
                DisplayImage.sprite = gameController.ImageForWildCard;
                break;
            case BlockKind.Anvil:
                DisplayImage.sprite = gameController.ImageForAnvil;
                break;
            case BlockKind.Plant:
                DisplayImage.sprite = gameController.ImageForPlant;
                break;
            default:
                DisplayImage.sprite = gameController.ImageForBlockProgression[Level - 1];
                break;
        }
    }
}
