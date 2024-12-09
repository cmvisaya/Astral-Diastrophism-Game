using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PauseMenuPlayerStatHUD : MonoBehaviour
{
    public TextMeshProUGUI[] statTexts = new TextMeshProUGUI[8];
    
    public void DisplayUnit(Unit unit)
    {
        if(unit != null)
        {
            string daetraString = "";
            switch (unit.baseDaetra)
            {
                case -3:
                    daetraString = "<color=#960000><b>Chaos</b></color>";
                    break;
                case -2:
                    daetraString = "<color=#960096><b>Death</b></color>";
                    break;
                case -1:
                    daetraString = "<color=#282828><b>Void</b></color>";
                    break;
                case 1:
                    daetraString = "<color=#d2aa32><b>Existence</b></color>";
                    break;
                case 2:
                    daetraString = "<color=#187800><b>Life</b></color>";
                    break;
                case 3:
                    daetraString = "<color=#000096><b>Order</b></color>";
                    break;
            }
            statTexts[0].text = unit.unitName + ": Lv" + unit.unitLevel + " (" + daetraString + ")";
            statTexts[1].text = "XP: " + unit.xp + "/" + unit.xpToLevel;
            statTexts[2].text = "HP: " + unit.currentHealth + "/" + unit.maxHealth + " | MP: " + unit.currentMana + "/" + unit.maxMana;
            statTexts[3].text = "Attack: " + unit.baseAtk;
            statTexts[4].text = "Defense: " + unit.baseDef;
            statTexts[5].text = "Max Pips: " + unit.maxPips;
            statTexts[6].text = "Pip Regen: " + unit.pipRegenRate;
            statTexts[7].text = "Initiative: " + unit.initiative;
        }
        else
        {
            statTexts[0].text = "";
            statTexts[1].text = "";
            statTexts[2].text = "";
            statTexts[3].text = "";
            statTexts[4].text = "";
            statTexts[5].text = "";
            statTexts[6].text = "";
            statTexts[7].text = "";
        }
    }
}
