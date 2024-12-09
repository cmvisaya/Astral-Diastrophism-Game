using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitiateBattle : MonoBehaviour
{
    public string battleScene;
    public int battleBgmIndex = -1;
    public float battleBgmVolume;

    public int reflectedID;

    private string overworldReturnScene;

    private Vector3 distToPlayer;

    public GameObject[] enemyPartyPrefabs = new GameObject[3];
    public GameObject[] possibleSpawnPrefabs;
    public int[] possibleSpawnRaffleTickets;
    public int secondSummonDecay;

    // Start is called before the first frame update
    void Start()
    {
        //gameObject.name = GetInstanceID().ToString();
        enemyPartyPrefabs[0] = possibleSpawnPrefabs[1];
        overworldReturnScene = SceneManager.GetActiveScene().name;
    }

    void Update()
    {
        distToPlayer = GameObject.Find("Player").transform.position - transform.position;

        if (distToPlayer.magnitude <= 1.6f && FindObjectOfType<GameManager>().interactable) //1.6 just happens to be the magic number and idk why
        {
            CalculateSummonedParty(1);
            FindObjectOfType<GameManager>().InitiateBattle(battleScene, overworldReturnScene, reflectedID, transform.position, enemyPartyPrefabs);
            if (battleBgmIndex >= 0)
            {
                FindObjectOfType<AudioManager>().StopAll();
                FindObjectOfType<AudioManager>().PlayBGM(battleBgmIndex, battleBgmVolume);
            }
        }
    }

    void CalculateSummonedParty(int currentSlot)
    {
        int totalTickets = 0;
        for(int i = 0; i < possibleSpawnRaffleTickets.Length; i++) //Calculate total tickets to be used as cap for roll range
        {
            if(possibleSpawnRaffleTickets[i] < 0)
            {
                possibleSpawnRaffleTickets[i] = 0;
            }
            totalTickets += possibleSpawnRaffleTickets[i];
        }
        Debug.Log("Total Tickets: " + totalTickets);

        int roll = Random.Range(0, totalTickets);
        Debug.Log("Roll: " + roll);

        int ticketsCounted = 0;
        int prefabSlotToSummon = 0;
        for(int i = 0; i < possibleSpawnRaffleTickets.Length; i++)
        {
            Debug.Log("Checking slot " + prefabSlotToSummon);
            ticketsCounted += possibleSpawnRaffleTickets[i];
            Debug.Log("Tickets Counted: " + ticketsCounted);
            if(ticketsCounted > roll) //If you counted the winning ticket, break at the current slot to summon
            {
                break;
            }
            prefabSlotToSummon++; //If you didn't count the winning ticket, go to the next prefab slot and count their tickets.
        }

        Debug.Log(currentSlot);
        enemyPartyPrefabs[currentSlot] = possibleSpawnPrefabs[prefabSlotToSummon];

        if(currentSlot < 2 && prefabSlotToSummon != 0) //If you summoned a non-null enemy into the first slot
        {
            for(int i = 1; i < possibleSpawnRaffleTickets.Length; i++)
            {
                possibleSpawnRaffleTickets[i] -= secondSummonDecay;
            }
            CalculateSummonedParty(currentSlot + 1);
        }
    }

    /*private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && FindObjectOfType<GameManager>().interactable)
        {
            FindObjectOfType<GameManager>().InitiateBattle(battleScene, overworldReturnScene, reflectedID);
            if (battleBgmIndex >= 0)
            {
                FindObjectOfType<AudioManager>().PlayBGM(battleBgmIndex, battleBgmVolume);
            }
        }
    }*/
}
