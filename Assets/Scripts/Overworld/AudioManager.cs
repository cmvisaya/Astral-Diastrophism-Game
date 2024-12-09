using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public AudioSource soundEffectSource;
    public AudioSource[] bgmSources;
    public bool[] bgmSourceAtIndexIsPlaying;

    public AudioClip[] soundEffects;
    public AudioClip[] backgroundMusic;

    public float[] loopAtTimes;
    public float[] loopToTimes;

    public float loopAtTime;
    public float loopToTime;

    public static AudioManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSourceAtIndexIsPlaying = new bool[bgmSources.Length];
        loopAtTimes = new float[bgmSources.Length];
        loopToTimes = new float[bgmSources.Length];
    }

    // Start is called before the first frame update
    void Start()
    {
        if (soundEffectSource == null)
        {
            Debug.LogError("Sound effect source not found in Audio Manager");
        }
        for(int i = 0; i < bgmSources.Length; i++)
        {
            if (bgmSources[i] == null)
            {
                Debug.LogError("BGM source " + i + " not found in Audio Manager");
            }
        }
    }

    void Update()
    {
        for(int i = 0; i < bgmSources.Length; i++)
        {
            if (bgmSources[i] != null && loopAtTimes[i] != 0f && bgmSources[i].time >= loopAtTimes[i])
            {
                bgmSources[i].time = loopToTimes[i];
            }
        }
    }

    public void StopAll()
    {
        for(int i = 0; i < bgmSources.Length; i++)
        {
            bgmSources[i].Stop();
            bgmSourceAtIndexIsPlaying[i] = false;
            loopAtTimes[i] = 0f;
            loopToTimes[i] = 0f;
        }
    }

    public void PlaySoundEffect(AudioClip audioClip, float volume)
    {
        soundEffectSource.PlayOneShot(audioClip, volume);
    }

    public void PlayBGM(AudioClip audioClip, float volume)
    {
        for(int i = 0; i < bgmSources.Length; i++)
        {
            if(bgmSources[i] != null && !bgmSourceAtIndexIsPlaying[i])
            {
                bgmSources[i].clip = audioClip;
                bgmSources[i].volume = volume;
                bgmSources[i].Play();
                loopAtTimes[i] = loopAtTime;
                loopToTimes[i] = loopToTime;
                bgmSourceAtIndexIsPlaying[i] = true;
                break;
            }
        }
    }

    public void PlaySoundEffect(int index, float volume)
    {
        PlaySoundEffect(soundEffects[index], volume);
    }

    public void PlayBGM(int index, float volume)
    {
        loopAtTime = 0f;
        loopToTime = 0f;
        PlayBGM(backgroundMusic[index], volume);
    }

    public void PlayBGM(int index, float volume, float loopAtTime, float loopToTime)
    {
        this.loopAtTime = loopAtTime;
        this.loopToTime = loopToTime;
        PlayBGM(backgroundMusic[index], volume);
    }
}
