using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class BasicRessurection : Skill
{
    // Start is called before the first frame update
    void Start()
    {
        skillName = "Basic Resurrection";
        skillDaetra = 2;
        manaCost = 30;
        targetingMode = TargetingMode.SINGLE;
        canTargetDead = true;
        animID = 1;
        skillDescription = "Revives an incapacitated target. This skill has no effect on targets that aren't incapacitated.";
    }

    override public void UseSkill(Unit user, Unit target, TextMeshProUGUI dialogueText, BattleHUD hud, BattleHUD userHUD)
    {
        {
            randomDeviation = CalculateRandomDeviation();

            int healAmount = (int)(target.maxHealth * .2 * randomDeviation);

            if(target.currentHealth <= 0)
            {
                target.Heal(healAmount);
                hud.SetDeltaHealth(healAmount);
            }

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
