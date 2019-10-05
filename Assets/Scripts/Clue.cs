using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clue : MonoBehaviour
{
    ClueInfo mInfo;

    public Clue(Noun thingA, Noun thingB)
    {
        mInfo = new ClueInfo(thingA, thingB);
    }

    void OnMouseDown()
    {
        Debug.Log("Clicked on clue!");
        
        // add clue to "inventory"
        PlayerInventory.AddClue(mInfo);

        Destroy(gameObject);
    }
}
