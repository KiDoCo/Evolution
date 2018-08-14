using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class AudioManager : MonoBehaviour
{
    //lists containing sounds 
    private static List<AudioClip> SFXClips = new List<AudioClip>();
    private static List<AudioClip> MusicClips = new List<AudioClip>();
    static float musicvolume;
    static float sfxvolume;
    //list of sounds and their id

    /* Sound Effect ID list (SFX)
     0: Death
     1: Eat
     2: Hurt
     3: Lose
     4: RoundBegin
     5: RoundEnd
     6: Victory
     */

    /* Music ID list
     0: Ambient Music
     1: Hunting Music
     2: Main Menu
     */
    /// <summary>
    /// Randomizes the pitch of a clip
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="source"></param>
    private static void RandomizeSFX(AudioClip clip, AudioSource source)
    {
        source.clip = clip;

        // source.pitch = Random.Range(0.8f, 1.0f);
        source.Play();
    }

    private static void SFXMethod(AudioSource source, int id)
    {
        RandomizeSFX(SFXClips[id], source);
    }

    private static void MusicMethod(AudioSource source, int id)
    {
        source.clip = MusicClips[id];

        source.Play();
    }

    private static void StopSound(AudioSource source, int id)
    {
        source.Stop();
    }

    public static void Sfxvolume(float setsfxvolume)
    {
        sfxvolume = setsfxvolume;
    }

    public static void Musicvolume(float setmusicvolume)
    {
        musicvolume = setmusicvolume;
    }

    

    //Unity Methods

    private void Awake()
    {
        SFXClips.AddRange(WWW.LoadFromCacheOrDownload("file:///" + (Directory.GetCurrentDirectory() + "/Assets/StreamingAssets/AssetBundles/soundeffects.sfx").Replace("\\", "/"),0).assetBundle.LoadAllAssets<AudioClip>());
        MusicClips.AddRange(WWW.LoadFromCacheOrDownload("file:///" + (Directory.GetCurrentDirectory() + "/Assets/StreamingAssets/AssetBundles/music.mfg").Replace("\\", "/"), 0).assetBundle.LoadAllAssets<AudioClip>());
        Debug.Log("Sound effects loaded : " + SFXClips.ToArray().Length);
        Debug.Log("Music loaded : "         + MusicClips.ToArray().Length);
    }

    private void Start()
    {
        //sound effect handler adding
        EventManager.SoundAddHandler(EVENT.PlaySFX, SFXMethod);
        EventManager.SoundAddHandler(EVENT.StopSound, StopSound);

        //music handler adding
        EventManager.SoundAddHandler(EVENT.PlayMusic, MusicMethod);
    }
}
