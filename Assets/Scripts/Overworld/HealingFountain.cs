using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealingFountain : MonoBehaviour
{
    public TextMeshProUGUI dfText;
    public TextMeshProUGUI UIText;
    public TextMeshProUGUI prompterText;

    private bool interactable = false;
    
    public GameObject UI;
    public GameObject initialMenu;
    public TextMeshProUGUI[] currentMenuTexts;
    
    public int currentMenuItem;

    private bool m_isAxisInUse;

    void Start()
    {
        FindObjectOfType<GameManager>().inShop = false;
        UI.SetActive(false);

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
            Unit toHeal = FindObjectOfType<GameManager>().activePartyUnits[currentMenuItem];
            if (toHeal != null)
            {
                int cost = (toHeal.maxHealth - toHeal.currentHealth) + (toHeal.maxMana - toHeal.currentMana);
                if (cost > FindObjectOfType<GameManager>().currentGold) { cost = FindObjectOfType<GameManager>().currentGold; }
                UIText.text = cost + " DF\n\n" +
                    toHeal.unitName + "\n\n" +
                    "HP: " + toHeal.currentHealth + "/" + toHeal.maxHealth + "\n" +
                    "MP: " + toHeal.currentMana + "/" + toHeal.maxMana;
            }
            else
            {
                UIText.text = "Select a unit to heal.";
            }

            if (Input.GetButtonDown("Select"))
            {
                if (toHeal != null)
                {
                    while(FindObjectOfType<GameManager>().currentGold > 0)
                    {
                        if(toHeal.currentHealth != toHeal.maxHealth)
                        {
                            toHeal.currentHealth++;
                            FindObjectOfType<GameManager>().currentGold--;
                        }
                        else if(toHeal.currentMana != toHeal.maxMana)
                        {
                            toHeal.currentMana++;
                            FindObjectOfType<GameManager>().currentGold--;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            if (Input.GetButtonDown("Cancel") && UI.activeSelf)
            {
                FindObjectOfType<GameManager>().inShop = false;
                UI.SetActive(false);
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
        currentMenuItem = 0;
        prompterText.text = "Heal who?";
        ResetButtonFonts(currentMenuTexts);
        DisplayInitialMenu();
    }

    

    void DisplayInitialMenu()
    {
        for (int i = 0; i < FindObjectOfType<GameManager>().activePartyUnits.Length; i++)
        {
            if (FindObjectOfType<GameManager>().activePartyUnits[i] != null)
            {
                currentMenuTexts[i].text = FindObjectOfType<GameManager>().activePartyUnits[i].unitName;
            }
            else
            {
                currentMenuTexts[i].text = "None";
            }
        }
    }

}
