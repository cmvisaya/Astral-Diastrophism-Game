using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class ChaosPulse : Skill
{
    // Start is called before the first frame update
    void Start()
    {
        skillName = "Chaos Pulse";
        skillDaetra = -3;
        skillBasePower = 60;
        manaCost = 15;
        targetingMode = TargetingMode.SINGLE;
        animID = 1;
        skillDescription = "Base Power: " + skillBasePower + "\nGenerates a small shockwave of CHAOS energy from the user.";
    }

    override public void UseSkill(Unit user, Unit target, TextMeshProUGUI dialogueText, BattleHUD hud, BattleHUD userHUD)
    {
        {
            typeAdvantageMult = CalculateTypeAdvMult(skillDaetra, target.currentDaetra);
            contraryTypeMult = CalculateContraryTypeMult(skillDaetra, target.currentDaetra);
            sameTypeAttackBonus = CalculateSTAB(skillDaetra, user.currentDaetra);
            critBonus = CalculateCritBonus(user.critRatePhase);
            randomDeviation = CalculateRandomDeviation();

            int damage = (int)(Math.Floor(((skillBasePower + user.currentAtk) * typeAdvantageMult * contraryTypeMult * sameTypeAttackBonus * critBonus * randomDeviation) - (target.currentDef / 2)));

            //Remove or alter the below logic if a specific attack handles mana reclamation abnormally
            int manaReclaimed = (int)Math.Ceiling(damage * 0.5);
            target.IncrementMana(manaReclaimed);

            //Remove the below logic if a specific attack ignores guard.
            if (target.isGuarding)
            {
                damage = damage / 2;
                target.isGuarding = false;
            }

            //Remove the below logic if this skill should not swap the character's Daetra
            if (Math.Sign(user.currentDaetra) != Math.Sign(skillDaetra))
            {
                user.currentDaetra *= -1;
            }

            if (damage < 0)
            {
                damage = 0;
            }
            target.isDead = target.TakeDamage(damage);

            hud.SetDeltaHealth(-damage);
            hud.SetDeltaMana(manaReclaimed);
            userHUD.SetDeltaMana(-manaCost);
            dialogueText.text = user.unitName + " uses " + skillName + "!";
        }
    }

}
