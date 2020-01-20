using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour {

    public GameObject BoardObj;
    public GameObject UiCanvasObj;
    public GameObject SushiAnchor;
    public GameObject BlockPrefab;
    public List<Sprite> ImageForBlockProgression;
    public Sprite ImageForWildCard;
    public Sprite ImageForAnvil;
    public Sprite ImageForPlant;
    public GameObject CreateCelebrationPrefab;
    public GameObject CombineCelebrationPrefab;
    public GameObject DestroyCelebrationPrefab;
    public ScoreDisplay ScoreDisplayObj;
    public ScoreMultiplierPanel ScoreMultPanel;
    public UnlockProgressBar UnlockProgressBar;
    public bool SomethingJustPromoted;
    public GameObject GameOverPanel;
    public float CurrentWildChance;
    public float CurrentDoubleChance;
    public float CurrentTripleChance;
    public int HighScore;
    public GameObject HighScoreIndicator;
    public PremiumController PremiumControllerObj;
    
    public int Score { get; private set; }
    
    private List<Hex> hexes;
    private List<Hex> openHexes;
    private List<Block> blocks;
    private BoardDirection swipeDir;
    private Hex newDropHex;
    private Block newBlock;
    private bool allowInput;
    private static Touch currentTouch;
    private static float swipeMinDist;
    private static Vector3 swipeStartPos;
    private static Vector3 swipeEndPos;
    private float d100Roll;

    private bool IsBoardFull => blocks.Count >= hexes.Count;
    private bool IsAnyBlockMoving => blocks.Any(b => b.IsMoving);
    private bool IsGameOver => GameOverPanel.activeSelf;

    private const float minSwipeDistScreenFration = 0.1f;
    private const string playerPrefHighScoreKey = "HighScore";
    private const string playerPrefsGameCountKey = "GamesStarted";
    private const string saveFileName = "/savegame.save";
    
    //--------------------------------------------------------------------------------------------------------
    private void Start() {
        swipeMinDist = Screen.height * minSwipeDistScreenFration;
        hexes = BoardObj.transform.GetComponentsInChildren<Hex>().ToList();
        blocks = new List<Block>();
        Canvas.ForceUpdateCanvases();
        LoadGameStateOrStartNewGame();
    }

    //--------------------------------------------------------------------------------------------------------
    public void StartNewGame() {
        foreach (Block block in blocks) {
            Destroy(block.gameObject);
        }
        
        // reset various things for a new game
        blocks = new List<Block>();
        swipeDir = BoardDirection.Null;
        GameOverPanel.SetActive(false);
        CurrentWildChance = 0f;
        CurrentDoubleChance = 0f;
        CurrentTripleChance = 0f;
        HighScore = PlayerPrefs.GetInt(playerPrefHighScoreKey);
        Score = 0;
        ScoreMultPanel.ResetLevel();
        UnlockProgressBar.currentUnlock = 0;
        UnlockProgressBar.levelLabel.text = "Level " + (UnlockProgressBar.currentUnlock + 1);
        
        // record number of games started on this device
        int gamesStarted = PlayerPrefs.GetInt(playerPrefsGameCountKey) + 1;
        PlayerPrefs.SetInt(playerPrefsGameCountKey, gamesStarted);
        PlayerPrefs.Save();
        Debug.Log("Starting game " + gamesStarted + " on this device.");
        
        // start the game
        CreateNewBlocks();
        SaveGameState();
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

            CreateNewBlocks();
            SaveGameState();
            allowInput = true;
            SomethingJustPromoted = false;
        }
        
        swipeDir = GetSwipeDirection();
        
        if (swipeDir == BoardDirection.Null) {
            return;
        }

        ExecuteSwipe(out bool somethingMoved);

        if (somethingMoved) {
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
                if (column[curHex].CurrentLevel == 0) {
                    // this is an empty hex - do nothing
                    continue;
                }
                                
                int newHex = DeliciousEmptyHex(column, curHex);

//                if (newHex > 0 && column[newHex - 1].Occupant.Kind == BlockKind.Plant) {
//                    // this is a non-anvil block that's about to fall into a plant
//                    column[curHex].Occupant.SlideTo(column[newHex - 1].transform.position, swipeDir);
//                    column[newHex - 1].Occupant.BlocksToEat.Add(column[curHex].Occupant);
//                    column[newHex - 1].Occupant.SuicideAfterEating = true;
//                    column[curHex].Occupant = null;
//                    column[newHex - 1].Occupant = null;
//                    somethingMoved = true;
//                }
                //else if (newHex > 0
                if (newHex > 0
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
                    
                    column[curHex].Occupant.SlideTo(column[newHex], swipeDir);
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
                    column[newHex].Occupant.SlideTo(column[newHex], swipeDir);
                    somethingMoved = true;
                }
                else if (newHex == curHex) {
                    // this block has nowhere to go
                    column[newHex].Occupant.SlideTo(column[newHex], swipeDir);
                }
            }
        }

        swipeDir = BoardDirection.Null;
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void CreateNewBlocks() {
        // how many blocks are we supposed to create?
        int newBlockCount = 1;
        d100Roll = Random.Range(0f, 100f);
        
        if (d100Roll <= CurrentDoubleChance) {
            newBlockCount = 2;
        }
        else if (d100Roll <= CurrentDoubleChance + CurrentTripleChance) {
            newBlockCount = 3;
        }

        // create that many blocks
        for (int i = 0; i < newBlockCount; i++) {
            if (IsBoardFull) {
                EnterGameOverState();
                return;
            }

            openHexes = (from hex in hexes where hex.Occupant == null select hex).ToList();
            d100Roll = Random.Range(0f, 100f);
            BlockKind newBlockKind = d100Roll <= CurrentWildChance ? BlockKind.WildCard : BlockKind.Normal;
            newDropHex = openHexes[Random.Range(0, openHexes.Count - 1)];
        
            CreateBlock(newDropHex, newBlockKind, 1);

            Instantiate(
                original: CreateCelebrationPrefab,
                position: newBlock.transform.position,
                rotation: Quaternion.identity,
                parent: SushiAnchor.transform);
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void CreateBlock(Hex location, BlockKind kind, int level) {
        newBlock = Instantiate(
            original: BlockPrefab,
            position: location.transform.position,
            rotation: Quaternion.identity,
            parent: SushiAnchor.transform).GetComponent<Block>();
        newBlock.Initialize(location, level, kind);
        location.Occupant = newBlock;
        blocks.Add(newBlock);
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
    
    //--------------------------------------------------------------------------------------------------------
    public void ChangeScore(int change) {
        Score += change;

        if (HighScore < Score) {
            HighScore = Score;
            PlayerPrefs.SetInt(playerPrefHighScoreKey, Score);
            PlayerPrefs.Save();
            
            if (!HighScoreIndicator.activeSelf) {
                HighScoreIndicator.SetActive(true);
            }
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void SaveGameState() {
        // put all of the blocks on the board (and the current score) into a serializable state
        SaveState saveState = new SaveState {Score = this.Score};
        for (int i = 0; i < hexes.Count; i++) {
            if (hexes[i].Occupant != null) {
                saveState.BlockLocations.Add(i);
                saveState.BlockKinds.Add(hexes[i].Occupant.Kind);
                saveState.BlockLevels.Add(hexes[i].Occupant.Level);    
            }
        }
        
        // save the state to a file
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + saveFileName);
        formatter.Serialize(file, saveState);
        file.Close();
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void LoadGameStateOrStartNewGame() {
        // if this device has no saved game, just start a new game
        if (!File.Exists(Application.persistentDataPath + saveFileName)) {
            StartNewGame();
            return;
        }
        
        // fetch the save state from the save file
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + saveFileName, FileMode.Open);
        SaveState saveState = (SaveState)formatter.Deserialize(file);
        file.Close();
        
        // populate the board according to the saved state
        Score = saveState.Score;
        for (int i = 0; i < saveState.BlockLocations.Count; i++) {
            CreateBlock(hexes[saveState.BlockLocations[i]], saveState.BlockKinds[i], saveState.BlockLevels[i]);
        }
        
        // misc initialization tasks
        ScoreDisplayObj.Snap();
        swipeDir = BoardDirection.Null;
        allowInput = true;
        HighScore = PlayerPrefs.GetInt(playerPrefHighScoreKey);

        Debug.Log("Game loaded. High score: " + HighScore +
                  ", Games started: " + PlayerPrefs.GetInt(playerPrefsGameCountKey) +
                  ", Premium status: " + PremiumControllerObj.GameIsPremium);
    }
}
