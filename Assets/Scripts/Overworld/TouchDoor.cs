using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TouchDoor : MonoBehaviour
{
    public int targetSceneNum;
    public int bgmIndex = -1;
    public int sfxIndex = -1;
    public float bgmVolume;
    public float sfxVolume;

    public float xTarget;
    public float yTarget;
    public float zTarget;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (bgmIndex >= 0)
            {
                FindObjectOfType<AudioManager>().StopAll();
                FindObjectOfType<AudioManager>().PlayBGM(bgmIndex, bgmVolume);
            }
            if (sfxIndex >= 0)
            {
                FindObjectOfType<AudioManager>().PlaySoundEffect(sfxIndex, sfxVolume);
            }
            FindObjectOfType<GameManager>().HandleOverworldDoor(targetSceneNum, xTarget, yTarget, zTarget);
        }
    }
}
