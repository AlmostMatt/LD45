using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    public string scene;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseDown()
    {
        Debug.Log("yo");
        PlayerInteraction.Get().GoToRoom(scene);

        // load players in room?
        // the main player is always there.
        // sometimes an npc will be in there too.
        
        // query game state to see if a person is in this room?
        // need to know room id somehow
    }

}
