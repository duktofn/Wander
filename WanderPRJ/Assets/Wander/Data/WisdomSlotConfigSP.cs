using UnityEngine;

namespace Wander.Data {
    [CreateAssetMenu(fileName = "WisdomSlotConfigSP", menuName = "WisdomSlotConfigSO")]
    public class WisdomSlotConfigSO : ScriptableObject
    {
        public int[] slotRequireWIS;
    }
}
