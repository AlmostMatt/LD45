﻿using System.Collections;
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

    PersonState[] mPeople;
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
        
        PlayerId = 0; // todo: randomize?
        List<ClueInfo> cluesToScatter;
        MysteryGenerator.Generate(out mPeople, out mStartingClue, out cluesToScatter);
        
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

        mPersonRooms[0] = openingScene;
        mPersonRooms[1] = openingScene;
        mPersonRooms[2] = openingScene;
        
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

        // TEMP to advance time
        if(mCurrentStage == GameStage.SEARCH_1 && Input.GetButton("Submit"))
        {
            StartStage(mCurrentStage + 1);
        }
    }
    
    public void OnDialogueDismissed(int btnIdx)
    {
        StartStage(mCurrentStage + 1);
    }

    private void StartStage(GameStage stage)
    {
        Debug.Log("starting stage " + stage);
        mCurrentStage = stage;
        
        if(stage == GameStage.SEARCH_1)
        {
            // assign npcs to rooms (for now, ensure they go to different rooms)
            int[] roomChoices = Utilities.RandomList(clueRooms.Length, 2);
            for(int i = 0, j = 0; i < 3; ++i)
            {
                if(i != PlayerId)
                {
                    MoveToRoom(i, clueRooms[roomChoices[j++]]);
                }
            }

            MoveToRoom(PlayerId, mCurrentRoom); // hack to reload room with npcs gone
        }
        else if(stage == GameStage.COMMUNAL_1)
        {
            for (int i = 0; i < 3; ++i)
            {
                MoveToRoom(i, openingScene);
            }
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

        mCurrentRoom = mPendingRoom; // do this now, because the objects in the room Start during the load (i.e. before we get to OnRoomLoaded)

        // if we want to keep the game rules object around (and UI too?) then we load scenes additively
        mLoadSceneOperation = SceneManager.LoadSceneAsync(mPendingRoom, LoadSceneMode.Additive);
    }

    public void OnRoomLoaded()
    {
        blackFade.ResetTrigger("FadeOut");
        blackFade.SetTrigger("FadeIn");
        
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


        // maybe this is cleaner in its own function, like ContinueGameStage or whatever, idk
        Debug.Log(mCurrentRoom + " loaded. Current stage: " + mCurrentStage);
        if (mCurrentStage == GameStage.MENU)
        {
            mCurrentStage = GameStage.INTRO;

            Debug.Log("Dead body. The name " + mStartingClue.mConceptB + " is written in blood by the body.");
            PlayerInteraction.Get().QueueDialogue("Where am I?");
            PlayerInteraction.Get().QueueDialogue("WHO am I?");
            PlayerInteraction.Get().QueueDialogue("Look! A body!");
            PlayerInteraction.Get().QueueDialogue("And a name: " + mStartingClue.mConceptB);
            PlayerInteraction.Get().QueueDialogue("Let's split up and look for clues.");
            PlayerInteraction.Get().OpenDialogue(OnDialogueDismissed);
        }
        else if(mCurrentStage == GameStage.COMMUNAL_1)
        {
            PlayerInteraction.Get().QueueDialogue("What did everyone find?");

            PlayerInteraction.Get().QueueDialogue("I went to " + " BEDROOM 1 " + " and found " + " CLUE");
            PlayerInteraction.Get().QueueDialogue("I went to " + " BEDROOM 2 " + " and found " + " CLUE");
            PlayerInteraction.Get().OpenDialogue(OnDialogueDismissed);
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