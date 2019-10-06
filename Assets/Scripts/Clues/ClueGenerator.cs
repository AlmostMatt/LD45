using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClueGenerator : MonoBehaviour
{
    public virtual bool MatchTypes(NounType t1, NounType t2) { return false;  }
    public virtual ClueItem GetItem(Noun n1, Noun n2) { return null; }
}
