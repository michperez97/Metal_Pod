using System;
using MetalPod.Hovercraft;
using UnityEngine;

namespace MetalPod.Course
{
    [RequireComponent(typeof(Collider))]
    public class FinishLine : MonoBehaviour
    {
        public event Action<HovercraftController> OnPlayerFinished;

        private void Reset()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            HovercraftController controller = other.GetComponentInParent<HovercraftController>();
            if (controller == null)
            {
                return;
            }

            OnPlayerFinished?.Invoke(controller);
        }
    }
}
