using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DaetralIntegrator : MonoBehaviour
{
    public TextMeshProUGUI dfText;
    public TextMeshProUGUI prompterText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI nsdt;
    public TextMeshProUGUI rsdt;
    int cost = 100000000;
    public int lowerInventoryDisplayBound = 0;

    private bool interactable = false;
    private bool inLearnMenu = false;
    private int charSlot;
    private int desiredLearnSlot;
    public GameObject UI;
    public GameObject initialMenu;
    public TextMeshProUGUI[] initialMenuButtonTexts;
    public GameObject learnedMenu;
    public TextMeshProUGUI[] learnedSkillTexts;
    public GameObject replacedMenu;
    public TextMeshProUGUI[] replacedSkillTexts;
    public int currentMenuItem;
    public TextMeshProUGUI[] currentMenuTexts;

    private bool m_isAxisInUse;

    void Start()
    {
        FindObjectOfType<GameManager>().inShop = false;
        UI.SetActive(false);
        nsdt.text = "";
        rsdt.text = "";

    }

    void Update()
    {
        if (UI.activeSelf)
        {
            dfText.text = "Daetral Flux: " + FindObjectOfType<GameManager>().currentGold;
        }

        if (interactable && !UI.activeSelf && Input.GetButtonDown("Select"))
        {
            InitialMenu();
            FindObjectOfType<GameManager>().inShop = true;
            UI.SetActive(true);
        }
        else if (initialMenu.activeSelf && UI.activeSelf && FindObjectOfType<GameManager>().inShop)
        {
            if (Input.GetButtonDown("Select"))
            {
                if(FindObjectOfType<GameManager>().activePartyUnits[currentMenuItem] != null)
                {
                    charSlot = currentMenuItem;
                    LearnedMenu();
                }
            }

            if (Input.GetButtonDown("Cancel") && UI.activeSelf)
            {
                FindObjectOfType<GameManager>().inShop = false;
                UI.SetActive(false);
            }
        }
        else if (learnedMenu.activeSelf && UI.activeSelf && FindObjectOfType<GameManager>().inShop && inLearnMenu)
        {
            int[] levels = FindObjectOfType<GameManager>().activePartyUnits[charSlot].learnedSkills.levelsLearnedAt;
            int slot = lowerInventoryDisplayBound + currentMenuItem;
            if (slot < levels.Length && levels[slot] <= FindObjectOfType<GameManager>().activePartyUnits[charSlot].unitLevel)
            {
                cost = FindObjectOfType<GameManager>().activePartyUnits[charSlot].learnedSkills.levelsLearnedAt[slot] * 50;
                costText.text = cost + " DF\n--------->";
                nsdt.text = FindObjectOfType<GameManager>().activePartyUnits[charSlot].learnedSkills.skillPool[slot].skillDescription;
            }
            else
            {
                cost = 100000000;
                costText.text = "";
                nsdt.text = "";
            }

            if (Input.GetButtonDown("Select"))
            {
                Unit learner = FindObjectOfType<GameManager>().activePartyUnits[charSlot];
                desiredLearnSlot = lowerInventoryDisplayBound + currentMenuItem;
                if(desiredLearnSlot < learner.GetLearnableArray().Length)
                {
                    if (learner.SkillKnown(learner.GetLearnableArray()[desiredLearnSlot].skillName))
                    {
                        prompterText.text = learner.unitName + " already knows " + learner.GetLearnableArray()[desiredLearnSlot].skillName;
                    }
                    else if (cost > FindObjectOfType<GameManager>().currentGold)
                    {
                        prompterText.text = "You do not have enough Daetral Flux to learn that skill!";
                    }
                    else
                    {
                        ReplacedMenu();
                    }
                }
            }

            if (Input.GetButtonDown("Cancel"))
            {
                InitialMenu();
            }
        }
        else if (replacedMenu.activeSelf && UI.activeSelf && FindObjectOfType<GameManager>().inShop)
        {
            Skill toReplace = FindObjectOfType<GameManager>().activePartyUnits[charSlot].skills[currentMenuItem + 1];
            if(toReplace != null) { rsdt.text = FindObjectOfType<GameManager>().activePartyUnits[charSlot].skills[currentMenuItem + 1].skillDescription; }
            else { rsdt.text = ""; }
            if (Input.GetButtonDown("Select"))
            {
                FindObjectOfType<GameManager>().activePartyUnits[charSlot].skills[currentMenuItem + 1] = 
                    FindObjectOfType<GameManager>().activePartyUnits[charSlot].GetLearnableArray()[desiredLearnSlot]; //Skills learned not persisting into battleground
                FindObjectOfType<GameManager>().currentGold -= cost;
                LearnedMenu();
            }

            if (Input.GetButtonDown("Cancel"))
            {
                LearnedMenu();
            }
        }

        if (UI.activeSelf && FindObjectOfType<GameManager>().inShop)
        {
            currentMenuTexts[currentMenuItem].fontStyle = FontStyles.Bold | FontStyles.SmallCaps;
            var selectionAxis = Input.GetAxisRaw("Horizontal") - Input.GetAxisRaw("Vertical");
            if (selectionAxis != 0)
            {
                if (m_isAxisInUse == false)
                {
                    if (selectionAxis > 0)
                    {
                        if (currentMenuItem != currentMenuTexts.Length - 1) { currentMenuTexts[currentMenuItem].fontStyle = FontStyles.SmallCaps; }
                        currentMenuItem++;
                        if (currentMenuItem >= currentMenuTexts.Length)
                        {
                            currentMenuItem = currentMenuTexts.Length - 1;
                            if(inLearnMenu)
                            {
                                lowerInventoryDisplayBound++;
                                DisplayLearnedMenu();
                            }
                        }
                    }
                    else
                    {
                        //FindObjectOfType<AudioManager>().PlaySoundEffect(0, 0.1f);
                        if (currentMenuItem != 0) { currentMenuTexts[currentMenuItem].fontStyle = FontStyles.SmallCaps; }
                        currentMenuItem--;
                        if (currentMenuItem < 0)
                        {
                            currentMenuItem = 0;
                            if(inLearnMenu)
                            {
                                lowerInventoryDisplayBound--;
                                DisplayLearnedMenu();
                            }
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
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            interactable = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            interactable = false;
        }
    }

    void ResetButtonFonts(TextMeshProUGUI[] menuTexts)
    {
        for (int i = 0; i < menuTexts.Length; i++)
        {
            menuTexts[i].fontStyle = FontStyles.SmallCaps;
        }
    }

    void InitialMenu()
    {
        initialMenu.SetActive(true);
        learnedMenu.SetActive(false);
        replacedMenu.SetActive(false);
        inLearnMenu = false;
        currentMenuItem = 0;
        lowerInventoryDisplayBound = 0;
        currentMenuTexts = initialMenuButtonTexts;
        prompterText.text = "Whose skills do you want to manage?";
        costText.text = "";
        nsdt.text = "";
        rsdt.text = "";
        ResetButtonFonts(initialMenuButtonTexts);
        DisplayInitialMenu();
    }

    void LearnedMenu()
    {
        initialMenu.SetActive(false);
        learnedMenu.SetActive(true);
        replacedMenu.SetActive(true);
        inLearnMenu = true;
        currentMenuItem = 0;
        lowerInventoryDisplayBound = 0;
        currentMenuTexts = learnedSkillTexts;
        prompterText.text = "Select which skill " + FindObjectOfType<GameManager>().activePartyUnits[charSlot].unitName + " should learn";
        costText.text = "";
        rsdt.text = "";
        ResetButtonFonts(learnedSkillTexts);
        ResetButtonFonts(replacedSkillTexts);
        DisplayLearnedMenu();
        DisplayReplacedMenu();
    }

    void ReplacedMenu()
    {
        initialMenu.SetActive(false);
        learnedMenu.SetActive(true);
        replacedMenu.SetActive(true);
        inLearnMenu = false;
        currentMenuItem = 0;
        lowerInventoryDisplayBound = 0;
        currentMenuTexts = replacedSkillTexts;
        prompterText.text = "Select a skill for " + FindObjectOfType<GameManager>().activePartyUnits[charSlot].unitName + " to replace";
        DisplayReplacedMenu();
    }

    void DisplayInitialMenu()
    {
        for (int i = 0; i < FindObjectOfType<GameManager>().activePartyUnits.Length; i++)
        {
            if(FindObjectOfType<GameManager>().activePartyUnits[i] != null)
            {
                initialMenuButtonTexts[i].text = FindObjectOfType<GameManager>().activePartyUnits[i].unitName;
            }
            else
            {
                initialMenuButtonTexts[i].text = "None";
            }
        }
    }

    void DisplayLearnedMenu()
    {
        Skill[] learned = FindObjectOfType<GameManager>().activePartyUnits[charSlot].GetLearnableArray();
        Debug.Log("AAAA" + learned.Length);
        if (lowerInventoryDisplayBound < 0) { lowerInventoryDisplayBound = 0; }
        else if (learned.Length < learnedSkillTexts.Length && lowerInventoryDisplayBound > 0) { lowerInventoryDisplayBound = 0; }
        else if (learned.Length > learnedSkillTexts.Length && lowerInventoryDisplayBound >= learned.Length - learnedSkillTexts.Length) { lowerInventoryDisplayBound = learned.Length - learnedSkillTexts.Length; }
        for (int i = lowerInventoryDisplayBound; i < learnedSkillTexts.Length + lowerInventoryDisplayBound; i++)
        {
            if(i < learned.Length && learned[i] != null)
            {
                learnedSkillTexts[i - lowerInventoryDisplayBound].text = learned[i].skillName + "\n" + learned[i].manaCost + " MP";
            }
            else
            {
                learnedSkillTexts[i - lowerInventoryDisplayBound].text = "None";
            }
        }
    }

    void DisplayReplacedMenu()
    {
        for (int i = 0; i < replacedSkillTexts.Length; i++)
        {
            Skill skillToDisplay = FindObjectOfType<GameManager>().activePartyUnits[charSlot].skills[i + 1];
            if (skillToDisplay != null)
            {
                replacedSkillTexts[i].text = skillToDisplay.skillName + "\n" + skillToDisplay.manaCost + " MP";
            }
            else
            {
                replacedSkillTexts[i].text = "None";
            }
        }
    }
}
