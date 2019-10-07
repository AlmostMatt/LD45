using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameIdentityClue : ClueGenerator
{
    public override bool MatchTypes(NounType t1, NounType t2)
    {
        return (t1 == NounType.Name && t2 == NounType.Identity) || (t1 == NounType.Identity && t2 == NounType.Name);
    }
   
    public override ClueItem GetItem(Noun n1, Noun n2)
    {
        // ensure n1 is the identity
        if (n1.Type() == NounType.Name)
        {
            Noun temp = n1;
            n1 = n2;
            n2 = temp;
        }

        string spriteName = "Letter";
        string description = "A letter to the victim about " + Utilities.bold(n2.ToString()) + ", " + n1.WithVictim() + ".";
        ClueItem item = new ClueItem(n1, n2, spriteName, description);
        return (item);
    }
}
