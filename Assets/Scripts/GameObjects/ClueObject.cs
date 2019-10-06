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

        // just want to open a dialogue box
        // param is unused by function
        Sprite relevantImage = SpriteManager.GetSprite(mItem.spriteName);

        PersonState player = GameState.Get().Player;
        DialogBlock discussion = new DialogBlock(new PersonState[] { GameState.Get().Player }, null);        
        discussion.QueueDialogue(player, new Sprite[] { relevantImage }, mItem.description);
        discussion.Start();

        // add clue to "inventory"
        if (mItem.info != null)
            PlayerJournal.AddClue(mItem.info);

        Destroy(gameObject);
        // TODO: remove from list in gamestate
    }
}
