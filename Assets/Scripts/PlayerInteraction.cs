using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * Contains functions related to player interaction
 */
public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Get() { return GameObject.FindWithTag("GameRules").GetComponent<PlayerInteraction>(); }

    private Canvas mUICanvas;

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
