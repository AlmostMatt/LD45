using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackstoryClue : ClueGenerator
{
    public override bool MatchTypes(NounType t1, NounType t2)
    {
        // can do identity or name <-> backstory or motive
        return (
               CheckCommutative(t1, t2, NounType.Identity, NounType.Backstory)
            || CheckCommutative(t1, t2, NounType.Identity, NounType.Motive)
            || CheckCommutative(t1, t2, NounType.Name, NounType.Backstory)
            || CheckCommutative(t1, t2, NounType.Name, NounType.Motive)
        );
    }

    public override ClueItem GetItem(Noun n1, Noun n2)
    {
        string sprite = "Newspaper";
        string desc = "";
        if (n1.Type() == NounType.Identity)
        {
            desc = "A newspaper article about " + n1.AsSubject() + ", who " + n2.AsObject() + ".";
        }
        else if (n2.Type() == NounType.Identity)
        {
            desc = "A newspaper article about " + n2.AsSubject() + ", who " + n1.AsObject() + ".";
        }
        else if (n1.Type() == NounType.Name)
        {
            desc = "A newspaper article about " + n1.AsSubject() + ", who " + n2.AsObject() + ".";
        }
        else if (n2.Type() == NounType.Name)
        {
            desc = "A newspaper article about " + n2.AsSubject() + ", who " + n1.AsObject() + ".";
        }

        ClueItem item = new ClueItem(n1, n2, sprite, desc);
        return item;
    }

}
