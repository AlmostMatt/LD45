using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public bool MusicEnabled = true;

    void Start()
    {
        GetComponent<AudioSource>().Play();
    }

    void Update()
    {
    }

    public void ToggleMusic()
    {
        MusicEnabled = !MusicEnabled;
        if (MusicEnabled)
            GetComponent<AudioSource>().Pause();
        else
            GetComponent<AudioSource>().UnPause();
    }
}