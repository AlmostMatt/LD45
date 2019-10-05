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

    private ClueInfo[] mRoundClues = new ClueInfo[3]; // the clue that each person found that round
    PersonState[] mPeople;
    private Sprite[] mNonPlayerHeads;
    public Sprite[] NonPlayersHeads
    {
        get
        {
            if (mNonPlayerHeads == null)
            {
                Sprite[] otherSprites = new Sprite[2];
                int j = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (!mPeople[i].IsPlayer)
                    {
                        otherSprites[j] = mPeople[i].HeadSprite;
                        j++;
                    }
                }
                mNonPlayerHeads = otherSprites;
            }
            return mNonPlayerHeads;
        }
    }


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
        POLICE,
        REVEAL
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

        // give knowledge
        for(int i = 0; i < 3; ++i)
        {
            mPeople[i].knowledge.AddKnowledge(mStartingClue.GetSentence());
        }

        // scatter clues
        foreach (ClueInfo clue in cluesToScatter)
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
        if(Input.GetButton("Submit"))
        {
            if(
                    mCurrentStage == GameStage.SEARCH_1
                ||  mCurrentStage == GameStage.SEARCH_2
            )
            {
                StartStage(mCurrentStage + 1);
            }
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
        
        if(stage == GameStage.SEARCH_1 || stage == GameStage.SEARCH_2)
        {
            // assign npcs to rooms (for now, ensure they go to different rooms)
            int[] roomChoices = Utilities.RandomList(clueRooms.Length, 2);
            for(int i = 0, j = 0; i < 3; ++i)
            {
                if(i != PlayerId)
                {
                    string npcRoom = clueRooms[roomChoices[j++]];
                    MoveToRoom(i, npcRoom);

                    // npcs pick up a clue in the room they move to
                    if(mCluesInRooms.ContainsKey(npcRoom))
                    {
                        List<ClueInfo> cluesInRoom = mCluesInRooms[npcRoom];
                        if(cluesInRoom.Count > 0)
                        {
                            int clueIdx = Random.Range(0, cluesInRoom.Count);
                            ClueInfo info = cluesInRoom[clueIdx];
                            cluesInRoom.RemoveAt(clueIdx);
                            
                            mPeople[i].knowledge.AddKnowledge(info.GetSentence());
                            mRoundClues[i] = info;
                        }
                    }
                }
            }

            MoveToRoom(PlayerId, mCurrentRoom); // hack to reload room with npcs gone
        }
        else if(stage == GameStage.COMMUNAL_1 || stage == GameStage.COMMUNAL_2)
        {
            for (int i = 0; i < 3; ++i)
            {
                MoveToRoom(i, openingScene);
            }
        }
        else if(stage == GameStage.POLICE)
        {
            MoveToRoom(PlayerId, "EndScene");
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
            PlayerInteraction.Get().QueueDialogue(new Sprite[] { }, "Where am I?");
            PlayerInteraction.Get().QueueDialogue(new Sprite[] { }, "WHO am I?");
            PlayerInteraction.Get().QueueDialogue(new Sprite[] { SpriteManager.GetSprite("Victim") }, "Look! A body!");
            PlayerInteraction.Get().QueueDialogue(new Sprite[] { SpriteManager.GetSprite("CrimeScene") }, "And a name: " + mStartingClue.mConceptB);
            PlayerInteraction.Get().QueueDialogue(NonPlayersHeads, "And two more people.");
            PlayerInteraction.Get().QueueDialogue(NonPlayersHeads, "Let's split up and look for clues.");
            PlayerInteraction.Get().OpenDialogue(OnDialogueDismissed);
        }
        else if (mCurrentStage == GameStage.COMMUNAL_1)
        {
            ShareInformation();

            PlayerInteraction.Get().QueueDialogue(NonPlayersHeads, "There must be more clues around.");
            PlayerInteraction.Get().OpenDialogue(OnDialogueDismissed);
        }
        else if (mCurrentStage == GameStage.COMMUNAL_2)
        {
            ShareInformation();
            
            PlayerInteraction.Get().QueueDialogue(NonPlayersHeads, "Well, the police are here now.");
            PlayerInteraction.Get().OpenDialogue(OnDialogueDismissed);
        }
        else if(mCurrentStage == GameStage.POLICE)
        {
            PlayerInteraction.Get().QueueDialogue(new Sprite[] { }, "What happened? Which one of you killed the guy?");

            // get npc evaluations
            Sentence killer0 = new Sentence(Noun.Blonde, Verb.Is, Noun.Killer, Adverb.True);
            Sentence killer1 = new Sentence(Noun.Brown, Verb.Is, Noun.Killer, Adverb.True);
            Sentence killer2 = new Sentence(Noun.Red, Verb.Is, Noun.Killer, Adverb.True);
            for (int i = 0; i < 3; ++i)
            {
                if(i != PlayerId)
                {
                    Knowledge personKnowledge = mPeople[i].knowledge;
                    float confidence0 = personKnowledge.VerifySentence(killer0);
                    float confidence1 = personKnowledge.VerifySentence(killer1);
                    float confidence2 = personKnowledge.VerifySentence(killer2);
                    if(confidence0 > 0)
                        PlayerInteraction.Get().QueueDialogue(new Sprite[] { mPeople[i].HeadSprite }, "I think BLONDE did it");
                    else if(confidence1 > 0)
                        PlayerInteraction.Get().QueueDialogue(new Sprite[] { mPeople[i].HeadSprite }, "I think BROWN did it");
                    else if (confidence2 > 0)
                        PlayerInteraction.Get().QueueDialogue(new Sprite[] { mPeople[i].HeadSprite }, "I think RED did it");
                    else
                        PlayerInteraction.Get().QueueDialogue(new Sprite[] { mPeople[i].HeadSprite }, "I don't know.");
                }
            }

            PlayerInteraction.Get().OpenDialogue(OnDialogueDismissed);
        }
    }

    public PersonState GetPerson(int personId)
    {
        return mPeople[personId];
    }

    public bool IsPersonInCurrentRoom(int personId)
    {
        // check if we should display the given person.
        // special case is that because we're first person,
        // we never show ourselves (maybe we don't even need to update the player's location?)
        
        return (personId != PlayerId && mPersonRooms[personId].Equals(mCurrentRoom));
    }

    private void ShareInformation()
    {
        PlayerInteraction.Get().QueueDialogue(NonPlayersHeads, "What did everyone find?");

        List<Sentence> commonRevealed = new List<Sentence>();
        for (int i = 0; i < 3; ++i)
        {
            if (!mPeople[i].IsPlayer)
            {
                if(mRoundClues[i] != null)
                {
                    Sentence newInfo = mRoundClues[i].GetSentence();
                    PlayerInteraction.Get().QueueDialogue(new Sprite[] { mPeople[i].HeadSprite }, "I found " + newInfo);
                    commonRevealed.Add(newInfo); // TODO: track speaker

                    mRoundClues[i] = null;
                }
                else
                {
                    PlayerInteraction.Get().QueueDialogue(new Sprite[] { mPeople[i].HeadSprite }, "I found nothing");
                }
            }
        }

        foreach (Sentence s in commonRevealed)
        {
            foreach (PersonState p in mPeople)
            {
                p.knowledge.AddKnowledge(s);
            }
        }
    }


    private void GetRoomChoice(PersonObject p)
    {
        
    }
}
