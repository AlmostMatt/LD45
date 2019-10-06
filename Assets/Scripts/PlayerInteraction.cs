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
