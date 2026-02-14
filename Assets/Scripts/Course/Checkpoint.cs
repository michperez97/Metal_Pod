using System;
using MetalPod.Hovercraft;
using UnityEngine;

namespace MetalPod.Course
{
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private int checkpointIndex;
        [SerializeField] private Renderer indicatorRenderer;
        [SerializeField] private Color inactiveColor = Color.red;
        [SerializeField] private Color activeColor = Color.green;
        [SerializeField] private TextMesh checkpointNumberLabel;
        [SerializeField] private string checkpointPrefix = "CP ";

        public event Action<Checkpoint, HovercraftController> OnActivated;

        public int CheckpointIndex => checkpointIndex;
        public Transform SpawnPoint => transform;
        public bool IsActive { get; private set; }

        private void Reset()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
        }

        private void OnValidate()
        {
            UpdateLabelText();
        }

        private void Start()
        {
            SetVisualState(false);
            UpdateLabelText();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsActive)
            {
                return;
            }

            HovercraftController controller = other.GetComponentInParent<HovercraftController>();
            if (controller == null)
            {
                return;
            }

            IsActive = true;
            SetVisualState(true);
            OnActivated?.Invoke(this, controller);
        }

        public void ResetCheckpoint()
        {
            IsActive = false;
            SetVisualState(false);
        }

        private void SetVisualState(bool active)
        {
            if (indicatorRenderer != null)
            {
                indicatorRenderer.material.color = active ? activeColor : inactiveColor;
            }

            if (checkpointNumberLabel != null)
            {
                checkpointNumberLabel.color = active ? activeColor : inactiveColor;
            }
        }

        private void UpdateLabelText()
        {
            if (checkpointNumberLabel == null)
            {
                return;
            }

            checkpointNumberLabel.text = $"{checkpointPrefix}{checkpointIndex + 1}";
        }
    }
}
