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
        Debug.Log(mInfo.mConceptA + " <-> " + mInfo.mConceptB);

        // just want to open a dialogue box
        // param is unused by function
        PlayerInteraction.Get().QueueDialogue("A clue!");
        PlayerInteraction.Get().QueueDialogue(mInfo.mConceptA + "<->" + mInfo.mConceptB);
        PlayerInteraction.Get().ContinueDialogue();

        // add clue to "inventory"
        if (mInfo != null)
            PlayerInventory.AddClue(mInfo);

        Destroy(gameObject);
    }
}
