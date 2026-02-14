using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Hazards
{
    public class DamageZone : HazardBase
    {
        private void Reset()
        {
            damageType = DamageType.Physical;

            Collider trigger = GetComponent<Collider>();
            if (trigger != null)
            {
                trigger.isTrigger = true;
            }
        }
    }
}
