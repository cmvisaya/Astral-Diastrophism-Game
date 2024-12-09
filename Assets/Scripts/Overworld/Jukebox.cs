using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jukebox : MonoBehaviour
{

    public int bgmIndex;
    public float volume;
    public float loopAtTime;
    public float loopToTime;
    private bool isPlaying = false;

    public static Jukebox Instance;

    private void Awake()
    {
        //audioClip = Resources.Load<AudioClip>("ForestFreeRoam.wav");

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (bgmIndex >= 0 && !isPlaying)
        {
            //FindObjectOfType<AudioManager>().PlayBGM(bgmIndex, volume); //Original line of code
            FindObjectOfType<AudioManager>().PlayBGM(bgmIndex, volume, loopAtTime, loopToTime); //Tests looping audio
            //FindObjectOfType<AudioManager>().PlayBGM(0, 0.1f); //Tests multiple audio sources playing at once
            isPlaying = true;
        }
    }
}
