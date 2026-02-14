using MetalPod.Hovercraft;
using UnityEngine;

namespace MetalPod.Hazards.Ice
{
    [RequireComponent(typeof(Collider))]
    public class IcePatch : MonoBehaviour
    {
        [SerializeField] [Range(0.1f, 1f)] private float driftMultiplier = 0.6f;

        private void Reset()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            HovercraftPhysics physics = other.GetComponentInParent<HovercraftPhysics>();
            if (physics != null)
            {
                physics.SetSurfaceDriftMultiplier(driftMultiplier);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            HovercraftPhysics physics = other.GetComponentInParent<HovercraftPhysics>();
            if (physics != null)
            {
                physics.SetSurfaceDriftMultiplier(1f);
            }
        }
    }
}
