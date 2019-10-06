using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClueObject : MonoBehaviour
{
    public ClueItem mItem;
    
    void OnMouseDown()
    {
        // TODO - call a function in GameInteraction that checks if (UIHasOverlay()) { return; }
        Debug.Log("Found a clue!");
        Debug.Log(mItem.info.nounA + " <-> " + mItem.info.nounB);

        GameState.Get().PlayerFoundClue(this);        
    }
}
