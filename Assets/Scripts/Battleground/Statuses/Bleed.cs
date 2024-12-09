using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Bleed : Status
{
    // Start is called before the first frame update
    void Start()
    {
        statusName = "Bleed";
        statusID = 0;
        maxPhase = 1;
        currentPhase = maxPhase;
        turnsRemaining = CalculateRandomTurns(2, 4);
        animID = 1;
    }

    override public void ApplyStatus(Unit target, TextMeshProUGUI dialogueText, BattleHUD affectedHUD)
    {
        {
            if(turnsRemaining > 0)
            {
                int bleedDamage = (int)(target.maxHealth * CalculateBleedDeviation());

                target.TakeDamage(bleedDamage);
                affectedHUD.SetDeltaHealth(-bleedDamage);
                turnsRemaining--;

                dialogueText.text = target.unitName + " is hurt by their bleeding!";
            }
            else
            {
                target.statuses[statusID] = null;
                Destroy(this);
                dialogueText.text = target.unitName + " stopped bleeding!";
            }
        }
    }

    private double CalculateBleedDeviation()
    {
        return Random.Range(15, 20) / 100.0;
    }
}
