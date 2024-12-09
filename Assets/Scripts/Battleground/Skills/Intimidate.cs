using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class Intimidate : Skill
{
    // Start is called before the first frame update
    void Start()
    {
        skillName = "Intimidate";
        skillDaetra = -1;
        manaCost = 10;
        targetingMode = TargetingMode.SINGLE;
        animID = 1;
        skillDescription = "Slightly lowers the target's attack.";
    }

    override public void UseSkill(Unit user, Unit target, TextMeshProUGUI dialogueText, BattleHUD hud, BattleHUD userHUD)
    {
        {
            target.ApplyBuff(0, -1);

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
