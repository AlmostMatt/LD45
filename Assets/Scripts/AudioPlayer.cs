using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Probably remove or dont use PFFT
public enum AudioClipIndex { NONE=-1, IMPACT=0, PAPER = 1, PICKUP = 2, HI=3, HMM=4, DISAGREE=5, SURPRISE_EH=6, REALIZATION=7, SURPRISE_AH=8};

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
        if (index == AudioClipIndex.NONE)
        {
            return;
        }
        instance.GetComponent<AudioSource>().PlayOneShot(instance.audioClips[(int)index]);
    }
}