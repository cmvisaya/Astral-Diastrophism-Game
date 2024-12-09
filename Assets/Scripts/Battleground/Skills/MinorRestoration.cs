using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class MinorRestoration : Skill
{
    // Start is called before the first frame update
    void Start()
    {
        skillName = "Minor Restoration";
        skillDaetra = 2;
        manaCost = 10;
        //targetingMode = TargetingMode.SELF;
        targetingMode = TargetingMode.SINGLE;
        animID = 1;
        skillDescription = "Grants the target a little bit of hp.";
    }

    override public void UseSkill(Unit user, Unit target, TextMeshProUGUI dialogueText, BattleHUD hud, BattleHUD userHUD)
    {
        {
            randomDeviation = CalculateRandomDeviation();

            //int healAmount = (int)(user.maxHealth * .33 * randomDeviation);
            int healAmount = (int)(target.maxHealth * .33 * randomDeviation);

            //user.Heal(healAmount);
            target.Heal(healAmount);

            //Remove the below logic if this skill should not swap the character's Daetra
            if (Math.Sign(user.currentDaetra) != Math.Sign(skillDaetra))
            {
                user.currentDaetra *= -1;
            }

            //userHUD.SetDeltaHealth(healAmount);
            hud.SetDeltaHealth(healAmount);
            userHUD.SetDeltaMana(-manaCost);
            dialogueText.text = user.unitName + " uses " + skillName + "!";
        }
    }
}
