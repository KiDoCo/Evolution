using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip[]         SFXClips;
    public AudioClip[]         MusicClips;

    //Insert Functions which are called when player is doing action related to sounds
    
    private void RandomizeSFX(AudioClip clip)
    {
        
    }

    /// <summary>
    /// Audio when character is hit
    /// </summary>
    private void HurtSFX()
    {
        Debug.Log("Sound");
        //RandomizeSFX(SFXClips[0]);
    }

    private void EatingSFX()
    {

    }

    private void RoundStartSFX()
    {

    }

    private void Awake()
    {
        EventManager.AddHandler(EVENT.PlayerHit, HurtSFX);
    }
}
