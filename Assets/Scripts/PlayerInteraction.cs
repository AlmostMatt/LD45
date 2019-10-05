﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
 * Contains functions related to player interaction
 */
public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Get() { return GameObject.FindWithTag("GameRules").GetComponent<PlayerInteraction>(); }

    public delegate void DialogueDismissedCallback(int btnIdx);

    // -- DIALOGUE/TEXT -- (should this move to a separate system?)
    private DialogueDismissedCallback mDismissedCallback;
    List<string> mDialogueMessages = new List<string>();
    public void QueueDialogue(string msg)
    {
        mDialogueMessages.Add(msg);
    }
    public void ContinueDialogue()
    {
        if (mDialogueMessages.Count == 0) {
            UIController.Get().HideUI();

            if(mDismissedCallback != null)
            {
                mDismissedCallback(0);
                mDismissedCallback = null;
            }
            return;
        }

        string msg = mDialogueMessages[0];
        mDialogueMessages.RemoveAt(0);
        UIController.Get().ShowMessage(msg);
    }
    public void OpenDialogue(DialogueDismissedCallback callback = null)
    {
        if (callback != null) { mDismissedCallback = callback; }

        ContinueDialogue();
    }
    // -- DIALOGUE/TEXT --

    // Start is called before the first frame update
    void Start()
    {
    }

    public void StartDialog(int personId)
    {
        if (UIHasOverlay()) { return; }
        UIController.Get().ShowDialog(personId);
    }

    public void GoToRoom(string scene)
    {
        if (UIHasOverlay()) { return; }
        GameState.Get().MoveToRoom(GameState.Get().PlayerId, scene);
    }

    private bool UIHasOverlay()
    {
        return UIController.Get().IsVisible();
    }
}