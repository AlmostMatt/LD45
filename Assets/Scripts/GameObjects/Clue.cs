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
        Sprite relevantImage = SpriteManager.GetSprite("Item");

        PersonState player = GameState.Get().Player;
        DialogBlock discussion = new DialogBlock(new PersonState[] { GameState.Get().Player }, null);
        discussion.QueueDialogue(player, new Sprite[] { relevantImage }, "A clue!");
        discussion.QueueDialogue(player, new Sprite[] { relevantImage }, mInfo.mConceptA + "<->" + mInfo.mConceptB);
        discussion.Start();

        // add clue to "inventory"
        if (mInfo != null)
            PlayerInventory.AddClue(mInfo);

        Destroy(gameObject);
    }
}
