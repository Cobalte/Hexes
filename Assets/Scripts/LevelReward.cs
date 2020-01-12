using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "LevelReward", order = 1)]
public class LevelReward : ScriptableObject {

    public int Experience;
    public LevelRewardType Type;
    public float Chance;
    public Sprite Icon;

}
