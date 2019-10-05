using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * Contains functions for checking or modifying the state of the game
 */
public class GameState : MonoBehaviour
{
    public string openingScene;
    public GameObject cluePrefab; // TEMP. Will probably have full Clues instead of just ClueInfo from the generator (Clue being sprite + info)
    
    public static GameState Get() { return GameObject.FindWithTag("GameRules").GetComponent<GameState>(); }

    private string[] clueRooms = { "Bedroom1", "Bedroom2", "Bedroom3" }; // todo: better way of specifying this? data-drive?
    Dictionary<string, List<ClueInfo>> mCluesInRooms = new Dictionary<string, List<ClueInfo>>();

    string[] mPersonRooms = new string[3]; // A list of room-names corresponding to the current location of each person
    string mCurrentRoom; // The room that is currently visible

    [HideInInspector]
    public int PlayerId;
    
    private Canvas mUICanvas;

    private AsyncOperation mLoadSceneOperation;

    // Start is called before the first frame update
    void Start()
    {
        ClueInfo startingClue;
        List<ClueInfo> cluesToScatter;
        MysteryGenerator.Generate(out startingClue, out cluesToScatter);

        Debug.Log("Dead body. The name " + startingClue.mConceptB + " is written in blood by the body.");

        // scatter clues
        foreach(ClueInfo clue in cluesToScatter)
        {
            int room = (int)Random.Range(0, clueRooms.Length);
            string roomName = clueRooms[room];
            if(!mCluesInRooms.ContainsKey(roomName))
            {
                mCluesInRooms.Add(roomName, new List<ClueInfo>());
            }

            mCluesInRooms[roomName].Add(clue);
        }


        PlayerId = 0; // todo: randomize?

        mPersonRooms[0] = openingScene;
        mPersonRooms[1] = "Bedroom1"; // TESTING
        mPersonRooms[2] = "Bedroom2"; // TESTING

        PlayerInteraction.Get().GoToRoom(openingScene);
    }


    // Update is called once per frame
    void Update()
    {
        if(mLoadSceneOperation != null && mLoadSceneOperation.isDone)
        {
            OnRoomLoaded();
        }
    }

    public void MoveToRoom(int personId, string scene)
    {
        mPersonRooms[personId] = scene; // maybe unnecessary, idk

        if(mCurrentRoom != null)
            { SceneManager.UnloadScene(mCurrentRoom); } // apparently obsolete, and we should use the async version (TODO)
        
        mCurrentRoom = scene;

        // if we want to keep the game rules object around (and UI too?) then we load scenes additively
        mLoadSceneOperation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
    }

    public void OnRoomLoaded()
    {
        mLoadSceneOperation = null;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(mCurrentRoom)); // ensures instantiate objects are added to the current room's scene (so they'll be destroyed when leaving)

        // populate room with clues
        if (mCluesInRooms.ContainsKey(mCurrentRoom))
        {
            // Debug.Log("You find these clues in the room:");
            int clueX = -2;
            foreach (ClueInfo c in mCluesInRooms[mCurrentRoom])
            {
                // Debug.Log(c.mConceptA + " <-> " + c.mConceptB);

                // TODO: markup the scene with valid spawn points for clues (maybe further filtered by type of clue)
                // for now, spawn them wherever
                GameObject clueObj = GameObject.Instantiate(cluePrefab);
                clueObj.GetComponent<Clue>().mInfo = c;
                Vector3 pos = new Vector3(clueX, 0, 0);
                clueObj.transform.position = pos;
                clueX += 1;
            }
        }
    }

    public bool IsPersonInCurrentRoom(int personId)
    {
        // check if we should display the given person.
        // special case is that because we're first person,
        // we never show ourselves (maybe we don't even need to update the player's location?)
        
        return (personId != PlayerId && mPersonRooms[personId].Equals(mCurrentRoom));
    }

    private void GenerateSetup()
    {

    }

    private void GetRoomChoice(PersonObject p)
    {
        
    }
}
