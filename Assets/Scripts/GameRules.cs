using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRules : MonoBehaviour
{
    public string openingScene;
    
    public static GameRules Get() { return GameObject.FindWithTag("GameRules").GetComponent<GameRules>(); }

    string[] mPersonRooms = new string[3]; // A list of room-names corresponding to the current location of each person
    string mCurrentRoom; // The room that is currently visible

    int mPlayerId;

    private Canvas mUICanvas;

    // Start is called before the first frame update
    void Start()
    {
        mUICanvas = GameObject.FindWithTag("UICanvas").GetComponent<Canvas>();
        mPlayerId = 0; // todo: randomize?

        mPersonRooms[0] = openingScene;
        mPersonRooms[1] = "Bedroom1"; // TESTING
        mPersonRooms[2] = "Bedroom2"; // TESTING

        GoToRoom(openingScene);
    }

    public void StartDialog(int PersonId)
    {
        if (UIHasOverlay()) { return; }
        mUICanvas.transform.Find("dialogView").gameObject.SetActive(true);
    }

    public void GoToRoom(string scene)
    {
        if (UIHasOverlay()) { return; }
        mPersonRooms[mPlayerId] = scene; // maybe unnecessary, idk

        if(mCurrentRoom != null)
            { SceneManager.UnloadScene(mCurrentRoom); } // apparently obsolete, and we should use the async version (TODO)
        
        mCurrentRoom = scene;

        // if we want to keep the game rules object around (and UI too?) then we load scenes additively
        SceneManager.LoadScene(scene, LoadSceneMode.Additive);
    }

    public bool IsPersonInCurrentRoom(int personId)
    {
        // check if we should display the given person.
        // special case is that because we're first person,
        // we never show ourselves (maybe we don't even need to update the player's location?)
        
        return (personId != mPlayerId && mPersonRooms[personId].Equals(mCurrentRoom));
    }

    private void GetRoomChoice(PersonObject p)
    {

    }

    // Update is called once per frame
    void Update()
    {
        // scripted flow for where each npc goes
        // GetRoomChoice(mNpc1);
        // GetRoomChoice(mNpc2);
    }

    private bool UIHasOverlay()
    {
        return mUICanvas.transform.Find("dialogView").gameObject.activeInHierarchy;
    }
}
