using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wander.Data
{
    [CreateAssetMenu(fileName = "SpellDefinitionSO", menuName = "SpellDefinitionSO")]
    public class SpellDefinitionSO : ScriptableObject
    {
        public int ID;
        public string DisplayName;
        public float MPCost;
        public int Cooldown;
        public Elements Element;
        public Ranks Rank;
        public int WISRequire;
        public List<SpellEffect> SpellEffects;
    }

    [Serializable]
    public class SpellEffect
    {
        public SpellEffectType EffectType;
        public SpellTargetType TargetType;
        public SpellScalingType ScalingType;
        public float BaseMagnitude;
        public List<AttributeWeight> ScalingWeights;
        public SpellEffect SourceEffectRef;
        public StatusEffects EffectToApply;
        public int EffectToApplyDuration;
        public StatusEffects EffectToRemove;
    }

    [Serializable]
    public class AttributeWeight
    {
        public Attributes attribute;
        public float Weight;
    }
}
