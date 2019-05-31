﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
#pragma warning disable CS0618 // Type or member is obsolete
public abstract class PuzzleObj : NetworkBehaviour
{
    public Requires[] puzzleRequirements;
    [Server]
    public abstract void GenerateAsProp(long propSeed);
    [Server]
    public abstract void GenerateAsPuzzle(long puzzleSeed);
    [Server]
    public abstract void GenerateAsProp(PuzzleObj po, PuzzleType pt); // Literally when a puzzle uses this as a hint or 


    [SyncVar]
    public string state = "";

    [ClientRpc]
    public abstract void RpcGeneratePropWithState(string state);
}

#pragma warning restore CS0618 // Type or member is obsolete


public class Requires
{
    public bool isItem = false; // If the required object is an item to pick up, assumed it is a "prop" otherwise
    public PuzzleType pt; // The type of puzzle/hint that you are required to spawn to solve this puzzle.
    public GameObject[] possibleHints; // A list of hint objects that I am allowed to spawn....need to pick on from the list
    public GameObject specific_prop; // A prop that is REQUIRED (note only used if PT is Specific_prop)
}

public enum PuzzleType
{
    HINT, KEY, THREECOMBO, FOURCOMBO, COLORCODE, SPECIFIC_PROP, PUZZLE
}
