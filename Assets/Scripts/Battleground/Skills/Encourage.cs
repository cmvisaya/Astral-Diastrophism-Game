using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class Encourage : Skill
{
    // Start is called before the first frame update
    void Start()
    {
        skillName = "Encourage";
        skillDaetra = 1;
        manaCost = 10;
        targetingMode = TargetingMode.SINGLE;
        animID = 1;
        skillDescription = "Slightly increases the target's defense.";
    }

    override public void UseSkill(Unit user, Unit target, TextMeshProUGUI dialogueText, BattleHUD hud, BattleHUD userHUD)
    {
        {
            target.ApplyBuff(1, 1);

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
