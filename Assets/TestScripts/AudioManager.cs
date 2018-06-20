using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class AudioManager : MonoBehaviour
{
    public static AudioClip[]         SFXClips;
    public static AudioClip[]         MusicClips;

    //delegates
    private System.Action<AudioSource> hurtAction   = new System.Action<AudioSource>(HurtSFX);
    private System.Action<AudioSource> eatingAction = new System.Action<AudioSource>(EatingSFX);
    //Insert Functions which are called when player is doing action related to sounds

    private static void RandomizeSFX(AudioClip clip, AudioSource source)
    {
        source.clip = clip;
        source.pitch = Random.Range(0.1f, 1);
        source.Play();
    }

    /// <summary>
    /// Audio when character is hit
    /// </summary>
    private static void HurtSFX(AudioSource source)
    {
        Debug.Log("Sound");
        RandomizeSFX(SFXClips[0], source);
    }

    private static void EatingSFX(AudioSource source)
    {
        //RandomizeSFX(SFXClips[2], source);
    }

    private void RoundStartSFX(AudioSource source)
    {
        //source.clip = SFXClips[3];
        //source.Play();
    }

    private void RoundEndSFX(AudioSource source)
    {
        //source.clip = SFXClips[4];
        //source.Play();
    }

    private void Awake()
    {
    }
    private void Start()
    {
        EventManager.SoundAddHandler(EVENT.PlayerHit, hurtAction);
        EventManager.SoundAddHandler(EVENT.Eat, EatingSFX);
        EventManager.SoundAddHandler(EVENT.RoundBegin, RoundStartSFX);
        EventManager.SoundAddHandler(EVENT.RoundEnd, RoundEndSFX);
    }
}
