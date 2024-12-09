using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class Lacerate : Skill
{
    // Start is called before the first frame update
    void Start()
    {
        skillName = "Lacerate";
        skillDaetra = -3;
        manaCost = 10;
        targetingMode = TargetingMode.SINGLE;
        animID = 1;
        skillDescription = "Inflicts BLEED on the target.";
    }

    override public void UseSkill(Unit user, Unit target, TextMeshProUGUI dialogueText, BattleHUD hud, BattleHUD userHUD)
    {
        {
            Bleed statusToInflict = target.gameObject.AddComponent<Bleed>() as Bleed;
            target.InflictStatus(statusToInflict);

            //Remove the below logic if this skill should not swap the character's Daetra
            if (Math.Sign(user.currentDaetra) != Math.Sign(skillDaetra))
            {
                user.currentDaetra *= -1;
            }

            userHUD.SetDeltaMana(-manaCost);
            dialogueText.text = user.unitName + " uses " + skillName + "!";
        }
    }

}
