using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum TargetingMode { SELF, SINGLE, PARTY }

public class Skill : MonoBehaviour
{
    public string skillName;
    public int skillDaetra;
    public int skillBasePower;
    public int manaCost;
    public TargetingMode targetingMode;
    public bool canTargetDead = false;
    public int animID = 0;
    public string skillDescription = "This skill has no description yet.";

    //May need a targeting mode variable for the sake of proper enemy targeting and also AOE

    public double typeAdvantageMult;
    public double contraryTypeMult;
    public double sameTypeAttackBonus;
    public double critBonus;
    public double randomDeviation;

    protected double CalculateTypeAdvMult(int skillDaetra, int targetDaetra) //Flow
    {
        int bonusFormulation = ((Mathf.Abs(skillDaetra) % 3) - (Mathf.Abs(targetDaetra) - 3)) % 3;
        if (bonusFormulation == 0) { FindObjectOfType<BattleSystem>().allyOnHitNotifTexts[2].text = ""; return 1; }
        else if (bonusFormulation == 1) { FindObjectOfType<BattleSystem>().allyOnHitNotifTexts[2].text = "<color=red>Flow vvv </color>"; return 0.85; }
        else if (bonusFormulation == 2) { FindObjectOfType<BattleSystem>().allyOnHitNotifTexts[2].text = "<color=#0d9d00>Flow ^^^</color>"; return 1.2; }
        else { return 1; }
    }

    protected double CalculateContraryTypeMult(int skillDaetra, int targetDaetra) //Contrary
    {
        int skillDirection = skillDaetra / Mathf.Abs(skillDaetra);
        int targetDirection = targetDaetra / Mathf.Abs(targetDaetra);
        int bonusFormulation = skillDirection / targetDirection;
        if (bonusFormulation < 0) { FindObjectOfType<BattleSystem>().allyOnHitNotifTexts[3].text = "<color=#0d9d00>Contrary ^^^ </color>"; return 1.15; }
        else { FindObjectOfType<BattleSystem>().allyOnHitNotifTexts[3].text = "<color=red>Contrary vvv </color>"; return 0.9; }
    }

    protected double CalculateSTAB(int skillDaetra, int currentDaetra) //STAB
    {
        if (skillDaetra == currentDaetra) { FindObjectOfType<BattleSystem>().allyOnHitNotifTexts[1].text = "STAB"; return 1.1; }
        else { FindObjectOfType<BattleSystem>().allyOnHitNotifTexts[1].text = ""; return 1.0; }
    }

    protected double CalculateCritBonus(int critPhase) //Crit
    {
        int critRate = (int) ((0.06 + 0.02 * critPhase) * 100);
        int roll = Random.Range(1, 101);
        if(roll <= critRate) { FindObjectOfType<BattleSystem>().allyOnHitNotifTexts[0].text = "Crit"; return 1.3; }
        else { FindObjectOfType<BattleSystem>().allyOnHitNotifTexts[0].text = ""; return 1.0; }

    }

    protected double CalculateRandomDeviation()
    {
        return Random.Range(85, 116) / 100.0;
    }

    //The below 4 methods were lazy, might want to refactor them and consolidate them into one method
    public bool SkillHit(Unit user)
    {
        return CalculateHit(user.hitRatePhase);
    }

    public bool SkillUseableByMP(Unit user)
    {
        return CalculateMPCost(user.currentMana);
    }

    protected bool CalculateHit(int hitRatePhase)
    {
        int hitRate = (int) ((0.88 + 0.04 * hitRatePhase) * 100);
        int roll = Random.Range(1, 101);
        if(roll <= hitRate) { return true; }
        else { return false; }
    }

    protected bool CalculateMPCost(int mana)
    {
        if(manaCost <= mana) { return true; }
        else { return false; }
    }

    virtual public void UseSkill(Unit user, Unit target, TextMeshProUGUI dialogueText, BattleHUD hud, BattleHUD userHUD)
    {
        Debug.Log("Whelp. Here this is.");
    }

    virtual public void UseSkill(Unit user, Unit[] targetParty, TextMeshProUGUI dialogueText, BattleHUD[] huds, BattleHUD userHUD)
    {
        Debug.Log("Whelp. Here this is.");
    }
}
