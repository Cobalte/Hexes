﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveState {
    
    public List<int> BlockLocations = new List<int>();
    public List<BlockKind> BlockKinds = new List<BlockKind>();
    public List<int> BlockLevels = new List<int>();
    public bool IsGameOver;
    public int Score;
    public int Multiplier;
    public int HungryNekoLocation;
    public int HungryNekoLevel;
    public int HungryNekoCount;
    public int TurnCount;
    public float CurrentWildChance;
    public float CurrentDoubleChance;
    public float CurrentTripleChance;
    public int CurrentHungryNekoInterval;
    public int MovesSinceLastHungryNeko;
    public LocalizedLanguage Language;
    public bool AudioEnabled;
}
