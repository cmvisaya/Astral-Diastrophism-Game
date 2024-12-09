using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class SmallHPPot : Item
{
    // Start is called before the first frame update
    void Start()
    {
        itemName = "Life Drop (S)";
        itemDescription = "Restores a small amount of HP to the target.";
        pipCost = 1;
        targetingMode = TargetingMode.SINGLE;
        animID = 1;
        buyPrice = 30;
        sellPrice = 10;
    }

    override public void UseItem(Unit user, Unit target, TextMeshProUGUI dialogueText, BattleHUD hud, BattleHUD userHUD)
    {
        {
            randomDeviation = CalculateRandomDeviation();

            int healAmount = (int)(target.maxHealth * .33 * randomDeviation);

            target.Heal(healAmount);

            hud.SetDeltaHealth(healAmount);
            dialogueText.text = user.unitName + " uses a " + itemName + "!";
        }
    }
}
