using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int currentGold;

    public static GameManager Instance;

    public GameObject[] possiblePartyPrefabs;

    public bool interactable = true;
    public bool inOverworld = false;
    public bool pausing = false;
    public bool inShop = false;
    public bool battlesInescapable = false;

    [SerializeField] private string overworldReturnScene;
    [SerializeField] private int overworldReturnSceneNum;

    [SerializeField] private int reflectedID;
    private Vector3 reflectedPos;
    [SerializeField] private GameObject[] overworldEnemies;
    [SerializeField] private bool[] enemyIndexIsDisabled;
    [SerializeField] private int disableGOState;

    public AudioClip returnBgm;
    public float returnBgmVolume;

    public GameObject[] summonedEnemyPrefabs = new GameObject[3];

    //SAVED DATA
    public SimpleSave saveScript;

    public GameObject[] activePartyPrefabs = new GameObject[3];
    [SerializeField] private Transform activePartySpawnLoc;
    public Unit[] activePartyUnits = new Unit[3]; //Will be drawn into battle

    public GameObject[] inactivePartyPrefabs = new GameObject[9];

    [SerializeField] private float playerOverworldX = 0;
    [SerializeField] private float playerOverworldY = 0;
    [SerializeField] private float playerOverworldZ = 0;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if(GameObject.Find("Player Active Party"))
        {
            activePartySpawnLoc = GameObject.Find("Player Active Party").transform;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        InitActiveParty();
        UpdateOverworldEnemiesArray(true);
    }

    // Update is called once per frame
    void Update()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (Input.GetButtonDown("Escape") && inOverworld && FindObjectOfType<PlayerController>().hasControl && !inShop)
        {
            FindObjectOfType<PlayerController>().hasControl = false;
            interactable = false;
            pausing = true;
            FindObjectOfType<PauseMenuController>().Open();
        }
    }

    public void UpdateOverworldEnemiesArray(bool resetIndexArray)
    {
        overworldEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (resetIndexArray || overworldEnemies.Length != enemyIndexIsDisabled.Length)
        {
            enemyIndexIsDisabled = new bool[overworldEnemies.Length];
        }
    }

    public void PersistOverworldDisables()
    {
        for (int i = 0; i < overworldEnemies.Length; i++)
        {
            int currID = overworldEnemies[i].GetComponent<InitiateBattle>().reflectedID;

            Debug.Log("CurrID: " + currID + " | Disabled?: " + enemyIndexIsDisabled[currID]);

            if (enemyIndexIsDisabled[currID])
            {
                overworldEnemies[i].SetActive(false);
            }
        }
    }

    public void InitActiveParty()
    {
        for(int i = 0; i < activePartyPrefabs.Length; i++)
        {
            if(activePartyPrefabs[i] != null)
            {
                GameObject playerGO = Instantiate(activePartyPrefabs[i], activePartySpawnLoc);
                activePartyUnits[i] = playerGO.GetComponent<Unit>();
            }
        }
    }

    public void InitActivePartyMember(int slot)
    {
        if (activePartyPrefabs[slot] != null)
        {
            //If that player unit has already been spawned in, handle it differently to avoid duplicates
            GameObject playerGO = Instantiate(activePartyPrefabs[slot], activePartySpawnLoc);
            activePartyUnits[slot] = playerGO.GetComponent<Unit>();
        }
    }

    public void UpdateActiveParty()
    {
        for (int i = 0; i < activePartyPrefabs.Length; i++)
        {
            if (activePartyPrefabs[i] != null)
            {
                activePartyUnits[i].Initialize(activePartyPrefabs[i].GetComponent<Unit>());
            }
        }
    }

    public void SwapActivePartySlot(int slotToSwap, int indexOfPrefab)
    {
        activePartyPrefabs[slotToSwap] = possiblePartyPrefabs[indexOfPrefab];
        InitActivePartyMember(slotToSwap);
        //UpdateActiveParty();
    }

    public void HealPartyMemberHP(int slot, int amount)
    {
        activePartyUnits[slot].Heal(amount);
        activePartyUnits[slot].Initialize(activePartyUnits[slot]);
        ES3.Save("activePartyUnits", activePartyUnits); //Be prepared to fuck everything up again
    }

    public void HealPartyMemberMana(int slot, int amount)
    {
        activePartyUnits[slot].IncrementMana(amount);
        activePartyUnits[slot].Initialize(activePartyUnits[slot]);
        ES3.Save("activePartyUnits", activePartyUnits); //Be prepared to fuck everything up again
    }

    public void AddGold(int goldToAdd)
    {
        currentGold += goldToAdd;
        if(currentGold < 0) { currentGold = 0; }
    }

    public void InitiateBattle(string battleScene, string overworldReturnScene, int reflectedID, Vector3 reflectedPos, GameObject[] summonedEnemyPrefabs)
    {
        inOverworld = false;
        interactable = false;
        this.summonedEnemyPrefabs = summonedEnemyPrefabs;
        playerOverworldX = FindObjectOfType<PlayerController>().playerModel.transform.position.x;
        playerOverworldY = FindObjectOfType<PlayerController>().playerModel.transform.position.y;
        playerOverworldZ = FindObjectOfType<PlayerController>().playerModel.transform.position.z;
        this.overworldReturnScene = overworldReturnScene;
        this.overworldReturnSceneNum = SceneManager.GetActiveScene().buildIndex;
        this.reflectedID = reflectedID;
        this.reflectedPos = reflectedPos;
        FindObjectOfType<MinimapUI>(true).UIGO.SetActive(false);
        SceneManager.LoadScene(battleScene, LoadSceneMode.Single);
    }

    public void PersistBattleInitialization(int slot, Unit battleUnit)
    {
        activePartyUnits[slot].Initialize(battleUnit);
    }

    public void EndBattle(int disableGOState)
    {
        if (disableGOState != 1) //Didn't game over
        {
            inOverworld = true;
            interactable = true;
            battlesInescapable = false;
            this.disableGOState = disableGOState;
            SceneManager.LoadScene(overworldReturnSceneNum);

            if (SceneManager.GetActiveScene().buildIndex != overworldReturnSceneNum)
            {
                StartCoroutine("waitForSceneLoad", overworldReturnSceneNum);
            }

            ES3.Load("activePartyUnits", activePartyUnits);
            FindObjectOfType<AudioManager>().StopAll();
            FindObjectOfType<AudioManager>().PlayBGM(returnBgm, returnBgmVolume);
        }
        else
        {
            ReturnToTitleScreen();
        }
    }

    IEnumerator waitForSceneLoad(int sceneNumber)
    {
        while (SceneManager.GetActiveScene().buildIndex != sceneNumber)
        {
            yield return null;
        }

        // Do anything after proper scene has been loaded
        if (SceneManager.GetActiveScene().buildIndex == sceneNumber)
        {
            FindObjectOfType<MinimapUI>(true).UIGO.SetActive(true);
            UpdateOverworldEnemiesArray(false);
            HandleDisableEntity();
            PersistOverworldDisables();
            FindObjectOfType<PlayerController>().controller.enabled = false;
            GameObject.Find("Player").transform.position = new Vector3(playerOverworldX, playerOverworldY + 1f, playerOverworldZ);
            FindObjectOfType<PlayerController>().controller.enabled = true;
            FindObjectOfType<ObjectiveManager>().SwapObjective(FindObjectOfType<ObjectiveManager>().currentObjectiveIndex);
        }
    }

    public void HandleDisableEntity()
    {
        if(reflectedID >= 0)
        {
            switch (disableGOState)
            {
                case 0: //Disable
                    enemyIndexIsDisabled[reflectedID] = true;
                    break;
                case 1: //Do not disable
                    break;
                case 2: //Make temporarily uninteractable
                        //(You also want to set the position of the enemy with the appropriate reflected id to match when it was encountered with)
                    interactable = false;
                    for (int i = 0; i < overworldEnemies.Length; i++)
                    {
                        if (overworldEnemies[i].GetComponent<InitiateBattle>().reflectedID == reflectedID)
                        {
                            overworldEnemies[i].GetComponent<EnemyController>().controller.enabled = false;
                            overworldEnemies[i].transform.position = reflectedPos;
                            overworldEnemies[i].GetComponent<EnemyController>().controller.enabled = true;
                            break;
                        }
                    }
                    StartCoroutine(WaitForRunInteractability());
                    break;
            }
        }
    }

    IEnumerator WaitForRunInteractability()
    {
        yield return new WaitForSeconds(5f);
        interactable = true;
    }

    public void HandleOverworldDoor(int sceneNum, float xTarget, float yTarget, float zTarget)
    {
        UpdateOverworldEnemiesArray(true);

        SceneManager.LoadScene(sceneNum);

        if (SceneManager.GetActiveScene().buildIndex != sceneNum)
        {
            StartCoroutine(WaitForDoorScene(sceneNum, xTarget, yTarget, zTarget));
        }
    }

    IEnumerator WaitForDoorScene(int sceneNumber, float xTarget, float yTarget, float zTarget)
    {
        while (SceneManager.GetActiveScene().buildIndex != sceneNumber)
        {
            yield return null;
        }

        // Do anything after proper scene has been loaded
        if (SceneManager.GetActiveScene().buildIndex == sceneNumber)
        {
            FindObjectOfType<PlayerController>().controller.enabled = false;
            GameObject.Find("Player").transform.position = new Vector3(xTarget, yTarget, zTarget);
            FindObjectOfType<PlayerController>().controller.enabled = true;
            FindObjectOfType<ObjectiveManager>().SwapObjective(FindObjectOfType<ObjectiveManager>().currentObjectiveIndex);
        }
    }

    public void StartNewGame()
    {
        inOverworld = true;
        battlesInescapable = false;
        FindObjectOfType<AudioManager>().StopAll();
        SceneManager.LoadScene(1);
        if (SceneManager.GetActiveScene().buildIndex != 1)
        {
            StartCoroutine(waitForTextLoad(1));
        }
    }

    IEnumerator waitForTextLoad(int sceneNumber) //Change this to also take a filepath and pass it to the initiate dialogue call below
    {
        while (SceneManager.GetActiveScene().buildIndex != sceneNumber)
        {
            yield return null;
        }

        // Do anything after proper scene has been loaded
        if (SceneManager.GetActiveScene().buildIndex == sceneNumber)
        {
            FindObjectOfType<AudioManager>().PlayBGM(1, 2f);
            FindObjectOfType<DialogueManager>().PlayCutscene(0);
            //FindObjectOfType<DialogueManager>().PlayCutscene(4);
            //FindObjectOfType<DialogueManager>().InitiateDialogue(); //USE THIS LINE AS A TEST FOR NEW DIALOGUE
        }
    }

    void ReturnToTitleScreen()
    {
        FindObjectOfType<DialogueManager>().Reset();
        FindObjectOfType<ObjectiveManager>().Reset();
        Destroy(gameObject);
        SceneManager.LoadScene(0);
    }

    public void OnApplicationQuit() //Remove this functionality later!
    {
        for (int i = 0; i < activePartyUnits.Length; i++)
        {
            if (activePartyUnits[i] != null)
            {
                activePartyUnits[i].Initialize(activePartyPrefabs[i].GetComponent<Unit>());
            }
        }
        ES3.Save("activePartyUnits", activePartyUnits);
        Debug.Log("Application has quit!");
    }
}
