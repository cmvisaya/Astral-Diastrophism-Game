using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPickup : MonoBehaviour
{

    public int value;

    public int audioClipIndex = 3;
    public float volume = 0.1f;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            FindObjectOfType<GameManager>().AddGold(value);
            FindObjectOfType<AudioManager>().PlaySoundEffect(audioClipIndex, volume);
            Destroy(gameObject);
        }
    }
}
