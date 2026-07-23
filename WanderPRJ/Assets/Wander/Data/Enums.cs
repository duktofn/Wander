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

    public enum PassiveEffects
    {
        INCR_POT = 0, INCR_F_POT = 1, INCR_W_POT = 2, INCR_L_POT = 3, INCR_I_POT = 4,
        DESC_POT = 5, DESC_F_POT = 6, DESC_W_POT = 7, DESC_L_POT = 8, DESC_I_POT = 9,
        INCR_VIT = 10, DESC_VIT = 11,
        INCR_RES = 12, INCR_F_RES = 13, INCR_W_RES = 14, INCR_L_RES = 15, INCR_I_RES = 16, 
        DESC_RES = 17, DESC_F_RES = 18, DESC_W_RES = 19, DESC_L_RES = 20, DESC_I_RES = 21,
        INCR_MAX_HP = 22, DESC_MAX_HP = 23,
        INCR_MAX_MP = 24, DESC_MAX_MP = 25,
        INCR_MP_RECOVER = 26, DESC_MP_RECOVER = 27,
        INCR_SPI = 28, DESC_SPI = 29,
        INCR_WIS = 30, DESC_WIS = 31,
        INCR_DMG_DEAL_TO_ENEMY = 32, DESC_DMG_DEAL_BY_ENEMY = 33, 
        INCR_HEAL = 34, DESC_HEAL = 35,
        INSTANT_KILL = 36
    }

    public enum PassiveConditionType
    {
        NONE = 0,
        HAS_STATUS = 1, HAS_ARMOR = 2,
        HP_LOWER_THAN_PERCENT = 3, HP_HIGHER_THAN_PERCENT = 4,
        MP_LOWER_THAN_PERCENT = 5, MP_HIGHER_THAN_PERCENT = 6
    }

    public enum EquipmentType
    {
        STAFF = 0, RING = 1, BOOK = 2, GARB = 3
    }
}