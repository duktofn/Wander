using System;
using UnityEngine;

namespace Wander.Data
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
