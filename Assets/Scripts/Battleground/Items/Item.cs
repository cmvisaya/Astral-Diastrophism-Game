using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Item : MonoBehaviour
{
    public string itemName;
    public int itemDaetra;
    public int itemBasePower;
    public TargetingMode targetingMode;
    public int pipCost = 0;
    public int animID = 0;
    public string itemDescription = "This item has no description yet.";

    //May need to redeclare targeting mode in here but I don't think so

    public double typeAdvantageMult;
    public double contraryTypeMult;
    public double sameTypeAttackBonus;
    public double critBonus;
    public double randomDeviation;

    //SHOP VARIABLES
    public int sellPrice;
    public int buyPrice;

    protected double CalculateTypeAdvMult(int skillDaetra, int targetDaetra)
    {
        int bonusFormulation = ((Mathf.Abs(skillDaetra) % 3) - (Mathf.Abs(targetDaetra) - 3)) % 3;
        if (bonusFormulation == 0) { return 1; }
        else if (bonusFormulation == 1) { return 0.85; }
        else if (bonusFormulation == 2) { return 1.2; }
        else { return 1; }
    }

    protected double CalculateContraryTypeMult(int skillDaetra, int targetDaetra)
    {
        int skillDirection = skillDaetra / Mathf.Abs(skillDaetra);
        int targetDirection = targetDaetra / Mathf.Abs(targetDaetra);
        int bonusFormulation = skillDirection / targetDirection;
        if (bonusFormulation < 0) { return 1.15; }
        else { return 0.9; }
    }

    protected double CalculateSTAB(int skillDaetra, int currentDaetra)
    {
        if (skillDaetra == currentDaetra) { return 1.1; }
        else { return 1.0; }
    }

    protected double CalculateCritBonus(int critPhase)
    {
        int critRate = (int)((0.06 + 0.02 * critPhase) * 100);
        int roll = Random.Range(1, 101);
        if (roll <= critRate) { return 1.3; }
        else { return 1.0; }

    }

    protected double CalculateRandomDeviation()
    {
        return Random.Range(85, 116) / 100.0;
    }

    //Also, item fizzle chance is 0%. This is partially why I need to refactor such that fizzle chance and hit chance are two separate things. I am going to kershoot myself for having to make prc and eva stats
    //That or I could just keep fizzle chance - I feel like using an item could be fine if the hit is just guaranteed. Will think about balancing later.

    //USING AN ITEM SHOULD USE ONE PIP BUT NOT END THE USERS TURN (unless of course the item is a pip reclamaition item)

    virtual public void UseItem(Unit user, Unit target, TextMeshProUGUI dialogueText, BattleHUD hud, BattleHUD userHUD)
    {
        Debug.Log("Whelp. Here this is.");
    }

    virtual public void UseItem(Unit user, Unit[] targetParty, TextMeshProUGUI dialogueText, BattleHUD[] huds, BattleHUD userHUD)
    {
        Debug.Log("Whelp. Here this is.");
    }
}
