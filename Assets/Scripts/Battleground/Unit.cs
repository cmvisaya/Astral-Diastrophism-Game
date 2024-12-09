using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Unit : MonoBehaviour
{
    public int xp = 0;
    public int xpToLevel = 100;
    public GameObject skillPool;
    public SkillPool learnedSkills;

    //XP granted should be 15 / ((playerLv - enemyLv) + 15) * 100
    //xpToLevel should go up by 50 each level (xpToLevel = 100 + 50 * (unitLevel - 1));

    public string unitName;
    public int unitLevel;
    public int baseDaetra;
    public int currentDaetra;
    public int pipRegenRate;
    public int maxPips;
    public int currentPips;
    public int maxHealth;
    public int currentHealth;
    public int maxMana;
    public int currentMana;
    public int baseAtk;
    public int currentAtk;
    public int baseDef;
    public int currentDef;
    public int initiative;

    public int minHealthAffinity;
    public int maxHealthAffinity;
    public int minManaAffinity;
    public int maxManaAffinity;
    public int minAttackAffinity;
    public int maxAttackAffinity;
    public int minDefenseAffinity;
    public int maxDefenseAffinity;
    public int initiativeUpEveryLvs;
    public int pipsUpEveryLvs;
    public int pipRegenUpEveryLvs;

    public int critRatePhase = 0;
    public int hitRatePhase = 0;

    public Skill[] skills = new Skill[5];

    public Status[] statuses = new Status[2];
    public bool statusesAppliedOnTurn = false; //Using this for additional logic to make sure statuses are only applied once per turn

    public int[,] buffs = new int[4,2]; 
    /*
     * Top row is for phase, bottom row is for turns remaining
     * Indexes: 0 = atk, 1 = def; 2 = cr; 3 = hr
     */

    public bool isGuarding = false;
    public bool isDead = false;
    public int animID = 0; //0 is the neutral state

    [SerializeField] private Animator anim;

    //This is for the camera code so we can appropriately look at the target in the battleground
    public float sizeOffset;

    void Awake()
    {
        skills[0] = gameObject.AddComponent<BasicAttack>() as BasicAttack;
        skills[0].skillDaetra = baseDaetra;
        statuses = new Status[2];
    }

    void Start()
    {
        learnedSkills = skillPool.GetComponent<SkillPool>();
    }

    void Update()
    {
        if(anim != null)
        {
            anim.SetBool("isDead", isDead);
            anim.SetInteger("animID", animID);
        }
    }

    public void Initialize(Unit other)
    {
        xp = other.xp;
        xpToLevel = other.xpToLevel;

        skills = other.skills;

        unitName = other.unitName;
        unitLevel = other.unitLevel;
        baseDaetra = other.baseDaetra;
        pipRegenRate = other.pipRegenRate;

        //Current mana, health, name, and level are retained across battles
        //Current pips, atk, def, and statuses are not retained

        maxHealth = other.maxHealth;
        currentHealth = other.currentHealth;
        maxMana = other.maxMana;
        currentMana = other.currentMana;

        maxPips = other.maxPips;
        baseAtk = other.baseAtk;
        baseDef = other.baseDef;
    }

    public Animator GetAnim()
    {
        return anim;
    }

    public void SetAnim(Animator anim)
    {
        this.anim = anim;
    }

    public bool TakeDamage(int dmg)
    {
        animID = -1;
        currentHealth -= dmg;
        if(currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
        }
        return isDead;
    }

    public int GetStatusWaitTime()
    {
        int waitTime = 0;
        for(int i = 0; i < statuses.Length; i++)
        {
            if(statuses[i] != null)
            {
                waitTime += 3;
            }
        }
        return waitTime;
    }

    public void InflictStatus(Status status)
    {
        if(status.statusID < statuses.Length)
        {
            if (statuses[status.statusID] != null && statuses[status.statusID].currentPhase < status.maxPhase)
            {
                status.currentPhase = statuses[status.statusID].currentPhase + 1;
            }
            statuses[status.statusID] = status;
        }
    }

    public void UpdateBuffs()
    {
        for(int i = 0; i < buffs.GetLength(0); i++)
        {
            if(buffs[i, 1] != 0) //If there is a non-zero turns remaining
            {
                switch (i)
                {
                    case 0:
                        currentAtk = (int) (baseAtk * (1 + .2 * buffs[i, 0]));
                        break;
                    case 1:
                        currentDef = (int) (baseDef * (1 + .2 * buffs[i, 0]));
                        break;
                    case 2:
                        critRatePhase = buffs[i, 0];
                        break;
                    case 3:
                        hitRatePhase = buffs[i, 0];
                        break;
                }
                buffs[i, 1]--;
            }
            else
            {
                buffs[i, 0] = 0; //Buff phase = 0;
            }
        }
    }

    public void ApplyBuff(int buffIndex, int deltaPhase)
    {
        buffs[buffIndex, 0] += deltaPhase;
        if(Math.Abs(buffs[buffIndex, 0]) > 3)
        {
            buffs[buffIndex, 0] = 3 * Math.Sign(buffs[buffIndex, 0]);
        }
        buffs[buffIndex, 1] = 3;
    }

    public bool HasStatus()
    {
        for(int i = 0; i < statuses.Length; i++)
        {
            if (statuses[i] != null)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsAsleep()
    {
        if(statuses[1] != null)
        {
            return true;
        }
        return false;
    }

    public void IncrementMana(int mana)
    {
        currentMana += mana;
        if(currentMana < 0)
        {
            currentMana = 0;
        }
        else if(currentMana > maxMana)
        {
            currentMana = maxMana;
        }
    }

    public void Heal(int hp)
    {
        Debug.Log("heal amount: " + hp);
        currentHealth += hp;
        isDead = false;
        if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    public void RegenPips()
    {
        currentPips += pipRegenRate;
        if(currentPips > maxPips)
        {
            currentPips = maxPips;
        }
    }

    //XP granted should be 15 / ((playerLv - enemyLv) + 15) * 100
    //xpToLevel should go up by 50 each level (xpToLevel = 100 + 50 * (unitLevel - 1));
    //The formula currently gates the max xp gained per enemy to 1500 xp if you defeat an enemy 15 levels higher than you or over

    public bool GainXP(int level)
    {
        bool leveled = false;

        int xpToGain = (int) ((100 + 50 * (level - 1)) * UnityEngine.Random.Range(0.7f, 1.3f));
        Debug.Log("gained: " + xpToGain);

        while (xpToGain > 0)
        {
            xp++;
            xpToGain--;
            if(xp == xpToLevel)
            {
                leveled = true;
                HandleLevelUp();
            }
        }
        return leveled;
    }

    public void HandleLevelUp()
    {
        unitLevel++;
        xp = 0;
        xpToLevel = (int) (Math.Ceiling(100 * Math.Pow(unitLevel, 1.8)));

        FindObjectOfType<BattleSystem>().levelNotifTexts[0].text = unitName + ": Lv" + unitLevel;

        int added;
        added = UnityEngine.Random.Range(minHealthAffinity, maxHealthAffinity);
        maxHealth += added;
        FindObjectOfType<BattleSystem>().levelNotifTexts[1].text = "Health: +" + added;
        Debug.Log("hp += " + added);
        added = UnityEngine.Random.Range(minManaAffinity, maxManaAffinity);
        maxMana += added;
        FindObjectOfType<BattleSystem>().levelNotifTexts[2].text = "Mana: +" + added;
        Debug.Log("mana += " + added);
        added = UnityEngine.Random.Range(minAttackAffinity, maxAttackAffinity);
        baseAtk += added;
        currentAtk = baseAtk;
        FindObjectOfType<BattleSystem>().levelNotifTexts[3].text = "Attack: +" + added;
        Debug.Log("atk += " + added);
        added = UnityEngine.Random.Range(minDefenseAffinity, maxDefenseAffinity);
        baseDef += added;
        currentDef = baseDef;
        FindObjectOfType<BattleSystem>().levelNotifTexts[4].text = "Defense: +" + added;
        Debug.Log("def += " + added);

        if (unitLevel % pipsUpEveryLvs == 0) { maxPips += 1; FindObjectOfType<BattleSystem>().levelNotifTexts[5].text = "Max Pips: +" + 1; }
        else { FindObjectOfType<BattleSystem>().levelNotifTexts[5].text = "Max Pips: +" + 0; }
        if (unitLevel % pipRegenUpEveryLvs == 0) { pipRegenRate += 1; FindObjectOfType<BattleSystem>().levelNotifTexts[6].text = "Pip Regen: +" + 1; }
        else { FindObjectOfType<BattleSystem>().levelNotifTexts[6].text = "Pip Regen: +" + 0; }
        if (unitLevel % initiativeUpEveryLvs == 0) { initiative += 1; FindObjectOfType<BattleSystem>().levelNotifTexts[7].text = "Initiative: +" + 1; }
        else { FindObjectOfType<BattleSystem>().levelNotifTexts[7].text = "Initiative: +" + 0; }

        Debug.Log("to level: " + xpToLevel);
    }

    public Skill[] GetLearnableArray()
    {
        int length = 0;
        for(int i = 0; i < learnedSkills.skillPool.Length; i++)
        {
            if(learnedSkills.levelsLearnedAt[i] <= unitLevel)
            {
                length++;
            }
            else
            {
                break;
            }
        }
        Debug.Log("BBBB" + length);

        Skill[] returned = new Skill[length];
        for(int i = 0; i < returned.Length; i++)
        {
            returned[i] = learnedSkills.skillPool[i];
        }

        return returned;
    }

    public bool SkillKnown(string skillName)
    {
        for(int i = 1; i < skills.Length; i++)
        {
            if (skills[i] != null && skills[i].skillName == skillName)
            {
                return true;
            }
        }
        return false;
    }

    public int GetRandomOccupiedSlot()
    {
        int slot = UnityEngine.Random.Range(0, skills.Length);
        while(skills[slot] == null || skills[slot].manaCost > currentMana)
        {
            slot = UnityEngine.Random.Range(0, skills.Length);
        }
        return slot;
    }
}
