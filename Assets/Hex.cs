using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour {
    
    public List<BoardDirection> NeighborDirections;
    public List<Hex> Neighbors;
    public Block Occupant;

    public int CurrentLevel => Occupant == null ? 0 : Occupant.Level;

}