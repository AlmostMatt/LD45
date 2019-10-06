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

        public DialogEntry(PersonState spkr, Sprite[] sprs, string msg, bool isInfo)
        {
            speaker = spkr;
            sprites = sprs;
            message = msg;
            isInfoExchange = isInfo;
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
        mDialogEntries.Add(new DialogEntry(speaker, sprites, msg, false));
    }

    public void QueueInformationExchange()
    {
        // TODO: have an order to revealing info? maybe an AI decides not to reveal info
        // if someone else reveals information that would incriminate them
        for (int i=0; i < Participants.Length; i++)
        {
            Sprite[] sprites = new Sprite[] { Participants[i].HeadSprite };
            if (Participants[i].IsPlayer)
            {
                sprites = GetNonPlayerParticipantSprites();
            }
            mDialogEntries.Add(new DialogEntry(Participants[i], sprites, "", true));
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
        if (entry.isInfoExchange)
        {
            if (entry.speaker.IsPlayer)
            {
                // Show prompt, and share the result with other participants of this dialog
                UIController.Get().AskForSentence(entry.sprites, sentence => {ShareInfo(GameState.Get().Player, sentence);  Continue(); });
            } else
            {
                // The GameState round-clues is guaranteed to be a recent clue that is not the result of combining multiple clues
                Sentence newInfo = GameState.Get().mRoundClues[entry.speaker.PersonId].GetSentence();
                string message;
                if (newInfo != null) {
                    message = "I found " + newInfo; // TODO: Announce the room where it was found
                    ShareInfo(entry.speaker, newInfo);
                }
                else
                {
                    message = "I found nothing.";
                }
                UIController.Get().ShowMessage(entry.sprites, message, new string[] { "Continue" }, new UIButtonCallback[] { buttonIndex => Continue() });
            }
        } else // Not sharing info, just show the message
        {
            UIController.Get().ShowMessage(entry.sprites, entry.message, new string[] { "Continue" }, new UIButtonCallback[] { buttonIndex => Continue() });
        }
    }

    private void ShareInfo(PersonState speaker, Sentence newInfo) {
        for (int j = 0; j < Participants.Length; ++j)
        {
            if (Participants[j].PersonId != speaker.PersonId)
            {
                Participants[j].knowledge.Listen(speaker, newInfo);
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
