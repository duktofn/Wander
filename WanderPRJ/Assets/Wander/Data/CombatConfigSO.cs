using System;
using UnityEngine;

namespace Wander.Data {
    [CreateAssetMenu(fileName = "CombatConfigSO", menuName = "CombatConfigSO")]
    public class CombatConfigSO : ScriptableObject
    {
        public HPConfig hpConfig;
        public MPConfig mpConfig;
        public ResistanceConfig resistanceConfig;
        public EffectModifierConfig effectModifierConfig;
    }

    [Serializable]
    public class HPConfig
    {
        public float BASE_HP_CAP;
        public float BASE_HP_HALF_VIT;
    }

    [Serializable]
    public class MPConfig
    {
        public float MP_COEFF;
        public float BASE_MP_RECOVERY;
    }

    [Serializable]
    public class ResistanceConfig
    {
        public float EXPONENT_P;
    }

    [Serializable]
    public class EffectModifierConfig
    {
        [Header("Fire")]
        public float enrageModifier;
        public float burnBaseDMG;
        public float burnHPPercent;

        [Header("Water")]
        public float refreshingModifier;
        public float drenchedModifier;

        [Header("Lightning")]
        public float energizedBaseDMG;
        public float energizedPOTScale;
        public float dazedModifier;

        [Header("Ice")]
        public float fortifiedModifier;
        public float chilledModifier;

        [Header("Combos")]
        public float overdriveModifier;
        public float detonatesHPPercent;

        [Header("Other Effects")]
        public float regenHPPercent;
        public float distractedAdditionPercent;
    }
}
