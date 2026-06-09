using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Element
{
    VOID,
    FIRE,
    WATER,
    EARTH,
    WIND,
    LIGHT,
    DARK,

    LIGHTNING, // fire and wind
    ICE, // water and wind
    MAGMA, // fire and earth
    PLANT, // water and earth
    HEALING, // light and plant
    HEX, // dark and plant
}

public enum StatusEffectType
{
    BURNED,
    WET,
    BLINDED,
    POISONED,
    SHOCKED,
    FROZEN,
    CURSED,
}

public class StatusEffect
{
    public StatusEffectType type;
    public float duration = 1;
    public int severity = 0;
}

public class Status
{
    List<StatusEffect> statusEffects = new List<StatusEffect>();

    public int maxHp = 100;
    public int hp = 100;
    public int power = 2;

    public int fireRes = 0;
    public int waterRes = 0;
    public int earthRes = 0;
    public int windRes = 0;
    public int lightRes = 0;
    public int darkRes = 0;

    public bool CheckForStatusEffect(StatusEffectType statusEffect)
    {
        foreach(StatusEffect effect in statusEffects)
        {
            if(effect.type == statusEffect)
            {
                return true;
            }
        }
        return false;
    }

    public void AddStatusEffect(StatusEffect statusEffect)
    {
        foreach(StatusEffect effect in statusEffects)
        {
            if(effect.type == statusEffect.type)
            {
                effect.severity += statusEffect.severity; 
                if(effect.duration < statusEffect.duration)
                {
                    effect.duration = statusEffect.duration;
                }
                return;
            }
        }
        statusEffects.Add(statusEffect);
    }

    public void RemoveStatusEffect(StatusEffectType statusEffect)
    {
        foreach(StatusEffect effect in statusEffects)
        {
            if(effect.type == statusEffect)
            {
                statusEffects.Remove(effect);
            }
        }
    }

}
