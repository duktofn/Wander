using System;
using UnityEngine;
using Wander.Data.Enums;

namespace Wander.Data.Spell
{
    [CreateAssetMenu(fileName = "SpellDefinitionSO", menuName = "SpellDefinitionSO")]
    public class SpellDefinitionSO : ScriptableObject
    {
        public string displayName;
        public float cost;
        public Rank rank;
        public Element element;
    }

    [Serializable]
    public class SpellEffect
    {
        
    }
}
