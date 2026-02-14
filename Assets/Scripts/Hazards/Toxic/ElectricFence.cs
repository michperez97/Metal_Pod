using System.Collections;
using System.Collections.Generic;
using MetalPod.Hovercraft;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Hazards
{
    public class ElectricFence : HazardBase
    {
        [Header("Fence Settings")]
        [SerializeField] private float onDuration = 2f;
        [SerializeField] private float offDuration = 3f;
        [SerializeField] private float stunDuration = 1f;

        [Header("Visuals")]
        [SerializeField] private Renderer fenceRenderer;
        [SerializeField] private Material activeMaterial;
        [SerializeField] private Material inactiveMaterial;
        [SerializeField] private ParticleSystem sparkEffect;
        [SerializeField] private AudioClip electricHum;
        [SerializeField] private AudioClip zapSound;

        private readonly HashSet<HovercraftInput> _stunnedInputs = new HashSet<HovercraftInput>();
        private Coroutine _cycleRoutine;
        private bool _isPowered;

        protected override void Awake()
        {
            base.Awake();
            damageType = DamageType.Electric;
        }

        private void OnEnable()
        {
            _cycleRoutine = StartCoroutine(PowerCycleRoutine());
        }

        private void OnDisable()
        {
            if (_cycleRoutine != null)
            {
                StopCoroutine(_cycleRoutine);
                _cycleRoutine = null;
            }

            SetPoweredState(false);
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (!_isPowered)
            {
                return;
            }

            base.OnTriggerEnter(other);
            TryStun(other);

            if (zapSound != null)
            {
                AudioSource.PlayClipAtPoint(zapSound, transform.position);
            }
        }

        protected override void OnTriggerStay(Collider other)
        {
            if (!_isPowered)
            {
                return;
            }

            base.OnTriggerStay(other);
        }

        private IEnumerator PowerCycleRoutine()
        {
            WaitForSeconds onWait = new WaitForSeconds(Mathf.Max(0.1f, onDuration));
            WaitForSeconds offWait = new WaitForSeconds(Mathf.Max(0.1f, offDuration));

            while (enabled)
            {
                SetPoweredState(true);
                yield return onWait;
                SetPoweredState(false);
                yield return offWait;
            }
        }

        private void SetPoweredState(bool powered)
        {
            _isPowered = powered && isActive;

            if (fenceRenderer != null)
            {
                fenceRenderer.material = _isPowered ? activeMaterial : inactiveMaterial;
            }

            if (sparkEffect != null)
            {
                if (_isPowered)
                {
                    sparkEffect.Play();
                }
                else
                {
                    sparkEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }

            if (_isPowered && electricHum != null)
            {
                AudioSource.PlayClipAtPoint(electricHum, transform.position);
            }
        }

        private void TryStun(Collider other)
        {
            HovercraftInput input = other.GetComponentInParent<HovercraftInput>();
            if (input == null || _stunnedInputs.Contains(input))
            {
                return;
            }

            StartCoroutine(StunRoutine(input));
        }

        private IEnumerator StunRoutine(HovercraftInput input)
        {
            _stunnedInputs.Add(input);

            bool previousState = input.enabled;
            input.enabled = false;
            yield return new WaitForSeconds(Mathf.Max(0.05f, stunDuration));

            if (input != null)
            {
                input.enabled = previousState;
            }

            _stunnedInputs.Remove(input);
        }
    }
}
