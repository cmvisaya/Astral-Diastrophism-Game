using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItem : MonoBehaviour
{
    public Item item;
    public int newBuyPrice = -1;
    public int newSellPrice = -1;
    public int stock = -1;

    void Start()
    {
        if(newBuyPrice >= 0)
        {
            item.buyPrice = newBuyPrice;
        }

        if(newSellPrice >= 0)
        {
            item.sellPrice = newSellPrice;
        }
    }
}
