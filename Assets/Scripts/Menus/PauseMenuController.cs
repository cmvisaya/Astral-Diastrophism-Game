using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance;

    public GameObject pauseMenuUI;

    public TextMeshProUGUI[] pauseMenuButtonTexts;
    public int currentItem;
    private bool m_isAxisInUse;
    private bool selectable = false;

    public GameObject[] activePartyStatBlocks = new GameObject[3];

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

    void Update()
    {
        if (pauseMenuUI.activeSelf)
        {
            pauseMenuButtonTexts[currentItem].fontStyle = FontStyles.Bold | FontStyles.SmallCaps;
            FindObjectOfType<GameManager>().interactable = false;
            FindObjectOfType<GameManager>().pausing = true;
            FindObjectOfType<PlayerController>().hasControl = false;

            if (Input.GetButtonDown("Select"))
            {
                switch (currentItem)
                {
                    case 0:
                        Application.Quit();
                        break;
                    case 1:
                        FindObjectOfType<GameManager>().interactable = true;
                        FindObjectOfType<GameManager>().pausing = false;
                        FindObjectOfType<PlayerController>().hasControl = true;
                        pauseMenuUI.SetActive(false);
                        break;
                }
            }

            if (selectable && Input.GetButtonDown("Escape") || Input.GetButtonDown("Cancel"))
            {
                FindObjectOfType<GameManager>().interactable = true;
                FindObjectOfType<GameManager>().pausing = false;
                FindObjectOfType<PlayerController>().hasControl = true;
                pauseMenuUI.SetActive(false);
            }

            var selectionAxis = Input.GetAxisRaw("Horizontal") - Input.GetAxisRaw("Vertical");
            if (selectionAxis != 0)
            {
                if (m_isAxisInUse == false)
                {
                    if (selectionAxis > 0)
                    {
                        //FindObjectOfType<AudioManager>().PlaySoundEffect(0, 0.1f);
                        pauseMenuButtonTexts[currentItem].fontStyle = FontStyles.SmallCaps;
                        currentItem++;
                        if (currentItem >= pauseMenuButtonTexts.Length)
                        {
                            currentItem = 0;
                        }
                    }
                    else
                    {
                        //FindObjectOfType<AudioManager>().PlaySoundEffect(0, 0.1f);
                        pauseMenuButtonTexts[currentItem].fontStyle = FontStyles.SmallCaps;
                        currentItem--;
                        if (currentItem < 0)
                        {
                            currentItem = pauseMenuButtonTexts.Length - 1;
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
        selectable = true;
    }

    public void Open()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        for (int i = 0; i < gm.activePartyUnits.Length; i++)
        {
            activePartyStatBlocks[i].GetComponent<PauseMenuPlayerStatHUD>().DisplayUnit(gm.activePartyUnits[i]);
        }
        selectable = false;
        pauseMenuButtonTexts[0].fontStyle = FontStyles.SmallCaps;
        pauseMenuButtonTexts[1].fontStyle = FontStyles.SmallCaps;
        currentItem = 0;
        pauseMenuUI.SetActive(true);
    }
}
