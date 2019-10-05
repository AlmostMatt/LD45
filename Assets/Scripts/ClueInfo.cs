using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// I don't know what data goes in here yet, but this is what you get when you pick up a clue
public class ClueInfo
{
    public ClueInfo(Noun conceptA, Noun conceptB)
    {
        mConceptA = conceptA;
        mConceptB = conceptB;
    }

    public Noun mConceptA;
    public Noun mConceptB;

    public string mName;
    public string mFlavourString;
}
