using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AudioClipIndex { IMPACT=0, PAPER = 1, PICKUP = 2};

public class AudioPlayer : MonoBehaviour
{
    public AudioClip[] audioClips;
    private static AudioPlayer instance;

    void Start()
    {
        instance = this;
    }

    // call anywhere with code like AudioPlayer.PlaySound(AudioClipIndex.IMPACT);
    public static void PlaySound(AudioClipIndex index)
    {
        Debug.Log("Playing sound!");
        instance.GetComponent<AudioSource>().PlayOneShot(instance.audioClips[(int)index]);
    }
}