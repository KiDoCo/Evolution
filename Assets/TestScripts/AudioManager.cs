using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioClip[]         SFXClips;
    public AudioClip[]         MusicClips;

    //Insert Functions which are called when player is doing action related to sounds
    


    /// <summary>
    /// Audio when character is hit
    /// </summary>
    public void Hurt()
    {

    }


    private void RandomizeSFX(AudioClip clip)
    {
        
    }

    private void HurtSFX()
    {
        Debug.Log("Sound");
        //RandomizeSFX(SFXClips[0]);
    }
    private void Awake()
    {
        EventManager.AddHandler(EVENT.PlayerHit, HurtSFX);
    }
}
