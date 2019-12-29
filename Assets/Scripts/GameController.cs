using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour {

    public GameObject BoardObj;
    public GameObject UiCanvasObj;
    public GameObject BlockPrefab;
    public int Score;
    public List<Sprite> ImageForBlockProgression;
    public Sprite ImageForWildCard;
    public Sprite ImageForAnvil;
    public Sprite ImageForPlant;
    public GameObject CreateCelebrationPrefab;
    public GameObject CombineCelebrationPrefab;
    public GameObject DestroyCelebrationPrefab;
    public ScoreMultiplierPanel ScoreMultPanel;
    public bool SomethingJustPromoted;
    public GameObject GameOverPanel;

    private List<Hex> hexes;
    private List<Block> blocks;
    private BoardDirection swipeDir;
    private Hex newDropHex;
    private Block newBlock;
    private bool allowInput;
    private static Touch currentTouch;
    private static float swipeMinDist;
    private static Vector3 swipeStartPos;
    private static Vector3 swipeEndPos;
    private int turnCount;

    private bool IsBoardFull => hexes.All(h => h.Occupant != null);
    private bool IsAnyBlockMoving => blocks.Any(b => b.IsMoving);
    private bool IsGameOver => GameOverPanel.activeSelf;

    private const int dropsPerTurn = 1;
    private const int wildTurnReq = 20;
    private const int wildChance = 8;
    private const int anvilTurnReq = 30;
    private const int anvilChance = 7;
    private const int plantTurnReq = 40;
    private const int plantChance = 6;
    private const float minSwipeDistScreenFration = 0.1f;
    private const int doubleDropScoreThreshold = 2000;
    private const int tripleDropScoreThreshold = 6000;
    
    //--------------------------------------------------------------------------------------------------------
    private void Start() {
        swipeMinDist = Screen.height * minSwipeDistScreenFration;
        hexes = BoardObj.transform.GetComponentsInChildren<Hex>().ToList();
        blocks = new List<Block>();
        StartNewGame();
    }

    //--------------------------------------------------------------------------------------------------------
    private void StartNewGame() {
        foreach (Block block in blocks) {
            Destroy(block.gameObject);
        }
        
        blocks = new List<Block>();
        swipeDir = BoardDirection.Null;
        turnCount = 0;
        GameOverPanel.SetActive(false);
        DropBlocks();
        allowInput = true;
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void EnterGameOverState() {
        Debug.Log("Game is over.");
        GameOverPanel.SetActive(true);
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void Update() {
        blocks.RemoveAll(b => b == null);
        
        if (!allowInput && !IsAnyBlockMoving) {
            if (!SomethingJustPromoted) {
                ScoreMultPanel.ResetLevel();
            }
            
            DropBlocks();
            if (Score > doubleDropScoreThreshold) { DropBlocks(); }
            //if (Score > tripleDropScoreThreshold) { DropBlocks(); } 
            allowInput = true;
            SomethingJustPromoted = false;
        }
        
        swipeDir = GetSwipeDirection();
        
        if (swipeDir == BoardDirection.Null) {
            return;
        }

        ExecuteSwipe(out bool somethingMoved);

        if (somethingMoved) {
            Debug.Log("Swiping in direction " + swipeDir);
            allowInput = false;
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void ExecuteSwipe(out bool somethingMoved) {
        IEnumerable<Hex> captains = from h in hexes where !h.NeighborDirections.Contains(swipeDir) select h;
        BoardDirection searchDir = Opposite(swipeDir);
        somethingMoved = false;

        foreach (Hex captain in captains) {
            List<Hex> column = HexesInDirection(captain, searchDir);

            for (int curHex = 0; curHex < column.Count; curHex++) {
                if (column[curHex].CurrentLevel == 0 ||
                    column[curHex].Occupant.Kind == BlockKind.Plant) {
                    // this is an empty hex or a plant - do nothing
                    continue;
                }
                
                if (column[curHex].Occupant.Kind == BlockKind.Anvil) {
                    // this is an anvil - move and destroy everything in your path
                    for (int n = 0; n < curHex; n++) {
                        if (column[n].Occupant != null) {
                            column[curHex].Occupant.BlocksToEat.Add(column[n].Occupant);
                            column[n].Occupant = null;
                        }
                    }

                    column[curHex].Occupant.SlideTo(GetAnvilDestination(column));
                    column[curHex].Occupant.SuicideOnArrival = true;
                    column[curHex].Occupant = null;
                    somethingMoved = true;
                    continue;
                }
                
                int newHex = DeliciousEmptyHex(column, curHex);

                if (newHex > 0 && column[newHex - 1].Occupant.Kind == BlockKind.Plant) {
                    // this is a non-anvil block that's about to fall into a plant
                    column[curHex].Occupant.SlideTo(column[newHex - 1].transform.position);
                    column[newHex - 1].Occupant.BlocksToEat.Add(column[curHex].Occupant);
                    column[newHex - 1].Occupant.SuicideAfterEating = true;
                    column[curHex].Occupant = null;
                    column[newHex - 1].Occupant = null;
                    somethingMoved = true;
                }
                else if (newHex > 0
                    && column[curHex].CurrentLevel != ImageForBlockProgression.Count
                    && column[newHex - 1].CurrentLevel != ImageForBlockProgression.Count
                    && (column[curHex].CurrentLevel == column[newHex - 1].CurrentLevel
                        || column[curHex].Occupant.Kind == BlockKind.WildCard
                        || column[newHex - 1].Occupant.Kind == BlockKind.WildCard)
                    && (column[curHex].Occupant.Kind == BlockKind.Normal
                        || column[newHex - 1].Occupant.Kind == BlockKind.Normal)) {
                    // this is a block that's about to combine with another block - either both blocks are
                    // the same level or one of them is a wild card (bot not both)
                    newHex -= 1;
                    int newLevel = column[curHex].Occupant.Kind == BlockKind.WildCard
                        ? column[newHex].Occupant.Level + 1
                        : column[curHex].Occupant.Level + 1;
                    
                    column[curHex].Occupant.SlideTo(column[newHex]);
                    column[curHex].Occupant.BlocksToEat.Add(column[newHex].Occupant);
                    column[curHex].Occupant.Level  = newLevel;
                    column[curHex].Occupant.Kind = BlockKind.Normal;
                    column[newHex].Occupant = column[curHex].Occupant;
                    column[curHex].Occupant = null;
                    somethingMoved = true;
                }
                else if (newHex < curHex) {
                    // this is a block that's just sliding with no interaction
                    column[newHex].Occupant = column[curHex].Occupant;
                    column[curHex].Occupant = null;
                    column[newHex].Occupant.SlideTo(column[newHex]);
                    somethingMoved = true;
                }
            }
        }

        swipeDir = BoardDirection.Null;
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void DropBlocks() {
        for (int i = 0; i < dropsPerTurn; i++) {
            if (IsBoardFull) {
                EnterGameOverState();
                return;
            }

            List<Hex> openHexes = (from hex in hexes where hex.Occupant == null select hex).ToList();
            BlockKind newBlockKind = BlockKind.Normal;
            float rand = Random.Range(0f, 100f);
            
            if (turnCount >= wildTurnReq && rand < wildChance) {
                newBlockKind = BlockKind.WildCard;
            }
            else if (turnCount >= anvilTurnReq && rand < wildChance + anvilChance) {
                newBlockKind = BlockKind.Anvil;
            }
            else if (turnCount >= plantTurnReq && rand < wildChance + anvilChance + plantChance) {
                newBlockKind = BlockKind.Plant;
            }

            if (newBlockKind == BlockKind.Anvil || newBlockKind == BlockKind.Plant) {
                List<Hex> openInteriorHexes = (from hex in openHexes 
                    where hex.Neighbors.Count == 6
                    select hex).ToList();

                newDropHex = openInteriorHexes.Count > 0
                    ? openInteriorHexes[Random.Range(0, openInteriorHexes.Count - 1)]
                    : openHexes[Random.Range(0, openHexes.Count - 1)];
            }
            else {
                newDropHex = openHexes[Random.Range(0, openHexes.Count - 1)];
            }
            
            newBlock = Instantiate(BlockPrefab, UiCanvasObj.transform).GetComponent<Block>();
            newBlock.transform.position = newDropHex.transform.position;
            newBlock.Initialize(newDropHex, 1, newBlockKind);
            newDropHex.Occupant = newBlock;
            blocks.Add(newBlock);

            Instantiate(
                CreateCelebrationPrefab,
                newBlock.transform.position,
                Quaternion.identity,
                UiCanvasObj.transform);
        }

        turnCount++;
    }
    
    //--------------------------------------------------------------------------------------------------------
    private BoardDirection GetSwipeDirection() {
        // get keyboard input from PC
        if (Input.GetKeyDown(KeyCode.Keypad7)) return BoardDirection.UpLeft;
        if (Input.GetKeyDown(KeyCode.Keypad8)) return BoardDirection.Up;
        if (Input.GetKeyDown(KeyCode.Keypad9)) return BoardDirection.UpRight;
        if (Input.GetKeyDown(KeyCode.Keypad1)) return BoardDirection.DownLeft;
        if (Input.GetKeyDown(KeyCode.Keypad2)) return BoardDirection.Down;
        if (Input.GetKeyDown(KeyCode.Keypad3)) return BoardDirection.DownRight;
        
        // get touch input from device
        if (Input.touchCount != 1) {
            return BoardDirection.Null;
        }
        
        currentTouch = Input.GetTouch(0);
        
        switch (currentTouch.phase) {
            case TouchPhase.Began:
                // we just put our finer on the screen

                if (IsGameOver) {
                    StartNewGame();
                    return BoardDirection.Null;
                }
                
                swipeStartPos = currentTouch.position;
                swipeEndPos = currentTouch.position;
                break;
            
            case TouchPhase.Ended:
                // we just pulled our finger off the screen
                swipeEndPos = currentTouch.position;
                float swipeDist = Math.Abs(Vector3.Distance(swipeStartPos, swipeEndPos));
                
                if (swipeDist < swipeMinDist) {
                    break;
                }

                float swipeAngle = Vector3.Angle(Vector3.up, swipeEndPos - swipeStartPos);
                bool swipingRightish = swipeEndPos.x > swipeStartPos.x;

                if (swipeAngle < 30f) {
                    return BoardDirection.Up;
                }
                else if (30f <= swipeAngle && swipeAngle <= 90f) {
                    return swipingRightish ? BoardDirection.UpRight : BoardDirection.UpLeft;
                }
                else if (90f <= swipeAngle && swipeAngle <= 150f) {
                    return swipingRightish ? BoardDirection.DownRight : BoardDirection.DownLeft;
                }
                else {
                    return BoardDirection.Down;
                }
        }
        
        // no input detected
        return BoardDirection.Null;
    }
    
    //--------------------------------------------------------------------------------------------------------
    private static BoardDirection Opposite(BoardDirection dir) {
        switch (dir) {
            case BoardDirection.UpLeft: return BoardDirection.DownRight;
            case BoardDirection.Up: return BoardDirection.Down;
            case BoardDirection.DownLeft: return BoardDirection.UpRight;
            case BoardDirection.UpRight: return BoardDirection.DownLeft;
            case BoardDirection.Down: return BoardDirection.Up;
            case BoardDirection.DownRight: return BoardDirection.UpLeft;
        }
        return BoardDirection.Null;
    }
    
    //--------------------------------------------------------------------------------------------------------
    private static List<Hex> HexesInDirection(Hex source, BoardDirection dir) {
        List<Hex> result = new List<Hex> { source };
        Hex crawler = source;
        
        while (crawler.NeighborDirections.Contains(dir)) {
            int dirIndex = crawler.NeighborDirections.FindIndex(d => d == dir);
            crawler = crawler.Neighbors[dirIndex];
            result.Add(crawler);
        }

        return result;
    }
    
    //--------------------------------------------------------------------------------------------------------
    private static Vector3 GetAnvilDestination(List<Hex> column) {
        Vector3 pos0 = column[0].transform.position;
        Vector3 pos1 = column[1].transform.position;
        return pos0 + (pos0 - pos1);
    }
    
    //--------------------------------------------------------------------------------------------------------
    private static int DeliciousEmptyHex(List<Hex> column, int curHex) {
        // returns the index at which the block in curHex will fall using normal gravity rules, i.e.
        // by ignoring combine rules and special blocks
        for (int restHex = curHex - 1; restHex >= 0; restHex--) {
            if (column[restHex].CurrentLevel != 0) {
                return restHex + 1;
            }
        }

        return 0;
    }
}
