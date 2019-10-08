using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piano : MonoBehaviour
{
    bool pianoPlaying = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(pianoPlaying)
        {
            if(!AudioPlayer.IsPlaying())
            {
                pianoPlaying = false;
                MusicPlayer.Play();
            }
        }
    }

    private void OnMouseDown()
    {
        if (UIController.Get().IsVisible()) { return; }

        if (!pianoPlaying)
        {
            MusicPlayer.Stop();
            AudioPlayer.PlaySound(AudioClipIndex.PIANO);
            pianoPlaying = true;
        }
        else
        {
            StopPiano();
        }

    }

    private void StopPiano()
    {
        AudioPlayer.Stop();
        MusicPlayer.Play();
        pianoPlaying = false;
    }

    void OnDestroy()
    {
        if(pianoPlaying)
        {
            StopPiano();
        }
    }

}
