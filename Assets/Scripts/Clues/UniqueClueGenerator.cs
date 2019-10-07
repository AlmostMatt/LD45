using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniqueClueGenerator : ClueGenerator
{
    public override bool MatchTypes(NounType t1, NounType t2)
    {
        return t1 == NounType.Unique || t2 == NounType.Unique;
    }

    public override ClueItem GetItem(Noun n1, Noun n2)
    {
        string sprite = "";
        string desc = "";
        if (n1 == Noun.Potion || n2 == Noun.Potion)
        {
            sprite = "Potion";
            desc = "This is an airborne drug that causes temporary memory loss.";
        }
        // TODO: newspaper article about the victim
        // this could also be a backstory clue

        ClueItem item = new ClueItem(n1, n2, Verb.Has, sprite, desc);
        return item;
    }
}
