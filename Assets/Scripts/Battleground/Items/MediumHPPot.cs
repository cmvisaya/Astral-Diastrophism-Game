using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class MediumHPPot : Item
{
    // Start is called before the first frame update
    void Start()
    {
        itemName = "Life Drop (M)";
        itemDescription = "Restores a moderate amount of HP to the target.";
        pipCost = 1;
        targetingMode = TargetingMode.SINGLE;
        animID = 1;
        buyPrice = 100;
        sellPrice = 30;
    }

    override public void UseItem(Unit user, Unit target, TextMeshProUGUI dialogueText, BattleHUD hud, BattleHUD userHUD)
    {
        {
            randomDeviation = CalculateRandomDeviation();

            int healAmount = (int)(target.maxHealth * .50 * randomDeviation);

            target.Heal(healAmount);

            hud.SetDeltaHealth(healAmount);
            dialogueText.text = user.unitName + " uses a " + itemName + "!";
        }
    }
}
