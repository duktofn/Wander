using UnityEngine;
using System.Collections.Generic;
using System;

namespace Wander.Data {
    [CreateAssetMenu(fileName = "RuneDefinitionSO", menuName = "Scriptable Objects/RuneDefinitionSO")]
    public class RuneDefinitionSO : ScriptableObject
    {
        public int ID;
        public string DisplayName;
        public Ranks Rank;
        public List<Passive> Passives;
    }

    [Serializable]
    public class Passive
    {
        public PassiveEffects Effect;
        public PassiveCondition Condition; 
        public float Magnitude;  
    }

    [Serializable]
    public class PassiveCondition
    {
        public PassiveConditionType type;
        public float Threshold;
        public StatusEffects Effect;
    }
}
