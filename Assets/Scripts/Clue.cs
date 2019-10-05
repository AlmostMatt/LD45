using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clue : MonoBehaviour
{
    public ClueInfo mInfo;
    
    void OnMouseDown()
    {
        // TODO - call a function in GameInteraction that checks if (UIHasOverlay()) { return; }
        Debug.Log("Found a clue!");

        if (mInfo != null)
            Debug.Log(mInfo.mConceptA + " <-> " + mInfo.mConceptB);

        // just want to open a dialogue box
        // param is unused by function
        PlayerInteraction.Get().StartDialog(0);

        // add clue to "inventory"
        if (mInfo != null)
            PlayerInventory.AddClue(mInfo);

        Destroy(gameObject);
    }
}
