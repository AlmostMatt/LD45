using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerInventory
{
    static List<ClueInfo> sClues = new List<ClueInfo>();

    public static void AddClue(ClueInfo c)
    {
        sClues.Add(c);
    }
}
