using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActiveParty : MonoBehaviour
{
    public static PlayerActiveParty Instance; //DONT USE THIS SCRIPT FOR EVERYTHING

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
