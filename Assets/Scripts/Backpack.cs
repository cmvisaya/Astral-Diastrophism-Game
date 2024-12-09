using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Backpack : MonoBehaviour
{
    public static Backpack Instance;

    public Item[] items = new Item[50];
    public int currentPage = 0;

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

    public void TurnPage(bool left)
    {
        switch (left)
        {
            case true:
                {
                    currentPage--;
                    if (currentPage < 0)
                    {
                        currentPage = (items.Length / 5) - 1;
                    }
                    break;
                }
            case false:
                {
                    currentPage++;
                    if (currentPage > (items.Length / 5) - 1)
                    {
                        currentPage = 0;
                    }
                    break;
                }
        }

    }

    public int FindEmptySlot()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    public void AddItem(Item itemToAdd)
    {
        for(int i = 0; i < items.Length; i++)
        {
            if(items[i] == null)
            {
                items[i] = gameObject.AddComponent(typeof(Item)) as Item;
                break;
            }
        }
    }
}
