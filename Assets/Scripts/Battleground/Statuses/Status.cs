using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Status : MonoBehaviour
{
    public string statusName;
    public int statusID;
    public int maxPhase;
    public int currentPhase = 0;
    public int turnsRemaining;
    public int animID = 0;

    protected int CalculateRandomTurns(int min, int max)
    {
        return Random.Range(min, max);
    }

    virtual public void ApplyStatus(Unit target, TextMeshProUGUI dialogueText, BattleHUD affectedHUD)
    {
        Debug.Log("Whelp. Here this is.");
    }
}
