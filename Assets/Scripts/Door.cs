using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    public string scene;

    public void OnMouseDown()
    {
        PlayerInteraction.Get().GoToRoom(scene);
    }

}
