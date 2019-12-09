using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour {

    private const float slideSpeed = 5f;
    private const float arrivalDist = 0.1f;

    public int Level;
    public BlockKind Kind;
    public bool SuicideAfterEating;
    public bool SuicideOnArrival;
    public bool IsMoving => swipeDestPos != null;
    public List<Block> BlocksToEat;
    public Image DisplayImage;

    private Vector3? swipeDestPos;
    private Hex swipeDestHex;
    private bool isInitialized;
    private GameController gameController;
    private GameObject uiCanvas;

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
    public void SlideTo(Hex destHex) {
        swipeDestHex = destHex;
        swipeDestPos = destHex.transform.position;
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SlideTo(Vector3 destUiPos) {
        swipeDestHex = null;
        swipeDestPos = destUiPos;
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
        
        if (SuicideAfterEating || SuicideOnArrival) {
            Destroy(gameObject);
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void Eat(Block food) {
        Debug.Log(name + " ate " + food.name);
        BlocksToEat.Remove(food);
        Destroy(food.gameObject);

        GameObject celebration = Instantiate(
            original: gameController.CombineCelebrationObj,
            parent: uiCanvas.transform,
            worldPositionStays: false);
        celebration.transform.position = transform.position + Vector3.back;

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
