using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEntity : MonoBehaviour
{
    protected enum Daetra : int
    {
        Void = -1,
        Existence = 1,
        Death = -2,
        Life = 2,
        Chaos = -3,
        Order = 3
    }

    public string entityName;
    public int daetra;
    public int level = 0;

    public int pips = 0;
    public int pipRegenRate = 0;
    public int maxPips = 0;

    public int maxHealth = 0;
    public int health = 0;
    public int baseAtk = 0;
    public int atk = 0;
    public int baseDef = 0;
    public int def = 0;

    public double critRate = 0.06;
    public double hitRate = 0.85;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void init(string name, int daetra, int level, int pipRegenRate, int maxPips, int maxHealth, int baseAtk, int baseDef)
    {
        this.entityName = name;
        this.daetra = daetra;
        this.level = level;
        this.pipRegenRate = pipRegenRate;
        this.maxPips = maxPips;
        this.pipRegenRate = pipRegenRate;
        this.maxHealth = maxHealth;
        this.health = this.maxHealth;
        this.baseAtk = baseAtk;
        this.atk = this.baseAtk;
        this.baseDef = baseDef;
        this.def = this.baseDef;
    }
}
