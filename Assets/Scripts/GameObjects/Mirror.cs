using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mirror : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {
        PersonState player = GameState.Get().Player;
        player.KnowsOwnFace = true;
        DialogBlock dialog = new DialogBlock(new PersonState[] { player });
        dialog.QueueDialogue(player, new Sprite[] { player.HeadSprite }, "So this is me...");
        dialog.Start();
    }
}
