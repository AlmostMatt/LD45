using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clue : MonoBehaviour
{
    ClueInfo mInfo = new ClueInfo(); // how do we fill this out?

    void OnMouseDown()
    {
        Debug.Log("Clicked on clue!");
        
        // add clue to "inventory"
        PlayerInventory.AddClue(mInfo);

        Destroy(gameObject);
    }
}
