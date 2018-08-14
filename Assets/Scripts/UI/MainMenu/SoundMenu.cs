using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundMenu : MonoBehaviour {

public void SetSFXVolume(float volume)
    {
       
        AudioManager.Sfxvolume(volume);

    }

public void SetMusicVolume(float volume)
    {
        AudioManager.Musicvolume(volume);
    }

}
