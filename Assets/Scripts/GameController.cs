﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
using GoogleMobileAds.Api;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour {

    public GameObject BoardObj;
    public GameObject SushiAnchor;
    public GameObject SwipeTutorial;
    public GameObject CombineTutorial;
    public GameObject WildCardTutorial;
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
    public Hex CenterHex;
    public List<Hex> OrderedCornerHexes;
    public bool ForceWildCardNextTurn;
    public List<AudioClip> SwipeAudioClips;
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
    private static Vector3? swipeStartPos;
    private static Vector3? swipeEndPos;
    private bool isGameOver;
    private AudioManager audioManager;

    private int turnCount;
    private Localizer localizer;
    private bool IsAnyBlockMoving => blocks.Any(b => b.IsMoving);
    public bool IsFreshGame => turnCount <= 5;

    private const float swipeMinDist = 60f; // feels good?
    private const string playerPrefHighScoreKey = "HighScore";
    private const string playerPrefsGameCountKey = "GamesStarted";
    private const string saveFileName = "/savegame.save";
    private readonly List<BoardDirection> fullBoardCheckDirections = new List<BoardDirection>
        { BoardDirection.Down, BoardDirection.DownLeft, BoardDirection.DownRight };

    //--------------------------------------------------------------------------------------------------------
    private void Start() {
        hexes = BoardObj.transform.GetComponentsInChildren<Hex>().ToList();
        blocks = new List<Block>();
        localizer = GetComponent<Localizer>();
        audioManager = GetComponent<AudioManager>();
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
        turnCount = 1;
        Score = 0;
        isGameOver = false;
        ForceWildCardNextTurn = false;
        GlobalFood = new List<Block>();
        foreach (HungryNeko neko in HungryNekos) {
            neko.Reset();
        }
        foreach (Hex hex in hexes) {
            hex.Occupant = null;
        }
        
        // reset high score
        HighScore = PlayerPrefs.GetInt(playerPrefHighScoreKey);
        NewHighScoreIndicator.SetActive(false);
        if (HighScore > 0) {
            CurrentHighScoreIndicator.SetActive(true);
            CurrentHighScoreLabel.text = HighScore.ToString();
        }
        else {
            CurrentHighScoreIndicator.SetActive(false);
        }
        
        // reset other things
        ClearTutorials();
        UnlockProgressBar.Reset();
        ScoreMultPanel.ResetLevel();
        UnlockProgressBar.LevelLabel.text = (UnlockProgressBar.CurrentUnlock + 1).ToString();
        
        // record number of games started on this device
        int gamesStarted = PlayerPrefs.GetInt(playerPrefsGameCountKey) + 1;
        PlayerPrefs.SetInt(playerPrefsGameCountKey, gamesStarted);
        PlayerPrefs.Save();
        Debug.Log("Starting game " + gamesStarted + " on this device.");
        
        // start the game
        CreateBlocks();
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
        SaveGameState();
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

            CreateBlocks();
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

        turnCount++;
        allowInput = false;
        ClearTutorials();
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void ExecuteSwipe(out bool somethingMoved) {
        // 'captains' are the five hexes at the edge of the board in the swiped direction. this whole
        // function generally works by starting with those captain hexes then looking backwards to see
        // which blocks need to be 'sucked' in that direction.
        IEnumerable<Hex> captains = from h in hexes where !h.NeighborDirections.Contains(swipeDir) select h;
        BoardDirection searchDir = Opposite(swipeDir);
        somethingMoved = false;

        // pick a random swipe sound and play it
        if (SwipeAudioClips.Count > 0) {
            audioManager.Play(SwipeAudioClips[Random.Range(0, SwipeAudioClips.Count)]);
        }

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
        if (somethingMoved && CurrentHungryNekoInterval != -1 && !IsAnyNekoHungry()) {
            MovesSinceLastHungryNeko++;
            
            if (MovesSinceLastHungryNeko > CurrentHungryNekoInterval) { // ignore this bs off-by-one err
                MakeRandomNekoHungry();
            }
        }
        
        swipeDir = BoardDirection.Null;
    }

    //--------------------------------------------------------------------------------------------------------
    private void CreateBlocks() {
        // handle the early game using special block-creating logic
        switch (turnCount) {
            case 1:
                CreateFirstBlock();
                return;
            case 2:
                CreateSecondBlock();
                return;
        }
        
        int openHexCount = (from hex in hexes where hex.Occupant == null select hex).Count();
        if (openHexCount == 0) {
            // how did we get here?
            return;
        }
        
        // how many blocks are we supposed to create?
        int newBlockCount = 1;
        float d100Roll = Random.Range(0f, 100f);
        if (d100Roll <= CurrentDoubleChance) {
            newBlockCount = 2;
        }
        else if (d100Roll <= CurrentDoubleChance + CurrentTripleChance) {
            newBlockCount = 3;
        }

        // if the board only has 2 or 3 hexes open, don't suddenly fill all of them and make the
        // player lose - instead, make sure there's still 1 more empty hex after the drop
        if (openHexCount > 1 && newBlockCount >= openHexCount) {
            newBlockCount = openHexCount - 1;
        }
        
        // create as many blocks as we need
        for (int i = 0; i < newBlockCount; i++) {
            d100Roll = Random.Range(0f, 100f);
            BlockKind newBlockKind = d100Roll <= CurrentWildChance ? BlockKind.WildCard : BlockKind.Normal;
            openHexes = (from hex in hexes where hex.Occupant == null select hex).ToList();
            newDropHex = openHexes[Random.Range(0, openHexes.Count - 1)];
        
            CreateBlock(newDropHex, newBlockKind, 1, true);
            
            // if the board is full and there are no moves available, end the game
            if (IsBoardFullAndImmobile()) {
                EnterGameOverState();
                return;
            }
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    private bool IsBoardFullAndImmobile() {
        // if the board is not full then the game isn't over yet
        if (blocks.Count < hexes.Count) {
            return false;
        }

        // if any sushi on the board can combine with any of its neighbors then the game isn't over yet
        foreach (Hex hex in hexes) {
            // max level sushi never combines with anything
            if (hex.Occupant.Level == ImageForBlockProgression.Count) {
                continue;
            }
            
            // when we find a wasabi, we need to make sure it's not completely surrounded by max level blocks
            // or other wasabi (because that's comically unlikely but still technically possible)
            if (hex.Occupant.Kind == BlockKind.WildCard) {
                if (hex.Neighbors.Any(neighbor =>
                    neighbor.Occupant.Kind == BlockKind.Normal &&
                    neighbor.Occupant.Level < ImageForBlockProgression.Count)) {
                    
                    return false;
                }
            }

            // this works by checking the down, down-left, and down-right neighboring positions. if
            // there's an identical sushi in any of those positions then we know a combination is possible
            for (int d = 0; d < hex.NeighborDirections.Count; d++) {
                if (!fullBoardCheckDirections.Contains(hex.NeighborDirections[d])) {
                    continue;
                }

                if (hex.Occupant.Level == hex.Neighbors[d].Occupant.Level) {
                    return false;
                }
            }
        }

        return true;
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void CreateBlock(Hex location, BlockKind kind, int level, bool celebrate) {
        if (ForceWildCardNextTurn) {
            kind = BlockKind.WildCard;
            ShowWildcardTutorial(location);
            ForceWildCardNextTurn = false;
        }
        
        newBlock = Instantiate(
            original: BlockPrefab,
            position: location.transform.position,
            rotation: Quaternion.identity,
            parent: SushiAnchor.transform).GetComponent<Block>();
        
        newBlock.Initialize(location, level, kind, audioManager);
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
    public void SaveGameState() {
        // put all of the blocks on the board (and the current score) into a serializable state
        
        SaveState saveState = new SaveState {
            Score = this.Score,
            TurnCount = turnCount,
            CurrentWildChance = this.CurrentWildChance,
            CurrentDoubleChance = this.CurrentDoubleChance,
            CurrentTripleChance = this.CurrentTripleChance,
            CurrentHungryNekoInterval = this.CurrentHungryNekoInterval,
            MovesSinceLastHungryNeko = this.MovesSinceLastHungryNeko,
            Multiplier = ScoreMultPanel.CurrentLevel,
            IsGameOver = isGameOver,
            Language = localizer.CurrentLanguage,
            AudioEnabled = audioManager.AudioEnabled
        };
        
        for (int i = 0; i < hexes.Count; i++) {
            if (hexes[i].Occupant != null) {
                saveState.BlockLocations.Add(i);
                saveState.BlockKinds.Add(hexes[i].Occupant.Kind);
                saveState.BlockLevels.Add(hexes[i].Occupant.Level);    
            }
        }

        for (int i = 0; i < HungryNekos.Count; i++) {
            if (HungryNekos[i].IsHungry) {
                saveState.HungryNekoLocation = i;
                saveState.HungryNekoLevel = HungryNekos[i].BlockLevel;
                saveState.HungryNekoCount = HungryNekos[i].BlocksLeft;
                break;
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
            localizer.ResetLanguageToDeviceLanguage();
            StartNewGame();
            return;
        }
        
        // fetch the save state from the save file
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + saveFileName, FileMode.Open);
        SaveState saveState = (SaveState)formatter.Deserialize(file);
        file.Close();
        
        // if the loaded game is actually over, start a new one instead
        if (saveState.IsGameOver) {
            StartNewGame();
            return;
        }
        
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
        turnCount = saveState.TurnCount;
        
        // update the level rewards bar
        CurrentWildChance = saveState.CurrentWildChance;
        CurrentDoubleChance = saveState.CurrentDoubleChance;
        CurrentTripleChance = saveState.CurrentTripleChance;
        CurrentHungryNekoInterval = saveState.CurrentHungryNekoInterval;
        MovesSinceLastHungryNeko = saveState.MovesSinceLastHungryNeko;
        ForceWildCardNextTurn = false;
        UnlockProgressBar.SnapToCurrentScoreWithoutRewards();
        
        // initialize the current hungry neko, if we have one
        if (saveState.HungryNekoCount > 0) {
            HungryNekos[saveState.HungryNekoLocation].GetHungry(saveState.HungryNekoLevel);
        }
        
        // set text language
        localizer.SetLanguage(saveState.Language);
        
        // misc initialization tasks
        ScoreDisplayObj.Snap();
        swipeDir = BoardDirection.Null;
        allowInput = true;
        audioManager.SetAudioEnabled(saveState.AudioEnabled);
        
        HighScore = PlayerPrefs.GetInt(playerPrefHighScoreKey);
        CurrentHighScoreIndicator.SetActive(true);
        CurrentHighScoreLabel.text = HighScore.ToString();

        // Debug.Log(
        //     $"Game loaded. Total games started: {PlayerPrefs.GetInt(playerPrefsGameCountKey)}, " +
        //     $"Premium status: {PremiumControllerObj.IsGamePremium}, " +
        //     $"Language: {localizer.CurrentLanguage}"
        // );
        
        // StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/log.txt", true);
        // writer.WriteLine(
        //     $"Game loaded. Total games started: {PlayerPrefs.GetInt(playerPrefsGameCountKey)}, " +
        //     $"Premium status: {PremiumControllerObj.IsGamePremium}, " +
        //     $"Language: {localizer.CurrentLanguage}");
        // writer.Close();
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void CreateFirstBlock() {
        // create the first block in the middle of the board
        CreateBlock(CenterHex, BlockKind.Normal, 1, true);
        
        // show the swipe tutorial
        SwipeTutorial.gameObject.SetActive(true);
        SwipeTutorial.transform.position = CenterHex.transform.position;
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void CreateSecondBlock() {
        // we assume the block is at one of the six corners - create a block exactly 1 corners away
        // in a random direction so the player has to make one swipe to combine them
        int corner1 = OrderedCornerHexes.IndexOf(GetBlockHex(blocks[0]));
        int offset = Random.Range(0, 2) == 0 ? 1 : -1;
        int corner2 = (corner1 + offset + OrderedCornerHexes.Count) % OrderedCornerHexes.Count;
        CreateBlock(OrderedCornerHexes[corner2], BlockKind.Normal, 1, true);
        
        // show the combine tutorial
        CombineTutorial.gameObject.SetActive(true);
        CombineTutorial.transform.position = CenterHex.transform.position;
        
        // move and rotate the combine tutorial's arrow depending on block positions
        Vector3 arrowPosition = OrderedCornerHexes[corner1].transform.position;
        CombineTutorial tutorial = CombineTutorial.GetComponent<CombineTutorial>();
        tutorial.Arrow1.transform.position = arrowPosition;
        
        // I don't want to talk about this
        float angle = 0;
        switch (corner1) {
            case 0: angle = offset > 0 ?  30 : 270; break;
            case 1: angle = offset > 0 ? 330 : 210; break;
            case 2: angle = offset > 0 ? 270 : 150; break;
            case 3: angle = offset > 0 ? 210 :  90; break;
            case 4: angle = offset > 0 ? 150 :  30; break;
            case 5: angle = offset > 0 ?  90 : 330; break;
        }
        tutorial.Arrow1.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
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
    private void MakeRandomNekoHungry() {
        // what is the highest (non-maxed) level sushi on the board?
        int highestLevel = 0;
        foreach (Block sushi in blocks) {
            if (sushi.Level < ImageForBlockProgression.Count && sushi.Level > highestLevel) {
                highestLevel = sushi.Level;
            }
        }

        int randNeko = Random.Range(0, HungryNekos.Count);
        int nekoLevel = Random.Range(highestLevel / 2, highestLevel + 1);
        HungryNekos[randNeko].GetHungry(nekoLevel);
        MovesSinceLastHungryNeko = 0;
    }
    
    //--------------------------------------------------------------------------------------------------------
    private bool IsAnyNekoHungry() {
        return HungryNekos.Any(neko => neko.IsHungry);
    }
    
    //--------------------------------------------------------------------------------------------------------
    private Hex GetBlockHex(Block block) {
        return hexes.FirstOrDefault(hex => hex.Occupant == block);
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void ClearTutorials() {
        SwipeTutorial.gameObject.SetActive(false);
        CombineTutorial.gameObject.SetActive(false);
        WildCardTutorial.gameObject.SetActive(false);
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void ShowWildcardTutorial(Hex wildcardHex) {
        WildCardTutorial.gameObject.SetActive(true);
        WildCardTutorial.transform.position = CenterHex.transform.position;
        WildcardTutorial theActualTutorial = WildCardTutorial.GetComponent<WildcardTutorial>();
        theActualTutorial.Circle.transform.position = wildcardHex.transform.position;
    }
}
