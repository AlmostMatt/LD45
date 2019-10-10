using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer instance;

    public bool MusicEnabled = true;

    void Start()
    {
        instance = this;
        GetComponent<AudioSource>().Play();
    }

    void Update()
    {
    }

    public void ToggleMusic()
    {
        MusicEnabled = !MusicEnabled;
        if (MusicEnabled)
            GetComponent<AudioSource>().UnPause();
        else
            GetComponent<AudioSource>().Pause();
    }

    public static void Stop()
    {
        instance.GetComponent<AudioSource>().Stop();
    }

    public static void Play()
    {
        instance.GetComponent<AudioSource>().Play();
    }
}