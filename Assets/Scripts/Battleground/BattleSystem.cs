using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, RUN, WAITING }
public enum DesiredAction { NONE, SKILL, ITEM }

public class BattleSystem : MonoBehaviour
{
    public AudioClip menuCancel;
    public AudioClip menuSwitch;

    //public GameObject playerPrefab; //Refactor to 3 v 3
    //public GameObject enemyPrefab;

    public GameObject[] playerPartyPrefabs = new GameObject[3];
    public GameObject[] enemyPartyPrefabs = new GameObject[3];

    //public Transform playerBattleStation; //Refactor to 3 v 3
    //public Transform enemyBattleStation;

    public Transform[] playerBattleStations = new Transform[3];
    public Transform[] enemyBattleStations = new Transform[3];

    Unit playerUnit; //Keep this so we don't have to refactor everything
    Unit enemyUnit;

    [SerializeField] Unit[] playerUnits = new Unit[3];
    [SerializeField] Unit[] enemyUnits = new Unit[3];

    int numAllies = 0;
    int numEnemies = 0;

    //public BattleHUD playerHUD; //Refactor to 3 v 3
    //public BattleHUD enemyHUD;

    public BattleHUD[] playerHUDs = new BattleHUD[3];
    public BattleHUD[] enemyHUDs = new BattleHUD[3];

    public TextMeshProUGUI dialogueText;

    public GameObject onHitNotifs;
    public TextMeshProUGUI[] allyOnHitNotifTexts = new TextMeshProUGUI[5];
    public GameObject onLevelNotifs;
    public TextMeshProUGUI[] levelNotifTexts = new TextMeshProUGUI[8];

    [SerializeField] private GameObject initialMenu;
    [SerializeField] private GameObject skillMenu;
    [SerializeField] private GameObject itemMenu;
    [SerializeField] private GameObject targetSelectMenu;

    [SerializeField] private TextMeshProUGUI[] initialMenuButtonTexts = new TextMeshProUGUI[5];
    [SerializeField] private TextMeshProUGUI[] skillButtonTexts = new TextMeshProUGUI[5];
    [SerializeField] private TextMeshProUGUI[] itemButtonTexts = new TextMeshProUGUI[5];
    [SerializeField] private TextMeshProUGUI[] targetButtonTexts = new TextMeshProUGUI[3];

    [SerializeField] private TextMeshProUGUI skillDescriptionText;

    private int goldStored;
    public BattleState state;
    int currentPartySlot = 0;

    private int desiredSkillSlot;
    private int desiredInventorySlot;
    private int desiredTargetSlot;
    [SerializeField] private DesiredAction desiredAction = DesiredAction.NONE;
    private bool targetingPartySlot;
    private bool targetingWholeParty;

    //CAMERA
    private BattleCamController camController;
    [SerializeField] private GameObject cam;
    [SerializeField] private Transform newLookTarget;

    private float waitTime = 3f;

    //INVENTORY
    [SerializeField] private Backpack inventory;

    //CONTROLLER SUPPORT
    private bool m_isAxisInUse;
    private TextMeshProUGUI[] currentMenuTexts;
    private int currentMenuItem = 0;

    void Awake()
    {
        camController = cam.GetComponent<BattleCamController>();
        inventory = GameObject.Find("Backpack").GetComponent<Backpack>();
        playerPartyPrefabs = FindObjectOfType<GameManager>().activePartyPrefabs;
        enemyPartyPrefabs = FindObjectOfType<GameManager>().summonedEnemyPrefabs;
    }

    // Start is called before the first frame update
    void Start()
    {
        goldStored = 0;
        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    void Update() //This will handle button menu navigation
    {
        if (initialMenu.activeSelf || state == BattleState.START || state == BattleState.ENEMYTURN)
        {
            currentMenuTexts = initialMenuButtonTexts;
            if(Input.GetButtonDown("Select"))
            {
                FindObjectOfType<AudioManager>().PlaySoundEffect(0, 0.1f);
                switch (currentMenuItem)
                {
                    case 0:
                        OnSkillButton();
                        break;
                    case 1:
                        OnGuardButton();
                        break;
                    case 2:
                        OnItemButton();
                        break;
                    case 3:
                        OnRunButton();
                        break;
                    case 4:
                        OnEndTurnButton();
                        break;
                }
            }
        }
        else if (skillMenu.activeSelf)
        {
            if(playerUnit.skills[currentMenuItem] != null) { skillDescriptionText.text = playerUnit.skills[currentMenuItem].skillDescription; }
            else { skillDescriptionText.text = ""; }
            currentMenuTexts = skillButtonTexts;
            if(Input.GetButtonDown("Cancel")) { OnBackButton(); FindObjectOfType<AudioManager>().PlaySoundEffect(1, 0.1f); }
            if (Input.GetButtonDown("Select"))
            {
                FindObjectOfType<AudioManager>().PlaySoundEffect(0, 0.1f);
                OnSkillSelectButton(currentMenuItem);
            }
        }
        else if (itemMenu.activeSelf)
        {
            currentMenuTexts = itemButtonTexts;
            if (Input.GetButtonDown("Cancel")) { OnBackButton(); FindObjectOfType<AudioManager>().PlaySoundEffect(1, 0.1f); }
            if (Input.GetButtonDown("Select"))
            {
                FindObjectOfType<AudioManager>().PlaySoundEffect(0, 0.1f);
                OnItemSelectButton(currentMenuItem);
            }
        }
        else
        {
            //Handle camera control
            Vector3 sizeOffset = new Vector3(0, 1, 0);

            switch (targetingPartySlot)
            {
                case true:
                    //Handle Camera
                    if (playerUnits[currentMenuItem] != null) { sizeOffset = new Vector3(0, playerUnits[currentMenuItem].sizeOffset, 0); }
                    newLookTarget.SetPositionAndRotation(playerBattleStations[currentMenuItem].transform.position + sizeOffset, playerBattleStations[currentMenuItem].transform.rotation);
                    camController.ChangeLookTarget(newLookTarget);
                    camController.ChangePos(new Vector3(0, 1.15f, 0), 0, 270, 0);

                    //Handle HUD name highlight
                    ResetHUDHighlights();
                    playerHUDs[currentMenuItem].nameText.fontStyle = FontStyles.Bold | FontStyles.SmallCaps;
                    break;
                case false:
                    //Handle Camera
                    if (enemyUnits[currentMenuItem] != null) { sizeOffset = new Vector3(0, enemyUnits[currentMenuItem].sizeOffset, 0); }
                    newLookTarget.SetPositionAndRotation(enemyBattleStations[currentMenuItem].transform.position + sizeOffset, enemyBattleStations[currentMenuItem].transform.rotation);
                    camController.ChangeLookTarget(newLookTarget);
                    camController.ChangePos(new Vector3(0, 1.15f, 0), 0, 270, 0);

                    //Handle HUD name highlight
                    ResetHUDHighlights();
                    enemyHUDs[currentMenuItem].nameText.fontStyle = FontStyles.Bold | FontStyles.SmallCaps;
                    break;
            }

            //Handle Input
            currentMenuTexts = targetButtonTexts;
            if (Input.GetButtonDown("Cancel")) { OnBackButton(); ResetHUDHighlights(); FindObjectOfType<AudioManager>().PlaySoundEffect(1, 0.1f); }
            if (Input.GetButtonDown("Select")) { OnTargetSelectButton(currentMenuItem); ResetHUDHighlights(); FindObjectOfType<AudioManager>().PlaySoundEffect(0, 0.1f); }
            if (Input.GetButtonDown("Switch Sides")) { OnChangeSidesButton(); FindObjectOfType<AudioManager>().PlaySoundEffect(0, 0.1f); }
        }

        currentMenuTexts[currentMenuItem].fontStyle = FontStyles.Bold | FontStyles.SmallCaps;

        var selectionAxis = Input.GetAxisRaw("Horizontal");
        if (selectionAxis != 0 && state == BattleState.PLAYERTURN)
        {
            if (m_isAxisInUse == false)
            {
                if(selectionAxis > 0)
                {
                    currentMenuTexts[currentMenuItem].fontStyle = FontStyles.SmallCaps;
                    currentMenuItem++;
                    FindObjectOfType<AudioManager>().PlaySoundEffect(0, 0.1f);
                    if (currentMenuItem >= currentMenuTexts.Length)
                    {
                        if(itemMenu.activeSelf)
                        {
                            OnChangePageButton(false);
                        }
                        currentMenuItem = 0;
                    }
                }
                else
                {
                    currentMenuTexts[currentMenuItem].fontStyle = FontStyles.SmallCaps;
                    currentMenuItem--;
                    FindObjectOfType<AudioManager>().PlaySoundEffect(0, 0.1f);
                    if (currentMenuItem < 0)
                    {
                        if (itemMenu.activeSelf)
                        {
                            OnChangePageButton(true);
                        }
                        currentMenuItem = currentMenuTexts.Length - 1;
                    }
                }
                m_isAxisInUse = true;
            }
        }
        if (selectionAxis == 0)
        {
            m_isAxisInUse = false;
        }
    }

    void ResetHUDHighlights()
    {
        for(int i = 0; i < playerHUDs.Length; i++)
        {
            playerHUDs[i].nameText.fontStyle = FontStyles.SmallCaps;
            enemyHUDs[i].nameText.fontStyle = FontStyles.SmallCaps;
        }
    }

    IEnumerator SetupBattle()
    {
        //Unit[] loadedPlayerUnits = ES3.Load("activePartyUnits", new Unit[3]);
        Unit[] loadedPlayerUnits = FindObjectOfType<GameManager>().activePartyUnits;
        for(int i = 0; i < loadedPlayerUnits[0].skills.Length; i++)
        {
            if(loadedPlayerUnits[0].skills[i] != null)
            {
                Debug.Log("Persistence Check: " + loadedPlayerUnits[0].skills[i].skillName);
            }
        }
        for(int i = 0; i < playerPartyPrefabs.Length; i++)
        {
            if(playerPartyPrefabs[i] != null)
            {
                GameObject playerGO = Instantiate(playerPartyPrefabs[i], playerBattleStations[i]);
                playerUnits[i] = playerGO.GetComponent<Unit>();
                if (loadedPlayerUnits[i] != null)
                {
                    playerUnits[i].Initialize(loadedPlayerUnits[i]);
                }
                numAllies++;
            }
        }

        for (int i = 0; i < enemyPartyPrefabs.Length; i++)
        {
            if(enemyPartyPrefabs[i] != null)
            {
                GameObject enemyGO = Instantiate(enemyPartyPrefabs[i], enemyBattleStations[i]);
                enemyUnits[i] = enemyGO.GetComponent<Unit>();
                numEnemies++;
            }
        }

        initialMenu.SetActive(false);
        dialogueText.text = enemyUnits[0].unitName + " is raring to fight!";

        UpdateHUDs(true);

        playerUnit = playerUnits[0];
        enemyUnit = enemyUnits[0];

        yield return new WaitForSeconds(waitTime);

        PlayerInitialMenu();

        //Turn order calc
        int allyPartyInitiative = 0;
        int enemyPartyInitiative = 0;

        for (int i = 0; i < playerUnits.Length; i++)
        {
            if (playerUnits[i] != null)
            {
                allyPartyInitiative += playerUnits[i].initiative;
            }
            if (enemyUnits[i] != null)
            {
                enemyPartyInitiative += enemyUnits[i].initiative;
            }
        }

        Debug.Log("before roll: " + allyPartyInitiative + " | " + enemyPartyInitiative);

        allyPartyInitiative += Random.Range(1, 21);
        enemyPartyInitiative += Random.Range(1, 21);

        Debug.Log("after roll: " + allyPartyInitiative + " | " + enemyPartyInitiative);

        if(allyPartyInitiative >= enemyPartyInitiative)
        {
            state = BattleState.PLAYERTURN;
            PlayerPartyBegin();
        }
        else
        {
            state = BattleState.ENEMYTURN;
            EnemyPartyBegin();
        }
    }

    void UpdateHUDs(bool clearDelta)
    {
        for(int i = 0; i < playerHUDs.Length; i++)
        {
            if(playerUnits[i] != null)
            {
                if(clearDelta)
                {
                    playerHUDs[i].ClearDelta();
                }
                playerHUDs[i].SetHUD(playerUnits[i]);
            }
            else
            {
                playerHUDs[i].SetActive(false);
            }
            if(enemyUnits[i] != null)
            {
                if (clearDelta)
                {
                    enemyHUDs[i].ClearDelta();
                }
                enemyHUDs[i].SetHUD(enemyUnits[i]);
            }
            else
            {
                enemyHUDs[i].SetActive(false);
            }
        }
    }

    void UpdateAllBuffs()
    {
        for(int i = 0; i < playerUnits.Length; i++)
        {
            if(playerUnits[i] != null)
            {
                playerUnits[i].UpdateBuffs();
            }

            if(enemyUnits[i] != null)
            {
                enemyUnits[i].UpdateBuffs();
            }
        }
    }

    void ResetAnimIDs()
    {
        for(int i = 0; i < playerUnits.Length; i++)
        {
            if(playerUnits[i] != null)
            {
                playerUnits[i].animID = 0;
            }
            if(enemyUnits[i] != null)
            {
                enemyUnits[i].animID = 0;
            }
        }
    }

    void PlayerInitialMenu()
    {
        initialMenu.SetActive(true);
        skillMenu.SetActive(false);
        itemMenu.SetActive(false);
        targetSelectMenu.SetActive(false);
        currentMenuItem = 0;

        for(int i = 0; i < initialMenuButtonTexts.Length; i++)
        {
            initialMenuButtonTexts[i].fontStyle = FontStyles.SmallCaps;
        }

        dialogueText.text = "What will " + playerUnit.unitName + " do?";

        desiredAction = DesiredAction.NONE;
    }

    void PlayerSkillMenu(Unit unitToDisplay)
    {
        desiredAction = DesiredAction.SKILL;

        initialMenu.SetActive(false);
        skillMenu.SetActive(true);
        itemMenu.SetActive(false);
        targetSelectMenu.SetActive(false);
        currentMenuItem = 0;

        for (int i = 0; i < skillButtonTexts.Length; i++)
        {
            skillButtonTexts[i].fontStyle = FontStyles.SmallCaps;
        }

        dialogueText.text = "What will " + playerUnit.unitName + " do?";

        for (int i = 0; i < unitToDisplay.skills.Length; i++)
        {
            if(unitToDisplay.skills[i] != null)
            {
                skillButtonTexts[i].text = unitToDisplay.skills[i].skillName + "\n" + unitToDisplay.skills[i].manaCost + " MP";
                switch (unitToDisplay.skills[i].skillDaetra)
                {
                    case 1: //Existence
                        skillButtonTexts[i].color = new Color32(210, 170, 50, 255);
                        skillButtonTexts[i].ForceMeshUpdate();
                        break;
                    case -1: //Void
                        skillButtonTexts[i].color = new Color32(40, 40, 40, 255);
                        skillButtonTexts[i].ForceMeshUpdate();
                        break;
                    case 2: //Life
                        skillButtonTexts[i].color = new Color32(25, 120, 0, 255);
                        skillButtonTexts[i].ForceMeshUpdate();
                        break;
                    case -2: //Death
                        skillButtonTexts[i].color = new Color32(150, 0, 150, 255);
                        skillButtonTexts[i].ForceMeshUpdate();
                        break;
                    case 3: //Order
                        skillButtonTexts[i].color = new Color32(0, 0, 150, 255);
                        skillButtonTexts[i].ForceMeshUpdate();
                        break;
                    case -3: //Chaos
                        skillButtonTexts[i].color = new Color32(150, 0, 0, 255);
                        skillButtonTexts[i].ForceMeshUpdate();
                        break;
                }
            }
            else
            {
                skillButtonTexts[i].text = "None";
                skillButtonTexts[i].color = Color.black;
            }
        }
    }

    void PlayerItemMenu()
    {
        desiredAction = DesiredAction.ITEM;

        initialMenu.SetActive(false);
        skillMenu.SetActive(false);
        itemMenu.SetActive(true);
        targetSelectMenu.SetActive(false);
        currentMenuItem = 0;

        for (int i = 0; i < itemButtonTexts.Length; i++)
        {
            itemButtonTexts[i].fontStyle = FontStyles.SmallCaps;
        }

        dialogueText.text = "Select an item (" + (int) (inventory.currentPage + 1) + "/" + (inventory.items.Length / 5) + ")";

        for (int i = 0; i < 5; i++)
        {
            if (inventory.items[i + (inventory.currentPage * 5)] != null)
            {
                itemButtonTexts[i].text = inventory.items[i + (inventory.currentPage * 5)].itemName;
            }
            else
            {
                itemButtonTexts[i].text = "None";
            }
        }
    }

    /*
     * Okay, so for skill targeting. Make the targeting like OMORI where you can target any unit with any skill. That way, we can also 
     * I will need to refactor a fair bit of how skill targeting works to allow for AOE skills
     * IMPORTANT NOTE: AOE skills and random enemy skills do not need to take you to the selection menu, since it automatically targets an entire party
     */

    void TargetSelectMenu(bool targetingPartySlot)
    {
        initialMenu.SetActive(false);
        skillMenu.SetActive(false);
        itemMenu.SetActive(false);
        targetSelectMenu.SetActive(true);
        currentMenuItem = 0;

        for (int i = 0; i < targetButtonTexts.Length; i++)
        {
            targetButtonTexts[i].fontStyle = FontStyles.SmallCaps;
        }

        dialogueText.text = "Select a target...";

        for(int i = 0; i < playerUnits.Length; i++)
        {
            if(!targetingWholeParty)
            {
                if (targetingPartySlot)
                {
                    if (playerUnits[i] != null)
                    {
                        targetButtonTexts[i].text = playerUnits[i].unitName;
                    }
                    else
                    {
                        targetButtonTexts[i].text = "None";
                    }
                }
                else
                {
                    if (enemyUnits[i] != null)
                    {
                        targetButtonTexts[i].text = enemyUnits[i].unitName;
                    }
                    else
                    {
                        targetButtonTexts[i].text = "None";
                    }
                }
            }
            else
            {
                if (targetingPartySlot)
                {    
                    targetButtonTexts[i].text = "Ally Party";
                }
                else
                {
                    targetButtonTexts[i].text = "Enemy Party";
                }
            }
            
        }
    }

    IEnumerator PlayerSkill(int slot)
    {
        if(playerUnit.skills[slot] == null)
        {
            dialogueText.text = "You do not have a skill equipped in this slot!";
            yield return new WaitForSeconds(waitTime);
            yield break;
        }

        else if(playerUnit.currentPips <= 0)
        {
            dialogueText.text = playerUnit.unitName + " doesn't have enough pips!";
            yield return new WaitForSeconds(waitTime);
            yield break;
        }

        else if (!playerUnit.skills[slot].SkillUseableByMP(playerUnit))
        {
            dialogueText.text = "You do not have enough MP to use this skill!";
            yield return new WaitForSeconds(waitTime);
            yield break;
        }

        else
        {
            switch (playerUnit.skills[slot].targetingMode)
            {
                case TargetingMode.SELF:
                    {
                        enemyUnit = playerUnit; //Self
                        StartCoroutine(PlayerSkillExecute(slot));
                        break;
                    }
                case TargetingMode.SINGLE:
                    {
                        targetingPartySlot = false;
                        targetingWholeParty = false;
                        TargetSelectMenu(targetingPartySlot);
                        break;
                    }
                case TargetingMode.PARTY:
                    {
                        targetingPartySlot = false;
                        targetingWholeParty = true;
                        TargetSelectMenu(targetingPartySlot);
                        break;
                    }
            }
        }
    }

    IEnumerator PlayerSkillExecute(int slot)
    {
        if (!targetingWholeParty && (enemyUnit == null || (enemyUnit.isDead && !playerUnit.skills[slot].canTargetDead))) //REFACTOR THIS WHEN YOU CAN
        {
            dialogueText.text = "You did not select a valid target!";
            yield return new WaitForSeconds(waitTime);
            yield break;
        }

        state = BattleState.WAITING;
        camController.Reset(playerBattleStations[currentPartySlot], false, playerUnits[currentPartySlot]);
        PlayerInitialMenu();

        playerUnit.IncrementMana(-playerUnit.skills[slot].manaCost);
        //playerHUD.SetMP(playerUnit.currentMana);
        playerUnit.currentPips--;
        UpdateHUDs(true);


        //AYO FIZZLE AND MISS ARE GOING TO HAVE TO BE TWO COMPLETELY SEPARATE THINGS IF WE WANT AOE SKILLS TO BE ABLE TO GUARANTEED HIT ON SOME AND MISS ON OTHERS
        if (!playerUnit.skills[slot].SkillHit(playerUnit))
        {
            dialogueText.text = playerUnit.unitName + "'s skill fizzled out!"; //You could make it so that certain skills cant miss
            yield return new WaitForSeconds(waitTime);
            StartCoroutine(PlayerTurn(currentPartySlot));
            yield break;
        }

        playerUnit.animID = playerUnit.skills[slot].animID;

        //ALSO ALSO, we need to make sure the buttons are non-interactable while attacks are out and text is still going.
        //We can mitigate this by slapping state = BattleState.WAITING everywhere we need then immediately swapping back to a playerturn battle state.
        if(playerUnit.skills[slot].targetingMode == TargetingMode.PARTY)
        {
            if(targetingPartySlot)
            {
                playerUnit.skills[slot].UseSkill(playerUnit, playerUnits, dialogueText, playerHUDs, playerHUDs[currentPartySlot]);
            }
            else
            {
                playerUnit.skills[slot].UseSkill(playerUnit, enemyUnits, dialogueText, enemyHUDs, playerHUDs[currentPartySlot]);
            }
        }
        else
        {
            if (targetingPartySlot)
            {
                playerUnit.skills[slot].UseSkill(playerUnit, enemyUnit, dialogueText, playerHUDs[desiredTargetSlot], playerHUDs[currentPartySlot]);
            }
            else
            {
                playerUnit.skills[slot].UseSkill(playerUnit, enemyUnit, dialogueText, enemyHUDs[desiredTargetSlot], playerHUDs[currentPartySlot]);
            }
        }

        onHitNotifs.SetActive(true);
        UpdateHUDs(false);

        yield return new WaitForSeconds(2f);
        ResetAnimIDs();
        yield return new WaitForSeconds(1f);

        onHitNotifs.SetActive(false);

        if (enemyUnit.isDead)
        {
            Debug.Log("APPLY XP HERE");
            goldStored += (int)((100 + 50 * (enemyUnit.unitLevel - 1)) * UnityEngine.Random.Range(0.7f, 1.3f) / 3.0);
            for (int i = 0; i < playerUnits.Length; i++)
            {
                if (playerUnits[i] != null)
                {
                    if (playerUnits[i].GainXP(enemyUnit.unitLevel))
                    {
                        onLevelNotifs.SetActive(true);
                        dialogueText.text = playerUnits[i].unitName + " leveled up!";
                        yield return new WaitForSeconds(waitTime);

                        SkillPool potentialLearn = playerUnits[i].skillPool.GetComponent<SkillPool>();
                        for (int j = 0; j < potentialLearn.levelsLearnedAt.Length; j++)
                        {
                            if (potentialLearn.levelsLearnedAt[j] == playerUnits[i].unitLevel) //If we found a skill that is learned at a particular level
                            {
                                dialogueText.text = playerUnits[i].unitName + " can learn " + potentialLearn.skillPool[j].skillName + "!";
                                yield return new WaitForSeconds(waitTime);
                            }
                        }
                        onLevelNotifs.SetActive(false);
                    }
                }
            }
        }

        if (!PlayerPartyIsAlive())
        {
            state = BattleState.LOST;
            EndBattle();
        }
        else if (!EnemyPartyIsAlive())
        {
            state = BattleState.WON;
            EndBattle();
        }
        else
        {
            state = BattleState.PLAYERTURN;
            StartCoroutine(PlayerTurn(currentPartySlot));
        }
    }

    IEnumerator PlayerItem(int slot)
    {
        if (inventory.items[slot] == null)
        {
            dialogueText.text = "You do not have an item in this inventory slot!";
            yield return new WaitForSeconds(waitTime);
            yield break;
        }

        else if (playerUnit.currentPips < inventory.items[slot].pipCost)
        {
            dialogueText.text = "You do not have sufficient pips for this item!";
            yield return new WaitForSeconds(waitTime);
            yield break;
        }

        else
        {
            switch (inventory.items[slot].targetingMode)
            {
                case TargetingMode.SELF:
                    {
                        enemyUnit = playerUnit; //Self
                        StartCoroutine(PlayerItemExecute(slot));
                        break;
                    }
                case TargetingMode.SINGLE:
                    {
                        targetingPartySlot = false;
                        targetingWholeParty = false;
                        TargetSelectMenu(targetingPartySlot);
                        break;
                    }
                case TargetingMode.PARTY:
                    {
                        targetingPartySlot = false;
                        targetingWholeParty = true;
                        TargetSelectMenu(targetingPartySlot);
                        break;
                    }
            }
        }
    }

    IEnumerator PlayerItemExecute(int slot)
    {
        if (!targetingWholeParty && (enemyUnit == null || enemyUnit.isDead)) //Will likely have to refactor for ressurection skills
        {
            dialogueText.text = "You did not select a valid target!";
            yield return new WaitForSeconds(waitTime);
            yield break;
        }

        state = BattleState.WAITING;
        PlayerInitialMenu();
        camController.Reset(playerBattleStations[currentPartySlot], false, playerUnits[currentPartySlot]);
        //playerHUD.SetMP(playerUnit.currentMana);
        playerUnit.currentPips -= inventory.items[slot].pipCost;
        UpdateHUDs(true);

        playerUnit.animID = inventory.items[slot].animID;

        //ALSO ALSO, we need to make sure the buttons are non-interactable while attacks are out and text is still going.
        //We can mitigate this by slapping state = BattleState.WAITING everywhere we need then immediately swapping back to a playerturn battle state.
        if (inventory.items[slot].targetingMode == TargetingMode.PARTY)
        {
            if (targetingPartySlot)
            {
                inventory.items[slot].UseItem(playerUnit, playerUnits, dialogueText, playerHUDs, playerHUDs[currentPartySlot]);
            }
            else
            {
                inventory.items[slot].UseItem(playerUnit, enemyUnits, dialogueText, enemyHUDs, playerHUDs[currentPartySlot]);
            }
        }
        else
        {
            if (targetingPartySlot)
            {
                inventory.items[slot].UseItem(playerUnit, enemyUnit, dialogueText, playerHUDs[desiredTargetSlot], playerHUDs[currentPartySlot]);
            }
            else
            {
                inventory.items[slot].UseItem(playerUnit, enemyUnit, dialogueText, enemyHUDs[desiredTargetSlot], playerHUDs[currentPartySlot]);
            }
        }

        UpdateHUDs(false);
        inventory.items[slot] = null;

        yield return new WaitForSeconds(2f);
        ResetAnimIDs();
        yield return new WaitForSeconds(1f);

        if (enemyUnit.isDead)
        {
            Debug.Log("APPLY XP HERE");
            goldStored += (int)((100 + 50 * (enemyUnit.unitLevel - 1)) * UnityEngine.Random.Range(0.7f, 1.3f) / 3.0);
            for (int i = 0; i < playerUnits.Length; i++)
            {
                if (playerUnits[i] != null)
                {
                    if (playerUnits[i].GainXP(enemyUnit.unitLevel))
                    {
                        onLevelNotifs.SetActive(true);
                        dialogueText.text = playerUnits[i].unitName + " leveled up!";
                        yield return new WaitForSeconds(waitTime);

                        SkillPool potentialLearn = playerUnits[i].skillPool.GetComponent<SkillPool>();
                        for (int j = 0; j < potentialLearn.levelsLearnedAt.Length; j++)
                        {
                            if (potentialLearn.levelsLearnedAt[j] == playerUnits[i].unitLevel) //If we found a skill that is learned at a particular level
                            {
                                dialogueText.text = playerUnits[i].unitName + " can learn " + potentialLearn.skillPool[j].skillName + "!";
                                yield return new WaitForSeconds(waitTime);
                            }
                        }
                        onLevelNotifs.SetActive(false);
                    }
                }
            }
        }

        if (!PlayerPartyIsAlive())
        {
            state = BattleState.LOST;
            EndBattle();
        }
        else if (!EnemyPartyIsAlive())
        {
            state = BattleState.WON;
            EndBattle();
        }
        else
        {
            state = BattleState.PLAYERTURN;
            StartCoroutine(PlayerTurn(currentPartySlot));
        }
    }

    public IEnumerator ApplyStatuses(Unit target, BattleHUD targetHUD, int index) //Lets try using recursion
    {
        //dialogueText.text = "Applying statuses to " + target.unitName;
        
        if (index < target.statuses.Length)
        {
            if(target.statuses[index] != null && !target.isDead)
            {
                target.statuses[index].ApplyStatus(target, dialogueText, targetHUD);
                UpdateHUDs(false);
                yield return new WaitForSeconds(2f);
                ResetAnimIDs();
                yield return new WaitForSeconds(1f);
            }
            StartCoroutine(ApplyStatuses(target, targetHUD, index + 1));
        }
        
    }

    bool PlayerPartyIsAlive()
    {
        for(int i = 0; i < numAllies; i++)
        {
            if(!playerUnits[i].isDead)
            {
                return true;
            }
        }
        return false;
    }

    bool EnemyPartyIsAlive()
    {
        for (int i = 0; i < numEnemies; i++)
        {
            if (!enemyUnits[i].isDead)
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator PlayerRun()
    {
        if(FindObjectOfType<GameManager>().battlesInescapable)
        {
            dialogueText.text = "You can't run from this fight!";
            yield return new WaitForSeconds(waitTime);
            StartCoroutine(PlayerTurn(currentPartySlot));
            yield break;
        }

        if (playerUnit.currentPips <= 0)
        {
            dialogueText.text = playerUnit.unitName + " doesn't have enough pips!";
            yield return new WaitForSeconds(waitTime);
            StartCoroutine(PlayerTurn(currentPartySlot));
            yield break;
        }

        //Change to include escape success and failure
        dialogueText.text = playerUnit.unitName + "'s party tries to escape...";
        playerUnit.currentPips--;
        UpdateHUDs(true);

        int allyPartyInitiative = 0;
        int enemyPartyInitiative = 0;
        
        for (int i = 0; i < playerUnits.Length; i++)
        {
            if(playerUnits[i] != null)
            {
                allyPartyInitiative += playerUnits[i].initiative;
            }
            if(enemyUnits[i] != null)
            {
                enemyPartyInitiative += enemyUnits[i].initiative;
            }
        }

        Debug.Log("before roll: " + allyPartyInitiative + " | " + enemyPartyInitiative);

        allyPartyInitiative += Random.Range(1, 21);
        enemyPartyInitiative += Random.Range(1, 21);

        Debug.Log("after roll: " + allyPartyInitiative + " | " + enemyPartyInitiative);

        yield return new WaitForSeconds(waitTime);

        if(allyPartyInitiative >= enemyPartyInitiative)
        {
            EndBattle();
        }
        else
        {
            dialogueText.text = "The enemy party doesn't let you get away!";
            yield return new WaitForSeconds(waitTime);
            StartCoroutine(PlayerTurn(currentPartySlot));
        }
    }

    IEnumerator PlayerGuard()
    {
        state = BattleState.WAITING;
        if (playerUnit.currentPips > 0)
        {
            playerUnit.currentPips--;
            playerUnit.isGuarding = true;
            dialogueText.text = playerUnit.unitName + " braces for an attack!";
            UpdateHUDs(true);
            yield return new WaitForSeconds(waitTime);
            currentPartySlot++;
            StartCoroutine(PlayerTurn(currentPartySlot));
            //StartCoroutine(EnemyTurn());
        }
        else
        {
            dialogueText.text = playerUnit.unitName + " doesn't have enough pips!";
            yield return new WaitForSeconds(waitTime);
            StartCoroutine(PlayerTurn(currentPartySlot));
        }
    }

    void EnemyPartyBegin()
    {
        //enemyUnit.RegenPips();
        //StartCoroutine(EnemyTurn());

        initialMenu.SetActive(false);

        state = BattleState.ENEMYTURN;

        for (int i = 0; i < enemyUnits.Length; i++)
        {
            if (enemyUnits[i] != null)
            {
                enemyUnits[i].RegenPips();
                enemyUnits[i].statusesAppliedOnTurn = false;
            }
        }
        currentPartySlot = 0;
        StartCoroutine(EnemyTurn(currentPartySlot));
    }

    IEnumerator EnemyTurn(int slot)
    {
        UpdateAllBuffs();
        if (slot < enemyUnits.Length && enemyUnits[slot] != null)
        {
            enemyUnit = enemyUnits[slot];
            UpdateHUDs(true);
            camController.Reset(enemyBattleStations[slot], true, enemyUnits[slot]);
            state = BattleState.ENEMYTURN;
        }
        else
        {
            PlayerPartyBegin();
            yield break;
        }

        //HANDLE STATUS APPLICATION HERE, MAKING SURE TO UPDATE THE STATUSAPPLIEDONTURN VARIABLE - WILL HAVE TO PASS TO AN IENUMERATOR
        if (!enemyUnit.isDead && enemyUnit.HasStatus() && !enemyUnit.statusesAppliedOnTurn)
        {
            StartCoroutine(ApplyStatuses(enemyUnit, enemyHUDs[slot], 0));
            enemyUnit.statusesAppliedOnTurn = true;
            yield return new WaitForSeconds(enemyUnit.GetStatusWaitTime());

            if(enemyUnit.isDead)
            {
                Debug.Log("APPLY XP HERE");
                goldStored += (int)((100 + 50 * (enemyUnit.unitLevel - 1)) * UnityEngine.Random.Range(0.7f, 1.3f) / 3.0);
                for (int i = 0; i < playerUnits.Length; i++)
                {
                    if (playerUnits[i] != null)
                    {
                        if (playerUnits[i].GainXP(enemyUnit.unitLevel))
                        {
                            onLevelNotifs.SetActive(true);
                            dialogueText.text = playerUnits[i].unitName + " leveled up!";
                            yield return new WaitForSeconds(waitTime);

                            SkillPool potentialLearn = playerUnits[i].skillPool.GetComponent<SkillPool>();
                            for (int j = 0; j < potentialLearn.levelsLearnedAt.Length; j++)
                            {
                                if (potentialLearn.levelsLearnedAt[j] == playerUnits[i].unitLevel) //If we found a skill that is learned at a particular level
                                {
                                    dialogueText.text = playerUnits[i].unitName + " can learn " + potentialLearn.skillPool[j].skillName + "!";
                                    yield return new WaitForSeconds(waitTime);
                                }
                            }
                            onLevelNotifs.SetActive(false);
                        }
                    }
                }
            }

            if (!EnemyPartyIsAlive())
            {
                state = BattleState.WON;
                EndBattle();
                yield break;
            }
        }

        if (enemyUnit.isDead || enemyUnit.IsAsleep())
        {
            currentPartySlot++;
            StartCoroutine(EnemyTurn(currentPartySlot));
            yield break;
        }

        do
        {
            desiredTargetSlot = Random.Range(0, numAllies);
            playerUnit = playerUnits[desiredTargetSlot]; //Randomizes enemy single target (refactor with below logic to account for random skills)
        } while (playerUnit == null || playerUnit.isDead);

        //Below code makes the enemy select randomly between attack, guard, and pass
        int randAction = UnityEngine.Random.Range(0, 3);
        bool turnEnded = false;

        //Ensures that enemy will not pass turn with max pips or guard with max mana
        while ((randAction == 2 && enemyUnit.currentPips == enemyUnit.maxPips) || (randAction == 1 && enemyUnit.currentMana == enemyUnit.maxMana))
        {
            randAction = UnityEngine.Random.Range(0, 3);
        }

        switch (randAction) //0 = attack, 1 = guard, 2 = pass
        {
            case 0: //WILL HAVE TO HANDLE HEALING/BUFF SKILLS DIFFERENTLY TO ENSURE THE ENEMY PROPERLY TARGETS THEIR OWN PARTY
                enemyUnit.animID = enemyUnit.skills[0].animID;
                int randSlot = enemyUnit.GetRandomOccupiedSlot();
                enemyUnit.skills[randSlot].UseSkill(enemyUnit, playerUnit, dialogueText, playerHUDs[desiredTargetSlot], enemyHUDs[currentPartySlot]);
                enemyUnit.IncrementMana(-enemyUnit.skills[randSlot].manaCost);
                enemyUnit.currentPips--;
                onHitNotifs.SetActive(true);
                UpdateHUDs(false);
                break;
            case 1:
                dialogueText.text = enemyUnit.unitName + " braces for an attack!";
                enemyUnit.currentPips--;
                enemyUnit.isGuarding = true;
                break;
            case 2:
                dialogueText.text = enemyUnit.unitName + " holds actionability!";
                turnEnded = true;
                break;
        }

        yield return new WaitForSeconds(2f);
        ResetAnimIDs();
        yield return new WaitForSeconds(1f);

        onHitNotifs.SetActive(false);

        if (!PlayerPartyIsAlive())
        {
            state = BattleState.LOST;
            EndBattle();
        }
        else if (!EnemyPartyIsAlive())
        {
            state = BattleState.WON;
            EndBattle();
        }
        else if (enemyUnit.currentPips <= 0 || enemyUnit.isGuarding || turnEnded)
        {
            currentPartySlot++;
            StartCoroutine(EnemyTurn(currentPartySlot));
        }
        else
        {
            StartCoroutine(EnemyTurn(currentPartySlot));
        }
    }

    void EndBattle()
    {
        if(state == BattleState.WON)
        {
            dialogueText.text = playerUnit.unitName + "'s party is victorious!";
            PersistBattleStats();
            ES3.Save("activePartyUnits", playerUnits);
            StartCoroutine(HandleCurrency(goldStored, 0)); //0 Disables game object in overworld
        }
        else if(state == BattleState.LOST)
        {
            dialogueText.text = playerUnit.unitName + "'s party is defeated!";
            StartCoroutine(Exit(1)); //1 Game overs
            //StartCoroutine(HandleCurrency(-(FindObjectOfType<GameManager>().currentGold / 2), 1)); //1 Does not disable game object in overworld
        }
        else if (state == BattleState.RUN)
        {
            dialogueText.text = playerUnit.unitName + "'s party flees from the encounter!";
            PersistBattleStats();
            ES3.Save("activePartyUnits", playerUnits);
            StartCoroutine(Exit(2)); //2 Makes game object uninteractable for a time so that encounter does not immediately start again
        }
    }

    void PersistBattleStats()
    {
        for (int i = 0; i < playerUnits.Length; i++)
        {
            if(playerUnits[i] != null)
            {
                FindObjectOfType<GameManager>().PersistBattleInitialization(i, playerUnits[i]);
            }
        }
    }

    IEnumerator HandleCurrency(int df, int disableGOState)
    {
        yield return new WaitForSeconds(waitTime);
        FindObjectOfType<GameManager>().AddGold(df);
        if(df > 0)
        {
            dialogueText.text = playerUnit.unitName + "'s party obtained " + df + " daetral flux!";
        }
        else
        {
            df = -df;
            dialogueText.text = playerUnit.unitName + "'s party dropped " + df + " daetral flux in their retreat!";
        }
        StartCoroutine(Exit(disableGOState));
    }

    IEnumerator Exit(int disableGOState)
    {
        yield return new WaitForSeconds(waitTime);
        if(disableGOState == 1)
        {
            dialogueText.text = "<color=red>GAME OVER</color>";
            yield return new WaitForSeconds(waitTime);
        }
        FindObjectOfType<GameManager>().EndBattle(disableGOState);
    }


    /* We can utilize the below function structure to easily implement 3v3 combat
     * Firstly, we regen pips for the entire party
     * Then, we must refactor PlayerTurn to take an integer parameter "slot" which determines the slot we're looking at in the player party array
     * When we run this method, PlayerTurn(slot) sets playerUnit to PartyUnits[slot]
     * This means we have to store current slot as an instance variable in BattleSystem
     * On end turn, we add some logic stating that if currentPartySlot < 2, PlayerTurn(currentPartySlot + 1). Else, EnemyPartyBegin().
     */
    void PlayerPartyBegin()
    {
        state = BattleState.PLAYERTURN;
        for (int i = 0; i < playerUnits.Length; i++)
        {
            if(playerUnits[i] != null)
            {
                playerUnits[i].RegenPips();
                playerUnits[i].statusesAppliedOnTurn = false;
            }
        }
        currentPartySlot = 0;
        StartCoroutine(PlayerTurn(currentPartySlot));
    }

    IEnumerator PlayerTurn(int slot)
    {
        UpdateAllBuffs();
        PlayerInitialMenu();
        if (slot < playerUnits.Length && playerUnits[slot] != null)
        {
            playerUnit = playerUnits[slot];
            camController.Reset(playerBattleStations[currentPartySlot], false, playerUnits[currentPartySlot]);

            //HANDLE STATUS APPLICATION HERE, MAKING SURE TO UPDATE THE STATUSAPPLIEDONTURN VARIABLE - WILL HAVE TO PASS TO AN IENUMERATOR
            if (!playerUnit.isDead && playerUnit.HasStatus() && !playerUnit.statusesAppliedOnTurn)
            {
                StartCoroutine(ApplyStatuses(playerUnit, playerHUDs[slot], 0));
                playerUnit.statusesAppliedOnTurn = true;
                yield return new WaitForSeconds(playerUnit.GetStatusWaitTime());
                if (!PlayerPartyIsAlive())
                {
                    state = BattleState.LOST;
                    EndBattle();
                    yield break;
                }
            }

            if (playerUnit.isDead || playerUnit.IsAsleep())
            {
                currentPartySlot++;
                StartCoroutine(PlayerTurn(currentPartySlot));
                yield break;
            }

            dialogueText.text = "What will " + playerUnit.unitName + " do?";
            UpdateHUDs(true);
            state = BattleState.PLAYERTURN;
        }
        else
        {
            EnemyPartyBegin();
        }
    }

    public void OnSkillButton() //In initial menu
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }

        PlayerSkillMenu(playerUnit);
    }

    public void OnSkillSelectButton(int slot)
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        desiredSkillSlot = slot;

        StartCoroutine(PlayerSkill(slot));
    }

    public void OnItemButton()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }

        PlayerItemMenu();
    }

    public void OnItemSelectButton(int slot)
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        desiredInventorySlot = slot + (inventory.currentPage * 5);
        StartCoroutine(PlayerItem(desiredInventorySlot));
    }

    public void OnChangePageButton(bool left)
    {
        inventory.TurnPage(left);
        PlayerItemMenu();
    }

    public void OnChangeSidesButton()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        targetingPartySlot = !targetingPartySlot;

        TargetSelectMenu(targetingPartySlot);
    }

    public void OnTargetSelectButton(int targetSlot)
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }

        desiredTargetSlot = targetSlot;
        switch (targetingPartySlot)
        {
            case true:
                enemyUnit = playerUnits[targetSlot]; break;
            case false:
                enemyUnit = enemyUnits[targetSlot]; break;
        }

        switch (desiredAction)
        {
            case DesiredAction.SKILL:
                StartCoroutine(PlayerSkillExecute(desiredSkillSlot));
                break;
            case DesiredAction.ITEM:
                StartCoroutine(PlayerItemExecute(desiredInventorySlot));
                break;

        }
    }

    public void OnBackButton()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }

        if(itemMenu.activeSelf || skillMenu.activeSelf)
        {
            PlayerInitialMenu();
        }
        else if(targetSelectMenu.activeSelf)
        {
            switch(desiredAction)
            {
                case DesiredAction.SKILL:
                    PlayerSkillMenu(playerUnit);
                    break;
                case DesiredAction.ITEM:
                    PlayerItemMenu();
                    break;
            }

            camController.Reset(playerBattleStations[currentPartySlot], false, playerUnits[currentPartySlot]);
        }
    }

    public void OnGuardButton()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }

        camController.Reset(playerBattleStations[currentPartySlot], false, playerUnits[currentPartySlot]);
        StartCoroutine(PlayerGuard());
    }

    public void OnRunButton()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        state = BattleState.RUN;

        camController.Reset(playerBattleStations[currentPartySlot], false, playerUnits[currentPartySlot]);
        StartCoroutine(PlayerRun());
    }

    public void OnEndTurnButton()
    {
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }
        currentPartySlot++;

        StartCoroutine(PlayerTurn(currentPartySlot));
    }

    /*void OnApplicationQuit() //Remove this functionality later!
    {
        for(int i = 0; i < playerUnits.Length; i++)
        {
            if(playerUnits[i] != null)
            {
                playerUnits[i].Initialize(playerPartyPrefabs[i].GetComponent<Unit>());
            }
        }
        ES3.Save("activePartyUnits", playerUnits);
        Debug.Log("Application has quit!");
    }*/
}
