using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    public string scene;

    public void Start()
    {
        transform.Find("doorIcon").gameObject.SetActive(false);
    }

    public void Update()
    {
        
    }

    public void OnMouseOver() // Called every frame that the mouse is over something
    {
        // TODO: check if UI is in the way
        transform.Find("doorIcon").gameObject.SetActive(PlayerInteraction.Get().CanInteractWithScene());
    }

    public void OnMouseExit()
    {
        transform.Find("doorIcon").gameObject.SetActive(false);
    }

    public void OnMouseUpAsButton() // Alternatively, OnMouseDown
    {
        PlayerInteraction.Get().GoToRoom(scene);
    }
}
