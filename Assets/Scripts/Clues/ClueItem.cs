using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClueItem
{
    public ClueInfo info;
    public string spriteName;
    public string description;

    public ClueItem(Noun n1, Noun n2, string sprite, string desc)
    {
        info = new ClueInfo(n1, n2);
        description = desc;
        spriteName = sprite;
    }
}
