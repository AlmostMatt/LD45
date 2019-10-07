using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Probably remove or dont use PFFT
public enum AudioClipIndex { NONE=-1, IMPACT=0, PAPER = 1, PICKUP = 2, HI=3, HMM=4, DISAGREE=5, SURPRISE_EH=6, REALIZATION=7, SURPRISE_AH=8, HMM2=9, AGREE=10, OH=11, PIANO=12};

public class AudioPlayer : MonoBehaviour
{
    public AudioClip[] audioClips;
    private static AudioPlayer instance;
    private static AudioClipIndex prevClip;

    void Start()
    {
        instance = this;
    }

    // call anywhere with code like AudioPlayer.PlaySound(AudioClipIndex.IMPACT);
    public static void PlaySound(AudioClipIndex index)
    {
        // Don't play the same clip twice in a row. instead, be silent.
        if (index == prevClip && index != AudioClipIndex.PIANO)
        {
            return;
        }
        prevClip = index;
        // special case to randomize HMM and HMM2 sounds
        if (index == AudioClipIndex.HMM || index == AudioClipIndex.HMM2)
        {
            index = new AudioClipIndex[] { AudioClipIndex.HMM, AudioClipIndex.HMM2 }[Random.Range(0, 2)];
        }
        if (index == AudioClipIndex.NONE)
        {
            return;
        }
        instance.GetComponent<AudioSource>().PlayOneShot(instance.audioClips[(int)index]);
    }

    public static bool IsPlaying()
    {
        return instance.GetComponent<AudioSource>().isPlaying;
    }

    public static void Stop()
    {
        instance.GetComponent<AudioSource>().Stop();
    }

}