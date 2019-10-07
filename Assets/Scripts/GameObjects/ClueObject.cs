using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClueObject : MonoBehaviour
{
    public ClueItem mItem;
    
    void OnMouseDown()
    {
        if (!PlayerInteraction.Get().CanInteractWithScene()) return;
        GameState.Get().PlayerFoundClue(this);
    }
}
