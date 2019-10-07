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
    Dictionary<string, List<ClueItem>> mCluesInRooms = new Dictionary<string, List<ClueItem>>();

    private static int MAX_CLUES_PER_ROUND = 2;
    private int mCluesFoundThisRound = 0;
    public ClueInfo[] mRoundClues = new ClueInfo[3]; // the clue that each person found that round
    public PersonState[] mPeople;
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
    public int KillerId; // Populated by MysteryGenerator
    [HideInInspector]
    public int PlayerId = 0;
    public PersonState Player
    {
        get { return mPeople[PlayerId]; }
    }
    [HideInInspector]
    public int VictimId = 3;
    public PersonState Victim = new PersonState(3);
    public PersonState Police = new PersonState(4);

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
        SEARCH_3,
        COMMUNAL_3,
        POLICE,
        REVEAL
    }
    private GameStage mCurrentStage;
    private ClueInfo mStartingClue;

    // Start is called before the first frame update
    void Start()
    {
        UIController.Get().HideUI();
        mCurrentStage = GameStage.MENU;
        
        PlayerId = 0; // todo: randomize?
        Victim.AttributeMap[NounType.Name] = Noun.Victor;
        Victim.AttributeMap[NounType.HairColor] = Noun.Victim; // This is relevant for spritemanager lookup of his head
        List<ClueItem> cluesToScatter;
        MysteryGenerator.Generate(out mPeople, out mStartingClue, out cluesToScatter);

        // Give everyone knowledge of the name of the killer
        PlayerJournal.AddListen(VictimId, mStartingClue.GetSentence());
        for (int i = 0; i < 3; ++i)
        {
            mPeople[i].knowledge.AddKnowledge(mStartingClue.GetSentence());
        }

        // scatter clues evenly across rooms
        int[] clueShuffle = Utilities.RandomList(cluesToScatter.Count, cluesToScatter.Count);
        int roomIdx = 0;
        for(int i = 0; i < cluesToScatter.Count; ++i)
        {
            string roomName = clueRooms[roomIdx];
            if (!mCluesInRooms.ContainsKey(roomName))
            {
                mCluesInRooms.Add(roomName, new List<ClueItem>());
            }

            mCluesInRooms[roomName].Add(cluesToScatter[clueShuffle[i]]);
            roomIdx = (roomIdx + 1) % clueRooms.Length;
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
        if(Input.GetButton("Submit")) // TODO - remove before launch. Otherwise people will click this when using Submit to continue dialog.
        {
            if(
                    mCurrentStage == GameStage.SEARCH_1
                ||  mCurrentStage == GameStage.SEARCH_2
                ||  mCurrentStage == GameStage.SEARCH_3
            )
            {
                StartStage(mCurrentStage + 1);
            }
        }

        // TEMP to dump journal
        if(Input.GetButton("Jump"))
        {
            List<PlayerJournal.SentenceHistory> sentences = PlayerJournal.GetJournal();
            foreach(PlayerJournal.SentenceHistory s in sentences)
            {
                Debug.Log(s);
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
        
        if(stage == GameStage.SEARCH_1 || stage == GameStage.SEARCH_2 || stage == GameStage.SEARCH_3)
        {
            UIController.Get().ShowJournalButton();            

            // assign npcs to rooms (for now, ensure they go to different rooms)
            int[] roomChoices = Utilities.RandomList(clueRooms.Length, 2);
            for(int i = 0, j = 0; i < 3; ++i)
            {
                mRoundClues[i] = null;

                if (i != PlayerId)
                {
                    string npcRoom = clueRooms[roomChoices[j++]];
                    MoveToRoom(i, npcRoom);

                    // npcs pick up a clue in the room they move to
                    if(mCluesInRooms.ContainsKey(npcRoom))
                    {
                        List<ClueItem> cluesInRoom = mCluesInRooms[npcRoom];
                        if(cluesInRoom.Count > 0)
                        {
                            int clueIdx = Random.Range(0, cluesInRoom.Count);
                            ClueItem clue = cluesInRoom[clueIdx];
                            cluesInRoom.RemoveAt(clueIdx);

                            ClueInfo info = clue.info;
                            mPeople[i].knowledge.AddKnowledge(info.GetSentence());
                            mRoundClues[i] = info;
                        }
                    }
                }
            }

            MoveToRoom(PlayerId, mCurrentRoom); // hack to reload room with npcs gone
        }
        else if(stage == GameStage.COMMUNAL_1 || stage == GameStage.COMMUNAL_2 || stage == GameStage.COMMUNAL_3)
        {
            for (int i = 0; i < 3; ++i)
            {
                MoveToRoom(i, openingScene);
            }
        }
        else if(stage == GameStage.POLICE)
        {
            MoveToRoom(PlayerId, "EndScene");
        } else if (stage == GameStage.REVEAL)
        {
            // Give me closure please!
            Debug.Log("The actual killer was " + mPeople[KillerId].AttributeMap[NounType.HairColor]);
            for (int j = 0; j < 3; j++)
            {
                Debug.Log(mPeople[j].AttributeMap[NounType.HairColor] + " is " + mPeople[j].AttributeMap[NounType.Name]);
                Debug.Log(mPeople[j].AttributeMap[NounType.HairColor] + " is " + mPeople[j].AttributeMap[NounType.Identity]);
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
            // TODO: I don't know what to do if there are more clues in this room than spawn points
            // maybe we should just make sure that never happens? maybe it's fine that certain clues never spawn?
            GameObject[] clueSpawns = GameObject.FindGameObjectsWithTag("ClueSpawn");
            int numClues = Mathf.Min(mCluesInRooms[mCurrentRoom].Count, clueSpawns.Length);

            List<ClueItem> clues = mCluesInRooms[mCurrentRoom];
            int[] clueSpots = Utilities.RandomList(clueSpawns.Length, numClues);
            for(int i = 0; i < numClues; ++i)
            {
                ClueItem item = clues[i];
                GameObject clueObj = GameObject.Instantiate(cluePrefab);
                clueObj.GetComponent<ClueObject>().mItem = item;
                clueObj.GetComponent<SpriteRenderer>().sprite = SpriteManager.GetSprite(item.spriteName);

                // check this: it's annoying as hell when adding objects to scenes for some reason defaults their z to be too close to the camera
                // *but* for spawn points, it's actually convenient to be able to see them in the editor, yet have them be hidden in-game.
                // so, leave the spawn points in their stupid z-position, and spawn clues at their x,y and a sane z-position.
                Vector3 spawnPos = clueSpawns[clueSpots[i]].transform.position;
                clueObj.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0);
            }
        }

        // maybe this is cleaner in its own function, like ContinueGameStage or whatever, idk
        Debug.Log(mCurrentRoom + " loaded. Current stage: " + mCurrentStage);
        if (mCurrentStage == GameStage.MENU)
        {
            mCurrentStage = GameStage.INTRO;

            DialogBlock discussion = new DialogBlock(mPeople, OnDialogueDismissed);
            discussion.QueueDialogue(mPeople[2], new Sprite[] { mPeople[2].HeadSprite }, "Ow...");
            discussion.QueueDialogue(mPeople[1], new Sprite[] { mPeople[1].HeadSprite }, "Ugh... where am I?", AudioClipIndex.HMM);
            discussion.QueueDialogue(mPeople[1], new Sprite[] { mPeople[1].HeadSprite }, "Who are you two?");
            discussion.QueueDialogue(Player, new Sprite[] { Player.HeadSprite }, "Me? I'm...", AudioClipIndex.HMM);
            discussion.QueueDialogue(mPeople[2], new Sprite[] { mPeople[2].HeadSprite }, "I don't know... I can't remember.");
            discussion.QueueDialogue(mPeople[1], new Sprite[] { SpriteManager.GetSprite("Victim") }, "Ahhh! A body!!", AudioClipIndex.SURPRISE);
            discussion.QueueDialogue(mPeople[1], new Sprite[] { SpriteManager.GetSprite("CrimeScene") }, "And there's a name written by it in blood: " + Utilities.bold(mStartingClue.nounB.ToString()) + "!");
            discussion.QueueDialogue(mPeople[2], new Sprite[] { mPeople[2].HeadSprite }, "Oh my gosh! Which one of you is " + mStartingClue.nounB + "?!", AudioClipIndex.OH);
            discussion.QueueDialogue(mPeople[1], new Sprite[] { mPeople[1].HeadSprite }, "Not me! I'm...");
            discussion.QueueDialogue(mPeople[1], new Sprite[] { mPeople[1].HeadSprite }, "I can't remember my name either!");
            discussion.QueueDialogue(mPeople[2], new Sprite[] { mPeople[2].HeadSprite }, "Oh sure! You're probably " + mStartingClue.nounB + ", and you killed this guy!", AudioClipIndex.PFFT);
            discussion.QueueDialogue(mPeople[1], new Sprite[] { mPeople[1].HeadSprite }, "Calm down, " + mPeople[2].AttributeMap[NounType.HairColor] + "!");
            discussion.QueueDialogue(mPeople[1], new Sprite[] { mPeople[1].HeadSprite }, "We don't know anything for sure.");
            discussion.QueueDialogue(mPeople[1], new Sprite[] { mPeople[1].HeadSprite }, "Let's look around and see if we can figure out what happened here.");
            discussion.QueueDialogue(mPeople[2], new Sprite[] { mPeople[2].HeadSprite }, "Ok, ok... sorry. We can search the place, but let's not stay separated for long.");
            discussion.QueueDialogue(Player, NonPlayersHeads, "Ok, let's meet back here soon.");
            discussion.Start();
        }
        else if (mCurrentStage == GameStage.COMMUNAL_1 || mCurrentStage == GameStage.COMMUNAL_2)
        {
            DialogBlock discussion = new DialogBlock(mPeople, OnDialogueDismissed);
            discussion.QueueDialogue(mPeople[1], NonPlayersHeads, "What did everyone find?");
            discussion.QueueInformationExchange();
            discussion.QueueDialogue(Player, NonPlayersHeads, "There must be more clues around.");
            discussion.Start();
        }
        else if (mCurrentStage == GameStage.COMMUNAL_3)
        {
            DialogBlock discussion = new DialogBlock(mPeople, OnDialogueDismissed);
            discussion.QueueDialogue(mPeople[1], NonPlayersHeads, "The police are almost here. Let's do a final round of information exchange.");
            discussion.QueueInformationExchange();
            discussion.QueueDialogue(mPeople[2], NonPlayersHeads, "Well, the police are here now.");
            discussion.Start();
        }
        else if(mCurrentStage == GameStage.POLICE)
        {
            DialogBlock discussion = new DialogBlock(mPeople, OnDialogueDismissed);
            discussion.QueueDialogue(Police, new Sprite[] { mPeople[0].HeadSprite, mPeople[1].HeadSprite, mPeople[2].HeadSprite }, "What happened here? How did that man die?");
            // TODO: ask the player for their opinion, either first or last

            // get npc evaluations

            // multiple beliefs contribute to an AI thinking that someone is the killer:
            // 1) did the victim write their name in blood?
            // 2) did the killer have a motive?

            Noun[] hairColors = new Noun[] {
                mPeople[0].AttributeMap[NounType.HairColor],
                mPeople[1].AttributeMap[NounType.HairColor],
                mPeople[2].AttributeMap[NounType.HairColor]
            };

            Sentence[] named = new Sentence[]
            {
                new Sentence(hairColors[0], Verb.Is, Noun.SuspectedName, Adverb.True),
                new Sentence(hairColors[1], Verb.Is, Noun.SuspectedName, Adverb.True),
                new Sentence(hairColors[2], Verb.Is, Noun.SuspectedName, Adverb.True),
            };

            Sentence[] motive = new Sentence[]
            {
                new Sentence(hairColors[0], Verb.Has, Noun.Motive, Adverb.True),
                new Sentence(hairColors[1], Verb.Has, Noun.Motive, Adverb.True),
                new Sentence(hairColors[2], Verb.Has, Noun.Motive, Adverb.True)
            };

            for (int i = 1; i < 3; ++i)
            {
                float[] namedScores = new float[3];
                float[] motiveScores = new float[3];
                float[] innocenceScores = new float[3];
                float[] killerScores = new float[3];

                PersonState p = mPeople[i];
                Knowledge personKnowledge = p.knowledge;
                Sprite[] sprite = { mPeople[i].HeadSprite };

                int bestSuspect = -1;
                float bestScore = 0f;
                for (int j = 0; j < 3; ++j)
                {
                    namedScores[j] = personKnowledge.VerifyBelief(named[j]);
                    motiveScores[j] = personKnowledge.VerifyBelief(motive[j]);
                    killerScores[j] = (namedScores[j] + motiveScores[j]) / 2;
                    if(killerScores[j] > bestScore)
                    {
                        bestScore = killerScores[j];
                        bestSuspect = j;
                    }
                }
                
                if(bestScore == 0f)
                {
                    // TODO: check innocence
                    discussion.QueueDialogue(mPeople[i], sprite, "I have no idea.");
                }
                else
                {
                    if(bestSuspect == i)
                    {
                        // this is me! Don't accuse myself.
                        if(Random.Range(0, 100) == 0)
                        {
                            discussion.QueueDialogue(p, sprite, "Well I think I did it, but I'm not going to say that out loud.");
                            discussion.QueueDialogue(p, sprite, "...Oh wait.");
                            continue;
                        }
                        else
                        {
                            // look for another person to pin the blame on
                            bestScore = 0;
                            bestSuspect = -1;
                            for(int j = 0; j < 3; ++j)
                            {
                                if (j == i) continue;
                                if(killerScores[j] > bestScore)
                                {
                                    bestScore = killerScores[j];
                                    bestSuspect = j;
                                }
                            }

                            if(bestSuspect < 0)
                            {
                                // randomly accuse someone else
                                int randomAccusation = Random.Range(0, 3);
                                if (randomAccusation == i) randomAccusation = (randomAccusation + 1) % 3;
                                discussion.QueueDialogue(p, sprite, hairColors[randomAccusation] + " did it!");
                                continue;
                            }
                        }
                    }

                    // explain the accusation
                    string confidenceQualifier = "";
                    if (bestScore >= 1)
                    {
                        confidenceQualifier = "I'm certain ";
                    }
                    else if (bestScore >= 0.5)
                    {
                        confidenceQualifier = "I'm pretty sure ";
                    }
                    else
                    {
                        confidenceQualifier = "I think ";
                    }
                    discussion.QueueDialogue(p, sprite, confidenceQualifier + hairColors[bestSuspect] + " did it.");
                    List<string> namedExplanation = p.knowledge.ExplainBelief(named[bestSuspect]);
                    foreach(string s in namedExplanation)
                    {
                        discussion.QueueDialogue(p, sprite, s);
                    }

                    List<string> motiveExplanation = p.knowledge.ExplainBelief(motive[bestSuspect]);
                    if (motiveExplanation.Count > 0)
                    {
                        if(namedExplanation.Count > 0)
                            discussion.QueueDialogue(p, sprite, "Also...");

                        foreach (string s in motiveExplanation)
                        {
                            discussion.QueueDialogue(p, sprite, s);
                        }
                    }
                    
                }
            }
            /*
            Sentence killer0 = new Sentence(Noun.Blonde, Verb.Is, Noun.Killer, Adverb.True);
            Sentence killer1 = new Sentence(Noun.Brunette, Verb.Is, Noun.Killer, Adverb.True);
            Sentence killer2 = new Sentence(Noun.Redhead, Verb.Is, Noun.Killer, Adverb.True);
            for (int i = 0; i < 3; ++i)
            {
                if(i != PlayerId)
                {
                    Knowledge personKnowledge = mPeople[i].knowledge;
                    Noun myHair = mPeople[i].AttributeMap[NounType.HairColor];
                    Sprite[] sprite = { mPeople[i].HeadSprite };

                    float confidence0 = personKnowledge.VerifyBelief(killer0);
                    float confidence1 = personKnowledge.VerifyBelief(killer1);
                    float confidence2 = personKnowledge.VerifyBelief(killer2);

                    if (confidence0 > 0 && myHair != Noun.Blonde)
                        discussion.QueueDialogue(mPeople[i], sprite, "I think BLONDE did it (confidence " + confidence0 + ")");
                    else if (confidence1 > 0 && myHair != Noun.Brunette)
                        discussion.QueueDialogue(mPeople[i], sprite, "I think BROWN did it (confidence " + confidence1 + ")");
                    else if (confidence2 > 0 && myHair != Noun.Redhead)
                        discussion.QueueDialogue(mPeople[i], sprite, "I think RED did it (confidence " + confidence2 + ")");
                    else
                    {
                        float innocenceBlonde = personKnowledge.VerifyBelief(new Sentence(Noun.Blonde, Verb.Is, Noun.Killer, Adverb.False));
                        float innocenceBrown = personKnowledge.VerifyBelief(new Sentence(Noun.Brunette, Verb.Is, Noun.Killer, Adverb.False));
                        float innocenceRed = personKnowledge.VerifyBelief(new Sentence(Noun.Redhead, Verb.Is, Noun.Killer, Adverb.False));

                        if (myHair == Noun.Blonde)
                        {
                            if (innocenceBrown > 0f || innocenceRed > 0f)
                            {
                                string innocentName = innocenceBrown > innocenceRed ? "BROWN" : "RED";
                                string guiltyName = innocenceBrown > innocenceRed ? "RED" : "BROWN";
                                discussion.QueueDialogue(mPeople[i], sprite, "Well I didn't do it, and " + innocentName + " didn't do it, so " + guiltyName + " did.");
                            }
                            else
                            {
                                discussion.QueueDialogue(mPeople[i], sprite, "I have no idea.");
                            }
                        }
                        else if (myHair == Noun.Brunette)
                        {
                            if (innocenceBlonde > 0f || innocenceRed > 0f)
                            {
                                string innocentName = innocenceBlonde > innocenceRed ? "BLONDE" : "RED";
                                string guiltyName = innocenceBlonde > innocenceRed ? "RED" : "BLONDE";
                                discussion.QueueDialogue(mPeople[i], sprite, "Well I didn't do it, and " + innocentName + " didn't do it, so " + guiltyName + " did.");
                            }
                            else
                            {
                                discussion.QueueDialogue(mPeople[i], sprite, "I have no idea.");
                            }

                        }
                        else if (myHair == Noun.Redhead)
                        {
                            if (innocenceBrown > 0f || innocenceBlonde > 0f)
                            {
                                string innocentName = innocenceBrown > innocenceBlonde ? "BROWN" : "BLONDE";
                                string guiltyName = innocenceBrown > innocenceBlonde ? "BLONDE" : "BROWN";
                                discussion.QueueDialogue(mPeople[i], sprite, "Well I didn't do it, and " + innocentName + " didn't do it, so " + guiltyName + " did.");
                            }
                            else
                            {
                                discussion.QueueDialogue(mPeople[i], sprite, "I have no idea.");
                            }
                        }
                    }
                }
            }
            */
            discussion.Start();
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

    public void PlayerFoundClue(ClueObject clue)
    {
        // TODO: check if the object is the not-paper photo. If so, play PICKUP
        AudioPlayer.PlaySound(AudioClipIndex.PAPER);
        ClueItem item = clue.mItem;
        PersonState player = mPeople[0];

        // add clue to journal
        if (item.info != null)
            PlayerJournal.AddClue(item.info);

        player.knowledge.AddKnowledge(item.info.GetSentence());

        List<ClueItem> clues = mCluesInRooms[mCurrentRoom];
        clues.Remove(item);
        Destroy(clue.gameObject);
        
        Sprite relevantImage = SpriteManager.GetSprite(item.spriteName);
        DialogBlock discussion = new DialogBlock(new PersonState[] { player }, OnClueDismissed);
        discussion.QueueDialogue(player, new Sprite[] { relevantImage }, item.description);
        mCluesFoundThisRound++;
        if (mCluesFoundThisRound >= MAX_CLUES_PER_ROUND)
        {
            discussion.QueueDialogue(player, new Sprite[] { }, "I'd better get back to the common area now.");
        }
        discussion.Start();
    }

    public void OnClueDismissed(int i)
    {
        // Go to next stage after dismissing the Nth clue
        if (mCluesFoundThisRound >= MAX_CLUES_PER_ROUND)
        {
            mCluesFoundThisRound = 0;
            StartStage(mCurrentStage + 1);
        }
    }
}
