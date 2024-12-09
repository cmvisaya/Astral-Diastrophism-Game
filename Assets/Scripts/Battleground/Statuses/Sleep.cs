using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Sleep : Status
{
    // Start is called before the first frame update
    void Awake()
    {
        statusName = "Sleep";
        statusID = 1;
        maxPhase = 1;
        currentPhase = maxPhase;
        turnsRemaining = CalculateRandomTurns(2, 4);
        animID = 0;
    }

    override public void ApplyStatus(Unit target, TextMeshProUGUI dialogueText, BattleHUD affectedHUD)
    {
        {
            if (turnsRemaining > 0)
            {
                turnsRemaining--;
                dialogueText.text = target.unitName + " is fast asleep!";
            }
            else
            {
                target.statuses[statusID] = null;
                Destroy(this);
                dialogueText.text = target.unitName + " wakes up!";
            }
        }
    }
}
