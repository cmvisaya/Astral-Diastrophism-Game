using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class SleepPowder : Item
{
    // Start is called before the first frame update
    void Start()
    {
        itemName = "Sleep Powder";
        itemDescription = "Inflicts SLEEP on the target.";
        pipCost = 1;
        targetingMode = TargetingMode.SINGLE;
        animID = 1;
        buyPrice = 200;
        sellPrice = 70;
    }

    override public void UseItem(Unit user, Unit target, TextMeshProUGUI dialogueText, BattleHUD hud, BattleHUD userHUD)
    {
        {
            Sleep statusToInflict = target.gameObject.AddComponent<Sleep>() as Sleep;
            target.InflictStatus(statusToInflict);

            dialogueText.text = user.unitName + " used some " + itemName + "!";
        }
    }
}
