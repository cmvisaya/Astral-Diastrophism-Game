using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class HealingAura : Skill
{
    // Start is called before the first frame update
    void Start()
    {
        skillName = "Healing Aura";
        skillDaetra = 2;
        manaCost = 20;
        targetingMode = TargetingMode.PARTY;
        animID = 1;
        skillDescription = "Grants a small amount of healing to the targeted party.";
    }

    override public void UseSkill(Unit user, Unit target, TextMeshProUGUI dialogueText, BattleHUD hud, BattleHUD userHUD)
    {
        {
            randomDeviation = CalculateRandomDeviation();

            int healAmount = (int)(target.maxHealth * .2 * randomDeviation);

            target.Heal(healAmount);

            //Remove the below logic if this skill should not swap the character's Daetra
            if (Math.Sign(user.currentDaetra) != Math.Sign(skillDaetra))
            {
                user.currentDaetra *= -1;
            }

            hud.SetDeltaHealth(healAmount);
            hud.SetDeltaMana(-manaCost);
            dialogueText.text = user.unitName + " uses " + skillName + "!";
        }
    }

    override public void UseSkill(Unit user, Unit[] targetParty, TextMeshProUGUI dialogueText, BattleHUD[] huds, BattleHUD userHUD)
    {
        for (int i = 0; i < targetParty.Length; i++)
        {
            if (targetParty[i] != null && !targetParty[i].isDead)
            {
                UseSkill(user, targetParty[i], dialogueText, huds[i], userHUD);
            }
        }
    }
}
