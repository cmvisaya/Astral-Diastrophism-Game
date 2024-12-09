using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class BattleHUD : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI pipText;
    public Slider hpSlider;
    public Slider mpSlider;

    public Text deltaHealth;
    public Text deltaMana;

    private string pipTextEnd;

    public void SetHUD(Unit unit)
    {
        nameText.text = unit.unitName;
        switch(unit.currentDaetra)
        {
            case 1: //Existence
                nameText.color = new Color32(210, 170, 50, 255);
                break;
            case -1: //Void
                nameText.color = new Color32(40, 40, 40, 255);
                break;
            case 2: //Life
                nameText.color = new Color32(25, 120, 0, 255);
                break;
            case -2: //Death
                nameText.color = new Color32(150, 0, 150, 255);
                break;
            case 3: //Order
                nameText.color = new Color32(0, 0, 150, 255);
                break;
            case -3: //Chaos
                nameText.color = new Color32(150, 0, 0, 255);
                break;
        }
        nameText.ForceMeshUpdate();
        levelText.text = "Lvl " + unit.unitLevel;
        pipTextEnd = "/" + unit.maxPips + " P";
        pipText.text = unit.currentPips + pipTextEnd;
        hpSlider.maxValue = unit.maxHealth;
        hpSlider.value = unit.currentHealth;
        mpSlider.maxValue = unit.maxMana;
        mpSlider.value = unit.currentMana;
    }

    public void ClearDelta()
    {
        deltaHealth.text = "";
        deltaMana.text = "";
    }

    public void SetHP(int targetHP)
    {
        hpSlider.value = targetHP;
    }

    public void SetMP(int targetMP)
    {
        mpSlider.value = targetMP;
    }

    public void SetPips(int targetPips)
    {
        pipText.text = targetPips + pipTextEnd;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public bool IsActive()
    {
        return gameObject.activeSelf;
    }

    public void SetDeltaHealth(int delta)
    {
        string text = "";
        switch (Math.Sign(delta))
        {
            case -1:
                text = "" + delta;
                break;
            case 1:
                text = "+" + delta;
                break;
        }
        deltaHealth.text = text;
    }

    public void SetDeltaMana(int delta)
    {
        string text = "";
        switch (Math.Sign(delta))
        {
            case -1:
                text = "" + delta;
                break;
            case 1:
                text = "+" + delta;
                break;
        }
        deltaMana.text = text;
    }
}
