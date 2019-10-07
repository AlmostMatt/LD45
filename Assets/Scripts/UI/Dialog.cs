using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Represents a series of messages and interactions.
 * 
 * Has participants, current state, and some queued or potential state.
 */
public class DialogBlock
{
    public struct DialogEntry
    {
        public PersonState speaker;
        public Sprite[] sprites;
        public string message;
        public bool isInfoExchange;
        public bool isInfoExchangeRequest;

        public DialogEntry(PersonState spkr, Sprite[] sprs, string msg, bool isInfo, bool isInfoRequest)
        {
            speaker = spkr;
            sprites = sprs;
            message = msg;
            isInfoExchange = isInfo;
            isInfoExchangeRequest = isInfoRequest;
        }
    }

    public PersonState[] Participants; // Everyone in Participants will hear any new sentences
    private List<DialogEntry> mDialogEntries = new List<DialogEntry>();
    private UIButtonCallback mDismissedCallback;

    public DialogBlock(PersonState[] participants, UIButtonCallback endDialogCallback = null)
    {
        Participants = participants;
        mDismissedCallback = endDialogCallback;
    }

    // Speaker can be null or the player.
    public void QueueDialogue(PersonState speaker, Sprite[] sprites, string msg)
    {
        InsertDialogue(mDialogEntries.Count, speaker, sprites, msg);
    }

    private void InsertDialogue(int index, PersonState speaker, Sprite[] sprites, string msg)
    {
        mDialogEntries.Insert(index, new DialogEntry(speaker, sprites, msg, false, false));
    }

    public void QueueInfoExchangeRequest(PersonState speaker, Sprite[] sprites, string msg)
    {
        mDialogEntries.Add(new DialogEntry(speaker, sprites, msg, false, true));
    }

    public void QueueInformationExchange()
    {
        InsertInformationExchange(mDialogEntries.Count);
    }

    private void InsertInformationExchange(int index)
    {
        // TODO: have an order to revealing info? maybe an AI decides not to reveal info
        // if someone else reveals information that would incriminate them
        for (int i = 0; i < Participants.Length; i++)
        {
            Sprite[] sprites = new Sprite[] { Participants[i].HeadSprite };
            if (Participants[i].IsPlayer)
            {
                sprites = GetNonPlayerParticipantSprites();
            }
            mDialogEntries.Insert(index, new DialogEntry(Participants[i], sprites, "", true, false));
        }
    }

    public void Start()
    {
        Continue();
    }

    public void Continue()
    {
        if (mDialogEntries.Count == 0)
        {
            if (mDismissedCallback != null)
            {
                mDismissedCallback(0);
                mDismissedCallback = null;
            }
            return;
        }

        DialogEntry entry = mDialogEntries[0];
        mDialogEntries.RemoveAt(0);
        if (entry.isInfoExchangeRequest)
        {
            UIController.Get().ShowMessage(
                entry.speaker, entry.sprites, entry.message,
                new string[] { "Yes", "No" },
                new UIButtonCallback[] {
                    buttonIndex => { InsertInformationExchange(0);  Continue(); },
                    buttonIndex => { InsertDialogue(0,entry.speaker,entry.sprites,"Alright. Let's talk more later."); Continue(); },
                });
        } else if (entry.isInfoExchange)
        {
            if (entry.speaker.IsPlayer)
            {
                // Show prompt, and share the result with other participants of this dialog
                UIController.Get().AskForSentence(entry.sprites, sentence => {ShareInfo(GameState.Get().Player, sentence);  Continue(); });
            } else
            {
                // The GameState round-clues is guaranteed to be a recent clue that is not the result of combining multiple clues
                ClueInfo clueInfo = GameState.Get().mRoundClues[entry.speaker.PersonId];
                string message;
                if (clueInfo != null) {
                    Sentence newInfo = clueInfo.GetSentence();
                    message = "I found out " + entry.speaker.Speak(newInfo); // TODO: Announce the room where it was found
                    ShareInfo(entry.speaker, newInfo);
                    PlayerJournal.AddListen(entry.speaker.PersonId, newInfo);
                }
                else
                {
                    message = "I found nothing.";
                }
                UIController.Get().ShowMessage(entry.speaker, entry.sprites, message, new string[] { "Continue" }, new UIButtonCallback[] { buttonIndex => Continue() });
            }
        } else // a regular message, just show it
        {
            UIController.Get().ShowMessage(entry.speaker, entry.sprites, entry.message, new string[] { "Continue" }, new UIButtonCallback[] { buttonIndex => Continue() });
        }
    }

    private void ShareInfo(PersonState speaker, Sentence newInfo) {
        for (int j = 0; j < Participants.Length; ++j)
        {
            if (Participants[j].PersonId != speaker.PersonId)
            {
                string[] response = Participants[j].knowledge.Listen(speaker, newInfo);

                if(j != 0)
                {
                    for(int responseIdx = response.Length-1; responseIdx >= 0; --responseIdx)
                    {
                        InsertDialogue(0, Participants[j], new Sprite[] { Participants[j].HeadSprite }, response[responseIdx]);
                    }
                }
            }
        }
    }

    private Sprite[] GetNonPlayerParticipantSprites()
    {
        if (Participants.Length == 3)
        {
            return GameState.Get().NonPlayersHeads;
        }
        else if (Participants.Length == 2)
        {
            for (int i = 0; i<2; i++)
            {
                if (Participants[i].IsPlayer)
                {
                    return new Sprite[] { Participants[1-i].HeadSprite };
                }
            }
        }
        return new Sprite[] { };
    }
}
