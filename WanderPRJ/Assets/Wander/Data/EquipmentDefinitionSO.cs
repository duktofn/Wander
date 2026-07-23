using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wander.Data
{
    [CreateAssetMenu(fileName = "EquipmentDefinitionSO", menuName = "Scriptable Objects/EquipmentDefinitionSO")]
    public class EquipmentDefinitionSO : ScriptableObject
    {
        public EquipmentType Type;
        public Ranks Rank;
        public List<AttributeBonus> Bonuses;
        public List<Passive> Passives;
    }

    [Serializable]
    public class AttributeBonus
    {
        public Attributes Attribute;
        public int Amount;
    }
}
