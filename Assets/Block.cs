using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour {

    private const float slideSpeed = 800f;

    public int Level;
    public BlockKind Kind;
    public bool SuicideAfterEating;
    public bool SuicideOnArrival;
    public bool IsMoving => swipeDestPos != null;
    public List<Block> BlocksToEat;
    
    private Vector3? swipeDestPos;
    private Hex swipeDestHex;
    private bool isInitialized;
    private Text displayText;

    //--------------------------------------------------------------------------------------------------------
    public void Initialize(Hex dropHex, int startLevel, BlockKind blockKind) {
        Level = startLevel;
        swipeDestHex = null;
        swipeDestPos = null;
        displayText = GetComponent<Text>();
        Kind = blockKind;
        BlocksToEat = new List<Block>();
        name = "Block on " + dropHex.name + ", Level " + Level + " (" + Kind + ")";

        switch (blockKind) {
            case BlockKind.WildCard:
                displayText.text = "wild";
                break;
            case BlockKind.Anvil:
                displayText.text = "anvl";
                break;
            case BlockKind.Plant:
                displayText.text = "plnt";
                break;
            default:
                displayText.text = Level.ToString();
                break;
        }
        
        isInitialized = true;
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void Update() {
        if (!isInitialized) {
            return;
        }

        Block food = BlocksToEat.FirstOrDefault(
            b => Vector3.Distance(transform.position, b.transform.position) < 10f);

        if (food != null) {
            Eat(food);
            return;
        }
        
        if (swipeDestPos != null) {
            if (Vector3.Distance(transform.position, (Vector3)swipeDestPos) < 10f) {
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
    public void SetGlowing(bool glow) {
        displayText.color = glow ? Color.red : Color.black;
    } 
    
    //--------------------------------------------------------------------------------------------------------
    public void SlideTo(Hex destHex) {
        swipeDestHex = destHex;
        swipeDestPos = destHex.UiPosition;
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SlideTo(Vector3 destUiPos) {
        swipeDestHex = null;
        swipeDestPos = destUiPos;
    }
   
    //--------------------------------------------------------------------------------------------------------
    private void Arrive() {
        transform.position = (Vector3)swipeDestPos;

        if (swipeDestHex != null) {
            name = "Block on " + swipeDestHex.name + ", Level " + Level + " (" + Kind + ")";
            swipeDestHex = null;    
        }
        
        swipeDestPos = null;
        BlocksToEat.Clear();
        
        if (SuicideAfterEating || SuicideOnArrival) {
            Destroy(gameObject);
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void Eat(Block food) {
        if (Kind == BlockKind.Normal) {
            displayText.text = Level.ToString();
        }
        Debug.Log("Ate block: " + food.name);
        BlocksToEat.Remove(food);
        Destroy(food.gameObject);
        if (Kind == BlockKind.Plant) {
            Destroy(gameObject);
        }
    }
}
