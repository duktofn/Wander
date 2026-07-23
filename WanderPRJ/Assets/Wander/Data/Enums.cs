namespace Wander.Data
{
    public enum Ranks
    {
        I = 0,
        II = 1,
        III = 2
    }

    public enum Elements
    {
        FIRE = 0,
        WATER = 1,
        LIGHTNING = 2,
        ICE = 3
    }

    public enum SpellEffectType
    {
        DEAL_DMG = 0,
        HEAL = 1,
        RECOVER_MP = 2,
        GAIN_ARMOR = 3,
        APPLY_EFFECT = 4,
        REMOVE_EFFECT = 5,
    }

    public enum SpellTargetType
    {
        SELF = 0,
        SINGLE_ALLY = 1, RANDOM_ALLY = 2, RANDOM_MULTI_ALLIES = 3, ALL_ALLIES = 4, 
        LOWEST_HP_ALLY = 5, LOWEST_HP_ALLIES = 6, HIGHEST_HP_ALLY = 7, HIGHEST_HP_ALLIES = 8,
        SINGLE_ENEMY = 9, RANDOM_ENEMY = 10, RANDOM_MULTI_ENEMIES = 11, ALL_ENEMIES = 12,
        LOWEST_HP_ENEMY = 13, LOWEST_HP_ENEMIES = 14, HIGHEST_HP_ENEMY = 15, HIGHEST_HP_ENEMIES = 16
    }

    public enum SpellScalingType
    {
        Independent = 0,
        Derived = 1
    }

    public enum Attributes
    {
        VIT = 0, SPI = 1, POT = 2, WIS = 3,
        RES = 4, F_RES = 5, W_RES = 6, L_RES = 7, I_RES = 8,
        F_POT = 9, W_POT = 10, L_POT = 11, I_POT = 12,
        MAX_HP = 13, MAX_MP = 14
    }

    public enum StatusEffects
    {
        Enrage = 0, Refreshing = 1, Fortified = 2, Energized = 3,     
        Burn = 4, Drenched = 5, Chilled = 6, Dazed = 7,               
        Regen = 8, Distracted = 9,                            
        Overdrive = 10, Crystalize = 11, Detonates = 12, Frozen = 13
    }
}