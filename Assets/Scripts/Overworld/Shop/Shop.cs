using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Shop : MonoBehaviour
{
    public TextMeshProUGUI dfText;
    public TextMeshProUGUI descriptionText;

    public GameObject[] shopInventoryPrefabs;
    public Item[] shopInventory;
    public Item[] inventoryToDisplay;
    public int lowerInventoryDisplayBound = 0;

    private bool interactable = false;
    public GameObject shopUI;
    public GameObject initialMenu;
    public TextMeshProUGUI[] initialMenuButtonTexts;
    public GameObject buyMenu;
    public TextMeshProUGUI[] buyMenuButtonTexts;
    public GameObject sellMenu;
    public TextMeshProUGUI[] sellMenuButtonTexts;
    public int currentMenuItem;
    public TextMeshProUGUI[] currentMenuTexts;

    private bool m_isAxisInUse;

    void Awake()
    {
        shopInventory = new Item[shopInventoryPrefabs.Length];
        for (int i = 0; i < shopInventoryPrefabs.Length; i++)
        {
            shopInventory[i] = Instantiate(shopInventoryPrefabs[i], gameObject.transform).GetComponent<ShopItem>().item;
        }
    }

    void Start()
    {
        FindObjectOfType<GameManager>().inShop = false;
        shopUI.SetActive(false);
        descriptionText.text = "";

    }

    void Update()
    {
        if(shopUI.activeSelf)
        {
            dfText.text = "Daetral Flux: " + FindObjectOfType<GameManager>().currentGold;
        }

        if (interactable && !shopUI.activeSelf && Input.GetButtonDown("Select"))
        {
            InitialMenu();
            FindObjectOfType<GameManager>().inShop = true;
            shopUI.SetActive(true);
        }
        else if (initialMenu.activeSelf && shopUI.activeSelf && FindObjectOfType<GameManager>().inShop)
        {
            if (Input.GetButtonDown("Select") && shopUI.activeSelf)
            {
                switch(currentMenuItem)
                {
                    case 0: BuyMenu(); break;
                    case 1: SellMenu(); break;
                    case 2: FindObjectOfType<GameManager>().inShop = false; shopUI.SetActive(false); break;
                }
            }
        }
        else if (buyMenu.activeSelf && shopUI.activeSelf && FindObjectOfType<GameManager>().inShop)
        {
            int slot = currentMenuItem + lowerInventoryDisplayBound;
            Item toBuy = inventoryToDisplay[slot];
            if(toBuy != null) { descriptionText.text = toBuy.itemDescription; }
            else { descriptionText.text = ""; }
            if (Input.GetButtonDown("Select"))
            {
                if(FindObjectOfType<GameManager>().currentGold >= toBuy.buyPrice)
                {
                    FindObjectOfType<GameManager>().currentGold -= toBuy.buyPrice;
                    Backpack bp = FindObjectOfType<Backpack>();
                    bp.items[bp.FindEmptySlot()] = Instantiate(shopInventoryPrefabs[slot], bp.transform).GetComponent<ShopItem>().GetComponent<Item>();
                }
            }

            if(Input.GetButtonDown("Cancel"))
            {
                InitialMenu();
            }
        }
        else if (sellMenu.activeSelf && shopUI.activeSelf && FindObjectOfType<GameManager>().inShop)
        {
            int slot = currentMenuItem + lowerInventoryDisplayBound;
            Item toSell = inventoryToDisplay[slot];
            if (toSell != null) { descriptionText.text = toSell.itemDescription; }
            else { descriptionText.text = ""; }
            if (Input.GetButtonDown("Select"))
            {
                if(toSell != null)
                {
                    FindObjectOfType<GameManager>().currentGold += toSell.sellPrice;
                    Backpack bp = FindObjectOfType<Backpack>();
                    bp.items[slot] = null;
                    DisplayMenu();
                }
            }

            if (Input.GetButtonDown("Cancel"))
            {
                InitialMenu();
            }
        }

        if(shopUI.activeSelf && FindObjectOfType<GameManager>().inShop)
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
                            UpdateShopUI(false);
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
                            UpdateShopUI(true);
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
        if(other.tag == "Player")
        {
            interactable = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            interactable = false;
        }
    }

    void ResetButtonFonts()
    {
        for(int i = 0; i < currentMenuTexts.Length; i++)
        {
            currentMenuTexts[i].fontStyle = FontStyles.SmallCaps;
        }
    }

    void InitialMenu()
    {
        initialMenu.SetActive(true);
        buyMenu.SetActive(false);
        sellMenu.SetActive(false);
        currentMenuItem = 0;
        lowerInventoryDisplayBound = 0;
        currentMenuTexts = initialMenuButtonTexts;
        descriptionText.text = "";
        ResetButtonFonts();
    }

    void BuyMenu()
    {
        initialMenu.SetActive(false);
        buyMenu.SetActive(true);
        sellMenu.SetActive(false);
        currentMenuItem = 0;
        lowerInventoryDisplayBound = 0;
        currentMenuTexts = buyMenuButtonTexts;
        ResetButtonFonts();
        inventoryToDisplay = shopInventory;
        DisplayMenu();
    }

    void SellMenu()
    {
        initialMenu.SetActive(false);
        buyMenu.SetActive(false);
        sellMenu.SetActive(true);
        currentMenuItem = 0;
        lowerInventoryDisplayBound = 0;
        currentMenuTexts = sellMenuButtonTexts;
        ResetButtonFonts();
        inventoryToDisplay = FindObjectOfType<Backpack>().items;
        DisplayMenu();
    }

    void UpdateShopUI(bool left)
    {
        if(left)
        {
            lowerInventoryDisplayBound--;
            if(lowerInventoryDisplayBound < 0)
            {
                lowerInventoryDisplayBound = 0;
            }
        }
        else
        {
            lowerInventoryDisplayBound++;
            if(lowerInventoryDisplayBound >= inventoryToDisplay.Length - currentMenuTexts.Length)
            {
                lowerInventoryDisplayBound = inventoryToDisplay.Length - currentMenuTexts.Length;
            }
        }

        DisplayMenu();
    }

    void DisplayMenu()
    {
        for (int i = lowerInventoryDisplayBound; i < currentMenuTexts.Length + lowerInventoryDisplayBound; i++)
        {
            if (buyMenu.activeSelf)
            {
                if(inventoryToDisplay[i] != null)
                {
                    currentMenuTexts[i - lowerInventoryDisplayBound].text = inventoryToDisplay[i].itemName + "\n" + inventoryToDisplay[i].buyPrice + " DF";
                    Debug.Log(i);
                }
                else
                {
                    currentMenuTexts[i - lowerInventoryDisplayBound].text = "None";
                }
                
            }
            else if (sellMenu.activeSelf)
            {
                if(inventoryToDisplay[i] != null)
                {
                    currentMenuTexts[i - lowerInventoryDisplayBound].text = inventoryToDisplay[i].itemName + "\n" + inventoryToDisplay[i].sellPrice + " DF";
                }
                else
                {
                    currentMenuTexts[i - lowerInventoryDisplayBound].text = "None";
                }
            }
        }
    }
}
