using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject initialMenu;
    [SerializeField] private GameObject controlsMenu;

    [SerializeField] private TextMeshProUGUI[] initialMenuButtonTexts = new TextMeshProUGUI[4];

    private bool m_isAxisInUse;
    private TextMeshProUGUI[] currentMenuTexts;
    private int currentMenuItem;

    private bool pulsating = false;
    private bool enlarge = true;

    void Awake()
    {
        initialMenu.SetActive(true);
        controlsMenu.SetActive(false);
        currentMenuItem = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (initialMenu.activeSelf)
        {
            currentMenuTexts = initialMenuButtonTexts;
            if (Input.GetButtonDown("Select"))
            {
                switch (currentMenuItem)
                {
                    case 0:
                        FindObjectOfType<AudioManager>().PlaySoundEffect(0, 0.1f);
                        OnNewGameButton();
                        break;
                    case 1:
                        Debug.Log("CONTINUE NOT YET IMPLEMENTED");
                        //OnGuardButton();
                        break;
                    case 2:
                        Debug.Log("SETTINGS NOT YET IMPLEMENTED");
                        //OnItemButton();
                        break;
                    case 3:
                        FindObjectOfType<AudioManager>().PlaySoundEffect(0, 0.1f);
                        OpenControlsMenu();
                        break;
                    case 4:
                        Application.Quit();
                        break;
                }
            }
        }

        if (controlsMenu.activeSelf)
        {
            if (Input.GetButtonDown("Cancel"))
            {
                FindObjectOfType<AudioManager>().PlaySoundEffect(1, 0.1f);
                OpenInitialMenu(3);
            }
        }

        currentMenuTexts[currentMenuItem].fontStyle = FontStyles.Bold | FontStyles.SmallCaps;
        if(!pulsating)
        {
            StartCoroutine(PulsateText(currentMenuTexts[currentMenuItem], enlarge));
        }
        var selectionAxis = Input.GetAxisRaw("Horizontal") - Input.GetAxisRaw("Vertical");
        if (selectionAxis != 0)
        {
            if (m_isAxisInUse == false)
            {
                if (selectionAxis > 0)
                {
                    //FindObjectOfType<AudioManager>().PlaySoundEffect(0, 0.1f);
                    currentMenuTexts[currentMenuItem].fontStyle = FontStyles.SmallCaps;
                    currentMenuTexts[currentMenuItem].fontSize = 50;
                    currentMenuItem++;
                    if (currentMenuItem >= currentMenuTexts.Length)
                    {
                        currentMenuItem = 0;
                    }
                }
                else
                {
                    //FindObjectOfType<AudioManager>().PlaySoundEffect(0, 0.1f);
                    currentMenuTexts[currentMenuItem].fontStyle = FontStyles.SmallCaps;
                    currentMenuTexts[currentMenuItem].fontSize = 50;
                    currentMenuItem--;
                    if (currentMenuItem < 0)
                    {
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

    IEnumerator PulsateText(TextMeshProUGUI text, bool enlarge)
    {
        float pulsateTime = 0.05f;
        pulsating = true;
        switch (enlarge)
        {
            case true:
                yield return new WaitForSeconds(pulsateTime);
                text.fontSize = text.fontSize + 1;
                if(text.fontSize >= 60)
                {
                    enlarge = false;
                }
                break;
            case false:
                yield return new WaitForSeconds(pulsateTime);
                text.fontSize = text.fontSize - 1;
                if (text.fontSize <= 50)
                {
                    enlarge = true;
                }
                break;
        }
        this.enlarge = enlarge;
        pulsating = false;
    }

    void OnNewGameButton()
    {
        FindObjectOfType<GameManager>().StartNewGame();
    }
    
    void OpenControlsMenu()
    {
        initialMenu.SetActive(false);
        controlsMenu.SetActive(true);
    }

    void OpenInitialMenu(int currentMenuItem)
    {
        this.currentMenuItem = currentMenuItem; //We want to preserve the last menu item that was selected
        initialMenu.SetActive(true);
        controlsMenu.SetActive(false);
    }
}
