using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Tutorial
{
    [RequireComponent(typeof(Collider))]
    public class TutorialTrigger : MonoBehaviour
    {
        [SerializeField] private string stepId;
        [SerializeField] private bool triggerOnce = true;

        private bool _triggered;

        private void Reset()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered && triggerOnce)
            {
                return;
            }

            if (!other.CompareTag(GameConstants.TAG_PLAYER))
            {
                return;
            }

            _triggered = true;
            TutorialManager.Instance?.TriggerContextualStep(stepId);
        }
    }
}
