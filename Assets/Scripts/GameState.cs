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
    public Animator blackFade;

    public static GameState Get() { return GameObject.FindWithTag("GameRules").GetComponent<GameState>(); }

    private string[] clueRooms = { "Bedroom1", "Bedroom2", "Bedroom3" }; // todo: better way of specifying this? data-drive?
    Dictionary<string, List<ClueInfo>> mCluesInRooms = new Dictionary<string, List<ClueInfo>>();

    string[] mPersonRooms = new string[3]; // A list of room-names corresponding to the current location of each person
    string mCurrentRoom; // The room that is currently visible
    private string mPendingRoom;

    [HideInInspector]
    public int PlayerId;
    
    private Canvas mUICanvas;

    enum LoadState { NONE, UNLOADING_SCENE, LOADING_SCENE }
    private LoadState mLoadState = LoadState.NONE;
    private AsyncOperation mLoadSceneOperation;

    enum GameStage
    {
        MENU,
        INTRO,
        COMMUNAL_0,
        SEARCH_1,
        COMMUNAL_1,
        SEARCH_2,
        COMMUNAL_2,
        POLICE
    }
    private GameStage mCurrentStage;
    private ClueInfo mStartingClue;

    // Start is called before the first frame update
    void Start()
    {
        mCurrentStage = GameStage.MENU;

        List<ClueInfo> cluesToScatter;
        MysteryGenerator.Generate(out mStartingClue, out cluesToScatter);

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
            switch (mLoadState)
            {
                case LoadState.UNLOADING_SCENE:
                    {
                        LoadPendingRoom();
                        break;
                    }
                case LoadState.LOADING_SCENE:
                    {
                        OnRoomLoaded();
                        break;
                    }
            }
        }

        if(mCurrentStage == GameStage.INTRO)
        {    
            Debug.Log("Dead body. The name " + mStartingClue.mConceptB + " is written in blood by the body.");
            PlayerInteraction.Get().QueueDialogue("Where am I?");
            PlayerInteraction.Get().QueueDialogue("WHO am I?");
            PlayerInteraction.Get().QueueDialogue("Look! A body!");
            PlayerInteraction.Get().QueueDialogue("And a name: " + mStartingClue.mConceptB);
            PlayerInteraction.Get().ContinueDialogue();
            mCurrentStage = GameStage.COMMUNAL_0;
        }
    }
    
    public void MoveToRoom(int personId, string scene)
    {
        mPersonRooms[personId] = scene; // maybe unnecessary, idk

        if(personId == PlayerId)
        {
            if (mPendingRoom != null) { return; }

            mPendingRoom = scene;
            blackFade.SetTrigger("FadeOut");
        }
    }

    public void OnFadeOutComplete()
    {
        if (mCurrentRoom != null)
        {
            mLoadSceneOperation = SceneManager.UnloadSceneAsync(mCurrentRoom);
            mLoadState = LoadState.UNLOADING_SCENE;
            return;
        }

        LoadPendingRoom();
    }

    private void LoadPendingRoom()
    {
        mLoadState = LoadState.LOADING_SCENE;

        // if we want to keep the game rules object around (and UI too?) then we load scenes additively
        mLoadSceneOperation = SceneManager.LoadSceneAsync(mPendingRoom, LoadSceneMode.Additive);
    }

    public void OnRoomLoaded()
    {
        blackFade.ResetTrigger("FadeOut");
        blackFade.SetTrigger("FadeIn");
        
        mCurrentRoom = mPendingRoom;

        mLoadSceneOperation = null;
        mLoadState = LoadState.NONE;
        mPendingRoom = null;

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

        // is there some better way to defer the stage change until after the room is loaded?
        // this is a special case for the beginning of the game
        if (mCurrentStage == GameStage.MENU) { mCurrentStage = GameStage.INTRO; }
        
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
