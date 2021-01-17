using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Data", menuName = "LevelReward", order = 1)]
public class LevelReward : ScriptableObject {

    public int Experience;
    public LevelRewardType Type;
    public float Value;
    public Sprite Icon;

}
