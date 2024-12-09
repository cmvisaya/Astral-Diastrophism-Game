using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour //Attach this script to the dialogue box
{
    //WAIT TIMES: Slow = 0.05, Normal = 0.03, Fast = 0.01, Instant = 0

    public static DialogueManager Instance;

    public GameObject dialogueBox;
    public GameObject characterBox;
    public GameObject centeredBox;
    public GameObject selectionButtons;
    public GameObject imageOverlayGO;
    public GameObject deactivatableGO;
    public RawImage characterIcon;
    public RawImage imageOverlay;
    public TextMeshProUGUI characterDialogueText;
    public TextMeshProUGUI centeredText;
    public TextMeshProUGUI topSelectText;
    public TextMeshProUGUI bottomSelectText;

    [SerializeField] private TextMeshProUGUI currentTextBox;
    [SerializeField] private string textToWrite;

    private int currentChar;
    [SerializeField] private string currentText;
    [SerializeField] private float textWaitTime;

    private enum STATE { PRINTING, WAITING, SELECTING }

    [SerializeField] private STATE currentState;

    private bool centered;

    private string formattedSelectionString;
    private int buttonTextDelimIndex;
    private int buttonFilepathDelimIndex;
    private int filepathsDelimIndex;
    private string topFilepath;
    private string bottomFilepath;
    [SerializeField] private bool topSelected;
    private string replacementText = "";
    private bool m_isAxisInUse;

    public Texture2D[] characterIcons;
    public Texture2D[] imageOverlays;

    public string[] cutsceneFilepaths;
    public bool[] unreplayablePETPlayed = new bool[2];

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        //These lines are a test
        /* //<<< Open Block comment here
        textToWrite =
            "Hello, my name is <b>Lotus</b>, and I am the lead developer for <b>Astral Diastrophism</b>.; " +
            "I hope you enjoyed the demo, and even more so hope you'll look forward to the full release!; " +
            "<b>The game window will now close.</b>; " +
            "#2; " +
            " ";

        //MAKE SURE TO CHANGE THE BELOW LINE SO AS NOT TO OVERWRITE ALREADY CREATED FILES
        string path = "Assets/Cutscene Files/Text/End Demo.txt";
        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(textToWrite);
        writer.Close();
        */ //<<< Close Block Comment Here
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Hide Dialogue"))
        {
            deactivatableGO.SetActive(false);
        }
        else if (Input.GetButtonUp("Hide Dialogue"))
        {
            deactivatableGO.SetActive(true);
        }
        if (dialogueBox.activeSelf)
        {
            if (Input.GetButtonDown("Select"))
            {
                switch (currentState)
                {
                    case STATE.PRINTING:
                        if(textToWrite[currentChar] != ';')
                        {
                            int nearestEqualsIndex = textToWrite.Substring(currentChar).IndexOf('=');
                            int nearestSemicolonIndex = textToWrite.Substring(currentChar).IndexOf(';');
                            int nearestDelimiterIndex = Math.Min(nearestEqualsIndex, nearestSemicolonIndex);
                            if(nearestDelimiterIndex == -1)
                            {
                                if(nearestEqualsIndex != -1) { nearestDelimiterIndex = nearestEqualsIndex; }
                                else if (nearestSemicolonIndex != -1) { nearestDelimiterIndex = nearestSemicolonIndex; }
                                else { nearestDelimiterIndex = 1; }
                            }
                            string addedSubstring = textToWrite.Substring(currentChar, nearestDelimiterIndex - 1);
                            currentText += addedSubstring;
                            currentTextBox.text = currentText;
                            currentChar += addedSubstring.Length;
                        }
                        break;
                    case STATE.WAITING:
                        //FindObjectOfType<AudioManager>().PlaySoundEffect(0, 0.1f);
                        currentState = STATE.PRINTING;
                        replacementText = currentText;
                        currentText = "";
                        currentChar++;
                        break;
                    case STATE.SELECTING:
                        FindObjectOfType<AudioManager>().PlaySoundEffect(0, 0.1f);
                        StopCoroutine("Print Dialogue");
                        selectionButtons.SetActive(false);
                        if (topSelected) { InitiateDialogue(topFilepath); }
                        else { InitiateDialogue(bottomFilepath); }
                        break;

                }
            }

            switch (centered)
            {
                case true:
                    centeredBox.SetActive(true);
                    characterBox.SetActive(false);
                    currentTextBox = centeredText;
                    break;
                case false:
                    centeredBox.SetActive(false);
                    characterBox.SetActive(true);
                    currentTextBox = characterDialogueText;
                    break;
            }

            if (currentState == STATE.SELECTING)
            {
                switch (topSelected)
                {
                    case true:
                        topSelectText.fontStyle = FontStyles.Bold | FontStyles.SmallCaps;
                        bottomSelectText.fontStyle = FontStyles.SmallCaps;
                        break;
                    case false:
                        bottomSelectText.fontStyle = FontStyles.Bold | FontStyles.SmallCaps;
                        topSelectText.fontStyle = FontStyles.SmallCaps;
                        break;
                }

                var selectionAxis = Input.GetAxisRaw("Horizontal") - Input.GetAxisRaw("Vertical");
                if (selectionAxis != 0)
                {
                    if (m_isAxisInUse == false)
                    {
                        FindObjectOfType<AudioManager>().PlaySoundEffect(0, 0.1f);
                        topSelected = !topSelected;
                        m_isAxisInUse = true;
                    }
                }
                if (selectionAxis == 0)
                {
                    m_isAxisInUse = false;
                }
            }

            if (FindObjectOfType<PlayerController>() != null) { FindObjectOfType<PlayerController>().hasControl = false; }
        }
        else
        {
            if (FindObjectOfType<PlayerController>() != null && !FindObjectOfType<GameManager>().pausing) { FindObjectOfType<PlayerController>().hasControl = true; }
        }
    }

    IEnumerator PrintDialogue()
    {
        while(currentChar != textToWrite.Length)
        {
            if(currentState == STATE.PRINTING)
            {
                //Debug.Log(textToWrite[currentChar]);
                char c = textToWrite[currentChar];
                bool delimeterReached = c == ';' || c == '<' || c == '`' || c == '|' || c == '@' || c == '=' || c == '#';
                //Debug.Log(c + " | " + delimeterReached);
                while (delimeterReached)
                {
                    //These variables are used to iterate over numeric custom tags
                    int i;
                    string numericDelimString;
                    int parsedIndex;
                    switch (textToWrite[currentChar])
                    {
                        case '#': //RUN SCRIPT INSIDE THIS SCRIPT
                            numericDelimString = "";
                            i = 1;
                            if (!Char.IsDigit(textToWrite[currentChar + i]) && textToWrite[currentChar + i] != '-')
                            {
                                break;
                            }
                            while (Char.IsDigit(textToWrite[currentChar + i]) || textToWrite[currentChar + i] == '-')
                            {
                                numericDelimString += textToWrite[currentChar + i];
                                i++;
                            }
                            parsedIndex = Int32.Parse(numericDelimString);
                            RunDialogueScript(parsedIndex);
                            currentChar += i;
                            break;
                        case '=': //OPEN/CLOSE PET - NEGATIVE FOR CLOSE, POSITIVE FOR OPEN
                            numericDelimString = "";
                            i = 1;
                            if (!Char.IsDigit(textToWrite[currentChar + i]) && textToWrite[currentChar + i] != '-')
                            {
                                currentTextBox.text = "";
                                currentText = "";
                                currentChar++;
                                break;
                            }
                            while (Char.IsDigit(textToWrite[currentChar + i]) || textToWrite[currentChar + i] == '-')
                            {
                                numericDelimString += textToWrite[currentChar + i];
                                i++;
                            }
                            parsedIndex = Int32.Parse(numericDelimString);
                            if (parsedIndex <= 0)
                            {
                                parsedIndex = Math.Abs(parsedIndex);
                                unreplayablePETPlayed[parsedIndex] = false;
                            }
                            else
                            {
                                unreplayablePETPlayed[parsedIndex] = true;
                            }
                            currentChar += i;
                            break;
                        case ';': //TRANSITION TO WAITING FOR ANOTHER BOX
                            currentChar++;
                            currentState = STATE.WAITING;
                            break;
                        case '<': //SKIP RICH TEXT TAGS
                            if (textToWrite[currentChar + 1] != '-')
                            {
                                int indexOfDelimiter = textToWrite.Substring(currentChar).IndexOf('>');
                                currentText += textToWrite.Substring(currentChar, indexOfDelimiter);
                                currentChar += indexOfDelimiter;
                                break;
                            }
                            numericDelimString = textToWrite.Substring(currentChar + 2);
                            InitiateDialogue(numericDelimString);
                            break;
                        case '`': //TOGGLE CENTERED AND CHANGE OBJECTIVE - OBJECTIVE INDEX OF 0 DEACTIVATES OBJECTIVE
                            numericDelimString = "";
                            i = 1;
                            if (!Char.IsDigit(textToWrite[currentChar + i]))
                            {
                                characterDialogueText.text = "";
                                centeredText.text = "";
                                currentChar++;
                                centered = !centered;
                                break;
                            }
                            while (Char.IsDigit(textToWrite[currentChar + i]))
                            {
                                numericDelimString += textToWrite[currentChar + i];
                                i++;
                            }
                            parsedIndex = Int32.Parse(numericDelimString);
                            if (parsedIndex >= 0)
                            {
                                FindObjectOfType<ObjectiveManager>().SwapObjective(parsedIndex);
                            }
                            currentChar += i;
                            break;
                        case '|': //CHANGE CHARACTER ICON
                            {
                                numericDelimString = "";
                                i = 1;
                                while (Char.IsDigit(textToWrite[currentChar + i]))
                                {
                                    numericDelimString += textToWrite[currentChar + i];
                                    i++;
                                }
                                parsedIndex = Int32.Parse(numericDelimString);
                                characterIcons[parsedIndex].Apply();
                                characterIcon.texture = characterIcons[parsedIndex];
                                currentChar += i;
                                break;
                            }
                        case '@': //INITIATE SELECTION AND IMAGE OVERLAY - IMAGE OVERLAY INDEX OF 0 DEACTIVATES OVERLAY
                            {
                                numericDelimString = "";
                                i = 1;
                                if(!Char.IsDigit(textToWrite[currentChar + i]))
                                {
                                    currentState = STATE.SELECTING;
                                    currentChar += 2;
                                    InitializeSelection();
                                    break;
                                }
                                while (Char.IsDigit(textToWrite[currentChar + i]))
                                {
                                    numericDelimString += textToWrite[currentChar + i];
                                    i++;
                                    Debug.Log("IMAGE STRING" + numericDelimString);
                                }
                                parsedIndex = Int32.Parse(numericDelimString);
                                if (parsedIndex > 0)
                                {
                                    imageOverlays[parsedIndex].Apply();
                                    imageOverlay.texture = imageOverlays[parsedIndex];
                                    imageOverlayGO.SetActive(true);
                                }
                                else
                                {
                                    imageOverlayGO.SetActive(false);
                                }
                                currentChar += i;
                                break;
                            }
                    }
                    yield return new WaitForSeconds(textWaitTime);
                    c = textToWrite[currentChar];
                    delimeterReached = c == ';' || c == '<' || c == '`' || c == '|' || c == '@' || c == '=' || c == '#';
                }

                if (currentState == STATE.PRINTING) {
                    currentText += textToWrite[currentChar];
                    currentTextBox.text = currentText;
                    currentChar++;
                }

                if (currentChar > textToWrite.Length)
                {
                    currentChar = textToWrite.Length;
                }
                yield return new WaitForSeconds(textWaitTime);
            }
            else if (currentState == STATE.WAITING)
            {
                yield return null;
            }
            else //Selecting
            {
                yield return null;
            }
        }
        dialogueBox.SetActive(false);
    }

    public void InitiateDialogue(string filePath)
    {
        StreamReader reader = new StreamReader(filePath);
        textToWrite = reader.ReadToEnd();
        reader.Close();

        dialogueBox.SetActive(true);
        //FindObjectOfType<PlayerController>().anim.SetBool("isGrounded", true);
        //FindObjectOfType<PlayerController>().anim.SetBool("isSliding", false);
        FindObjectOfType<PlayerController>().anim.SetFloat("Speed", 0);
        currentChar = 0;
        currentState = STATE.PRINTING;

        //ASSUME THE DIALOGUE BEGINS CENTERED INSTEAD OF AS CHARACTER DIALOGUE
        centered = true;
        centeredBox.SetActive(true);
        characterBox.SetActive(false);
        currentTextBox = centeredText;
        centeredText.text = "";
        characterDialogueText.text = "";
        currentText = "";

        StartCoroutine(PrintDialogue());
    }

    public void InitiateDialogue() //THIS IS A TEST METHOD THAT HARDCODES TEXTTOWRITE INSTEAD OF READING FROM A FILE
    {
        textToWrite =
            "Hello, my name is <b>Lotus</b>, and I am the lead developer for <b>Astral Diastrophism</b>.; " +
            "I hope you enjoyed the demo, and even more so hope you'll look forward to the full release!; " +
            "<b>The game window will now close.</b>; " +
            "#2; " +
            " ";

        dialogueBox.SetActive(true);
        //FindObjectOfType<PlayerController>().anim.SetBool("isGrounded", true);
        //FindObjectOfType<PlayerController>().anim.SetBool("isSliding", false);
        FindObjectOfType<PlayerController>().anim.SetFloat("Speed", 0);
        currentChar = 0;
        currentState = STATE.PRINTING;

        //ASSUME THE DIALOGUE BEGINS CENTERED INSTEAD OF AS CHARACTER DIALOGUE
        centered = true;
        centeredBox.SetActive(true);
        characterBox.SetActive(false);
        currentTextBox = centeredText;
        centeredText.text = "";
        characterDialogueText.text = "";

        StartCoroutine(PrintDialogue());
    }

    void InitializeSelection()
    {
        currentTextBox.text = replacementText;
        formattedSelectionString = textToWrite.Substring(currentChar);
        buttonTextDelimIndex = formattedSelectionString.IndexOf('|');
        buttonFilepathDelimIndex = formattedSelectionString.IndexOf('@');
        filepathsDelimIndex = formattedSelectionString.IndexOf(';');
        topSelectText.text = formattedSelectionString.Substring(1, buttonTextDelimIndex - 1);
        bottomSelectText.text = formattedSelectionString.Substring(buttonTextDelimIndex + 2, buttonFilepathDelimIndex - buttonTextDelimIndex - 2);
        topFilepath = formattedSelectionString.Substring(buttonFilepathDelimIndex + 2, filepathsDelimIndex - buttonFilepathDelimIndex - 2);
        bottomFilepath = formattedSelectionString.Substring(filepathsDelimIndex + 2, formattedSelectionString.Length - filepathsDelimIndex - 2);
        Debug.Log(topFilepath);
        Debug.Log(bottomFilepath);
        selectionButtons.SetActive(true);
        topSelected = true;
    }

    public void Reset()
    {
        Destroy(gameObject);
    }

    public void PlayCutscene(int cutsceneID)
    {
        InitiateDialogue(cutsceneFilepaths[cutsceneID]);
    }

    public void RunDialogueScript(int scriptID)
    {
        switch (scriptID)
        {
            case 0: DS0(); break;
            case 1: DS1(); break;
            case 2: DS2(); break;
            case 3: DS3(); break;
            default: Debug.Log("That script does not exist!"); break;
        }
    }

    public void DS0() //Add Koki to party
    {
        Debug.Log("We're supposed to add Koki to the party here.");
        FindObjectOfType<GameManager>().SwapActivePartySlot(1, 1);
    }

    public void DS1()
    {
        Debug.Log("We're supposed to heal the party here.");
        FindObjectOfType<GameManager>().HealPartyMemberHP(0, 1000);
        FindObjectOfType<GameManager>().HealPartyMemberMana(0, 1000);
    }

    public void DS2()
    {
        Application.Quit();
    }

    [SerializeField] private GameObject[] DS3EncounterPrefabs;
    public void DS3()
    {
        Debug.Log("RUN A FORCED COMBAT SCENARIO HERE.");
        FindObjectOfType<GameManager>().battlesInescapable = true;
        FindObjectOfType<GameManager>().InitiateBattle("Huosen Battleground", SceneManager.GetActiveScene().name, -1, Vector3.zero, DS3EncounterPrefabs);
        FindObjectOfType<AudioManager>().StopAll();
        FindObjectOfType<AudioManager>().PlayBGM(0, 0.75f);
        unreplayablePETPlayed[7] = false;
    }
}
