using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClueGenerator : MonoBehaviour
{
    public virtual bool MatchTypes(NounType t1, NounType t2) { return false; }
    public virtual ClueItem GetItem(Noun n1, Noun n2) { return null; }

    protected bool CheckCommutative(NounType a1, NounType a2, NounType b1, NounType b2)
    {
        return (a1 == b1 && a2 == b2) || (a1 == b2 && a2 == b1);
    }
}
