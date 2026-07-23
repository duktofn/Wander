using System;
using System.Collections.Generic;
using Vector3 = UnityEngine.Vector3;
using UnityEngine;

namespace Wander.Data {
    [CreateAssetMenu(fileName = "ProgressionConfigSO", menuName = "ProgressionConfigSO")]
    public class ProgressionConfigSO : ScriptableObject
    {
        public ShopPriceConfig shopPrice;
        public RewardConfig rewardConfig;
    }

    [Serializable]
    public class ShopPriceConfig
    {
        public List<Vector3> ArcRankRate;
        public List<Vector3> ArcRankPrice;
        public List<Vector3> RuneSocketPrice;
    }

    [Serializable]
    public class RewardConfig
    {
        public List<Vector3> CombatArcGoldReward;
        public List<Vector3> MinionEliteDropRate;
        public List<Vector3> BossesDropRate;
    }
}
