using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour {

    public GameObject BoardObj;
    public GameObject SushiAnchor;
    public GameObject BlockPrefab;
    public List<Sprite> ImageForBlockProgression;
    public List<HungryNeko> HungryNekos;
    public Sprite ImageForWildCard;
    public GameObject CreateCelebrationPrefab;
    public ScoreDisplay ScoreDisplayObj;
    public ScoreMultiplierPanel ScoreMultPanel;
    public UnlockProgressBar UnlockProgressBar;
    public bool SomethingJustPromoted;
    public GameOverPanel GameOverPanelObj;
    public float CurrentWildChance;
    public float CurrentDoubleChance;
    public float CurrentTripleChance;
    public int CurrentHungryNekoInterval;
    public int MovesSinceLastHungryNeko;
    public int Score;
    public int HighScore;
    public GameObject NewHighScoreIndicator;
    public GameObject CurrentHighScoreIndicator;
    public TextMeshProUGUI CurrentHighScoreLabel;
    public PremiumController PremiumControllerObj;
    public bool WaitForFingerUpToCommitSwipe;
    public GameObject SwipeTutorialObj;
    public Hex CenterHex;
    public bool IsFreshGame;
    // this var sucks and it's here to fix a weird problem I don't know how to fix otherwise
    public List<Block> GlobalFood;
    
    private List<Hex> hexes;
    private List<Hex> openHexes;
    private List<Block> blocks;
    private BoardDirection swipeDir;
    private Hex newDropHex;
    private Block newBlock;
    private bool allowInput;
    private static Touch currentTouch;
    private static float swipeMinDist;
    private static Vector3? swipeStartPos;
    private static Vector3? swipeEndPos;
    private float d100Roll;
    private bool isGameOver;
    private GameObject swipeTutorialInstance;

    private bool IsAnyBlockMoving => blocks.Any(b => b.IsMoving);

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
    private void StartNewGame() {
        foreach (Block block in blocks) {
            Destroy(block.gameObject);
        }
        
        // reset the sim
        blocks.Clear();
        swipeDir = BoardDirection.Null;
        CurrentWildChance = 0f;
        CurrentDoubleChance = 0f;
        CurrentTripleChance = 0f;
        CurrentHungryNekoInterval = -1;
        MovesSinceLastHungryNeko = 0;
        Score = 0;
        isGameOver = false;
        IsFreshGame = true;
        GlobalFood = new List<Block>(); 
        
        // reset high score
        HighScore = PlayerPrefs.GetInt(playerPrefHighScoreKey);
        NewHighScoreIndicator.SetActive(false);
        if (HighScore > 0) {
            CurrentHighScoreIndicator.SetActive(true);
            CurrentHighScoreLabel.text = "High Score: " + HighScore;
        }
        else {
            CurrentHighScoreIndicator.SetActive(false);
        }
        
        // reset other things
        ScoreMultPanel.ResetLevel();
        UnlockProgressBar.currentUnlock = 0;
        UnlockProgressBar.levelLabel.text = (UnlockProgressBar.currentUnlock + 1).ToString();
        
        // record number of games started on this device
        int gamesStarted = PlayerPrefs.GetInt(playerPrefsGameCountKey) + 1;
        PlayerPrefs.SetInt(playerPrefsGameCountKey, gamesStarted);
        PlayerPrefs.Save();
        Debug.Log("Starting game " + gamesStarted + " on this device.");
        
        // start the game
        CreateFirstBlock();
        SaveGameState();
        allowInput = true;
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void EnterGameOverState() {
        isGameOver = true;
        GameOverPanelObj.ShowResultsMenu();
        GameOverPanelObj.SushiBoatResults = SushiAnchor;
        GameOverPanelObj.ScoreResult = Score;
        GameOverPanelObj.IsHighScore = Score >= HighScore;
        GameOverPanelObj.GameIsOver();
    }

    //--------------------------------------------------------------------------------------------------------
    public void CheatGameOver() {
        EnterGameOverState();
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void Update() {
        if (isGameOver) {
            return;
        }
        
        blocks.RemoveAll(b => b == null);
        
        if (!allowInput && !IsAnyBlockMoving) {
            // blocks just finished moving to their destinations
            if (!SomethingJustPromoted) {
                ScoreMultPanel.ResetLevel();
            }

            // this part sucks - somehow there's food left over from last frame?
            foreach (Block food in GlobalFood) {
                Destroy(food.gameObject);
            }
            GlobalFood.Clear();

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

        if (!somethingMoved) {
            return;
        }
        
        allowInput = false;
        swipeTutorialInstance.SetActive(false);
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void ExecuteSwipe(out bool somethingMoved) {
        // 'captains' are the five hexes at the edge of the board in the swiped direction. this whole
        // function generally works by starting with those captain hexes then looking backwards to see
        // which blocks need to be 'sucked' in that direction.
        IEnumerable<Hex> captains = from h in hexes where !h.NeighborDirections.Contains(swipeDir) select h;
        BoardDirection searchDir = Opposite(swipeDir);
        somethingMoved = false;

        foreach (Hex captain in captains) {
            List<Hex> column = HexesInDirection(captain, searchDir);
            HungryNeko activeNeko = AdjacentHungryNeko(captain, searchDir);
            
            for (int curHex = 0; curHex < column.Count; curHex++) {
                if (column[curHex].CurrentLevel == 0) {
                    // this is an empty hex - do nothing
                    continue;
                }
                
                int newHex = DeliciousEmptyHex(column, curHex);
                
                if (newHex == 0
                    && activeNeko != null
                    && activeNeko.IsHungry
                    && activeNeko.BlockLevel == column[curHex].CurrentLevel) {
                    
                    // this is a block that's about to be fed to the neko
                    activeNeko.BlocksIncoming++;
                    column[curHex].Occupant.SlideTo(activeNeko, swipeDir);
                    column[curHex].Occupant = null;
                    somethingMoved = true;
                }
                else if (newHex > 0
                    && column[curHex].CurrentLevel != ImageForBlockProgression.Count // we not max level
                    && column[newHex - 1].CurrentLevel != ImageForBlockProgression.Count // they not max level
                    && (column[curHex].CurrentLevel == column[newHex - 1].CurrentLevel // equal lvl or wild
                        || column[curHex].Occupant.Kind == BlockKind.WildCard
                        || column[newHex - 1].Occupant.Kind == BlockKind.WildCard)
                    && (column[curHex].Occupant.Kind == BlockKind.Normal // both not wild
                        || column[newHex - 1].Occupant.Kind == BlockKind.Normal)) {
                    
                    // this is a block that's about to combine with another block - either both blocks are
                    // the same level or one of them is a wild card (bot not both)
                    newHex -= 1;
                    int newLevel = column[curHex].Occupant.Kind == BlockKind.WildCard
                        ? column[newHex].Occupant.Level + 1
                        : column[curHex].Occupant.Level + 1;
                    
                    column[curHex].Occupant.SlideTo(column[newHex], swipeDir);
                    column[curHex].Occupant.BlocksToEat.Add(column[newHex].Occupant);
                    GlobalFood.Add(column[newHex].Occupant);
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

        // if we have a hungry neko interval and no currently hungry nekos, increment our counter
        if (CurrentHungryNekoInterval != -1 && !IsAnyNekoHungry()) {
            MovesSinceLastHungryNeko++;
            
            if (MovesSinceLastHungryNeko > CurrentHungryNekoInterval) { // ignore this bs off-by-one err
                MakeNekoHungry();
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

        // if the board only has 2 or 3 hexes open, don't suddenly fill all of them and make the
        // player lose - instead, make sure there's still 1 more empty hex after the drop
        int openHexCount = (from hex in hexes where hex.Occupant == null select hex).Count();
        if (openHexCount > 1 && newBlockCount >= openHexCount) {
            newBlockCount = openHexCount - 1;
        }
        
        // create as many blocks as we need
        for (int i = 0; i < newBlockCount; i++) {
            openHexes = (from hex in hexes where hex.Occupant == null select hex).ToList();    
            d100Roll = Random.Range(0f, 100f);
            BlockKind newBlockKind = d100Roll <= CurrentWildChance ? BlockKind.WildCard : BlockKind.Normal;
            newDropHex = openHexes[Random.Range(0, openHexes.Count - 1)];
        
            CreateBlock(newDropHex, newBlockKind, 1, true);
            
            if (blocks.Count == hexes.Count) {
                EnterGameOverState();
                return;
            }
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void CreateBlock(Hex location, BlockKind kind, int level, bool celebrate) {
        newBlock = Instantiate(
            original: BlockPrefab,
            position: location.transform.position,
            rotation: Quaternion.identity,
            parent: SushiAnchor.transform).GetComponent<Block>();
        
        newBlock.Initialize(location, level, kind);
        location.Occupant = newBlock;
        blocks.Add(newBlock);

        if (celebrate) {
            Instantiate(
                original: CreateCelebrationPrefab,
                position: newBlock.transform.position,
                rotation: Quaternion.identity,
                parent: SushiAnchor.transform);
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    private BoardDirection GetSwipeDirection() {
        BoardDirection result = BoardDirection.Null;
        
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

        if (currentTouch.phase == TouchPhase.Began) {
            // we just put our finger on the screen
            if (isGameOver) {
                StartNewGame();
                return BoardDirection.Null;
            }
                
            swipeStartPos = currentTouch.position;
        }
        else if (swipeStartPos != null &&
                ((WaitForFingerUpToCommitSwipe && currentTouch.phase == TouchPhase.Ended) ||
                 (!WaitForFingerUpToCommitSwipe && currentTouch.phase == TouchPhase.Moved))) {
            
            // have we swiped far enough on the screen to actually execute a swipe? 
            swipeEndPos = currentTouch.position;
            float swipeDist = Math.Abs(Vector3.Distance((Vector3)swipeStartPos, (Vector3)swipeEndPos));
            if (swipeDist >= swipeMinDist) {
                result = GetBoardDirectionFromPoints((Vector3)swipeStartPos, (Vector3)swipeEndPos);
                swipeStartPos = null;
            }
        }
        
        // no input detected
        return result;
    }
    
    //--------------------------------------------------------------------------------------------------------
    private static BoardDirection GetBoardDirectionFromPoints(Vector3 endPoint, Vector3 startPoint) {
        float swipeAngle = Vector3.Angle(Vector3.up, startPoint - endPoint);
        bool swipingRightish = startPoint.x > endPoint.x;

        if (swipeAngle < 30f) {
            return BoardDirection.Up;
        }
        if (30f <= swipeAngle && swipeAngle <= 90f) {
            return swipingRightish ? BoardDirection.UpRight : BoardDirection.UpLeft;
        }
        if (90f <= swipeAngle && swipeAngle <= 150f) {
            return swipingRightish ? BoardDirection.DownRight : BoardDirection.DownLeft;
        }
        
        return BoardDirection.Down;
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
            // new high score! save it to device.
            HighScore = Score;
            PlayerPrefs.SetInt(playerPrefHighScoreKey, HighScore);
            PlayerPrefs.Save();
            
            // swap out which indicators we're showing if we haven't already 
            if (!NewHighScoreIndicator.activeSelf) {
                NewHighScoreIndicator.SetActive(true);
                CurrentHighScoreIndicator.SetActive(false);
            }
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void ResetHighScore() {
        PlayerPrefs.SetInt(playerPrefHighScoreKey, 0);
        PlayerPrefs.Save();
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void SaveGameState() {
        // put all of the blocks on the board (and the current score) into a serializable state
        SaveState saveState = new SaveState {
            Score = this.Score,
            Multiplier = ScoreMultPanel.CurrentLevel
        };
        
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
        for (int i = 0; i < saveState.BlockLocations.Count; i++) {
            CreateBlock(
                location: hexes[saveState.BlockLocations[i]],
                kind: saveState.BlockKinds[i],
                level: saveState.BlockLevels[i],
                celebrate: false);
        }
        Score = saveState.Score;
        ScoreMultPanel.CurrentLevel = saveState.Multiplier;
        ScoreMultPanel.CreateComboPrefab();
        
        // misc initialization tasks
        ScoreDisplayObj.Snap();
        swipeDir = BoardDirection.Null;
        allowInput = true;
        HighScore = PlayerPrefs.GetInt(playerPrefHighScoreKey);
        IsFreshGame = Score == 0; // score will be > 0 if any blocks have combined

        Debug.Log("Game loaded. High score: " + HighScore +
              ", Games started: " + PlayerPrefs.GetInt(playerPrefsGameCountKey) +
              ", Premium status: " + PremiumControllerObj.GameIsPremium);
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void CreateFirstBlock() {
        Debug.Log("Dropping the first block ever.");
        /*openHexes = (from hex in hexes where hex.Neighbors.Count == 6 select hex).ToList();
        //newDropHex = openHexes[Random.Range(0, openHexes.Count - 1)];
        newDropHex = openHexes[3];*/
        
        CreateBlock(CenterHex, BlockKind.Normal, 1, true);

        if (swipeTutorialInstance == null) {
            swipeTutorialInstance = Instantiate(
                original: SwipeTutorialObj,
                position: CenterHex.transform.position,
                rotation: Quaternion.identity,
                parent: SushiAnchor.transform);
        }
        else {
            swipeTutorialInstance.SetActive(true);
            swipeTutorialInstance.transform.position = CenterHex.transform.position;
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    private HungryNeko AdjacentHungryNeko(Hex hex, BoardDirection dir) {
        foreach (HungryNeko neko in HungryNekos) {
            if (neko.IsHungry) {
                for (int i = 0; i < neko.FeedingHexes.Count; i++) {
                    if (neko.FeedingHexes[i] == hex && neko.FeedingDirections[i] == dir) {
                        return neko;
                    }
                }
            }
        }

        return null;
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void MakeNekoHungry() {
        int randNeko = Random.Range(0, HungryNekos.Count);
        int randLevel = Random.Range(1, 3);
        int randCount = Random.Range(3, 5);
        HungryNekos[randNeko].GetHungry(randLevel, randCount);
        MovesSinceLastHungryNeko = 0;
    }
    
    //--------------------------------------------------------------------------------------------------------
    private bool IsAnyNekoHungry() {
        return HungryNekos.Any(neko => neko.IsHungry);
    }
}
