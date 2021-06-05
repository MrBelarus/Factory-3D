using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Sounds
{
    CantReplaceHere,
    ReplaceSound,
    DeleteObj,
    DeteleObjInDeleteModeSound,
    DeleteModeOn,
    BuyFactoryObjSound,
    FactoryObjTransformed,
    AchievementRecieved,
}

[System.Serializable]
public class SoundAudioClip
{
    public Sounds sound;
    public AudioClip audioClip;

    [Range(0f, 1f)]
    public float volume;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField]
    [Range(0f, 1f)]
    private float effectsVolume;
    public float EffectsVolume
    {
        get => effectsVolume;
        set
        {
            effectsVolume = Mathf.Clamp(value, 0f, 1f);
        }
    }

    public bool SoundsEffectsEnabled { set; get; }

    [SerializeField]
    private SoundAudioClip[] gameSounds;
    private Dictionary<Sounds, SoundAudioClip> soundAudioClipMap;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            SoundsEffectsEnabled = PlayerPrefs.GetInt("SoundsEnabled", 1) == 1;

            FillSoundsMap();
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void PlaySound(Sounds sound, float pitch = 1f)
    {
        if (!SoundsEffectsEnabled)
        {
            return;
        }    

        GameObject soundObj = new GameObject("Sound");
        AudioSource newSound = soundObj.AddComponent<AudioSource>();
        SoundAudioClip soundAudio = soundAudioClipMap[sound];

        if (soundAudio == null)
        {
            Debug.LogError("Sound wasn't found in the map!");
        }

        newSound.clip = soundAudio.audioClip;
        newSound.volume = soundAudio.volume * effectsVolume;
        newSound.pitch = pitch;

        newSound.Play();

        Destroy(soundObj, newSound.clip.length / newSound.pitch);
    }

    private void FillSoundsMap()
    {
        soundAudioClipMap = new Dictionary<Sounds, SoundAudioClip>();

        for (int i = 0; i < gameSounds.Length; i++)
        {
            SoundAudioClip soundAudio = gameSounds[i];
            soundAudioClipMap.Add(soundAudio.sound, soundAudio);
        }

        gameSounds = null;

        if (soundAudioClipMap.Count != System.Enum.GetNames(typeof(Sounds)).Length)
        {
            Debug.LogError("Not all sounds tuned in!");
        }
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("SoundsEnabled", SoundsEffectsEnabled ? 1 : 0);
    }
}
