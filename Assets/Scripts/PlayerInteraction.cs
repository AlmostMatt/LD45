using System.Collections;
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

    private Canvas mUICanvas;

    // -- DIALOGUE/TEXT -- (should this move to a separate system?)
    List<string> mDialogueMessages = new List<string>();
    public void QueueDialogue(string msg)
    {
        mDialogueMessages.Add(msg);
    }
    public void ContinueDialogue()
    {
        if (mDialogueMessages.Count == 0) {
            mUICanvas.transform.Find("dialogView").gameObject.SetActive(false);
            return;
        }

        string msg = mDialogueMessages[0];
        mDialogueMessages.RemoveAt(0);
        mUICanvas.transform.Find("dialogView").gameObject.SetActive(true);
        mUICanvas.transform.Find("dialogView/V/empty/dialogText").GetComponent<Text>().text = msg;
    }
    // -- DIALOGUE/TEXT --

    // Start is called before the first frame update
    void Start()
    {
    }

    public void StartDialog(int PersonId)
    {
        if (UIHasOverlay()) { return; }
        mUICanvas.transform.Find("dialogView").gameObject.SetActive(true);
    }

    public void GoToRoom(string scene)
    {
        if (UIHasOverlay()) { return; }
        GameState.Get().MoveToRoom(GameState.Get().PlayerId, scene);
    }

    private bool UIHasOverlay()
    {
        if (mUICanvas == null)
        {
            mUICanvas = GameObject.FindWithTag("UICanvas").GetComponent<Canvas>();
        }
        return mUICanvas.transform.Find("dialogView").gameObject.activeInHierarchy;
    }
}
