using MetalPod.ScriptableObjects;
using UnityEngine;

namespace MetalPod.Hovercraft
{
    [RequireComponent(typeof(Rigidbody))]
    public class HovercraftPhysics : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HovercraftStatsSO stats;
        [SerializeField] private Transform[] hoverPoints;

        [Header("Raycasts")]
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float raycastLengthMultiplier = 2f;

        [Header("Fallback Hover Points")]
        [SerializeField] private Vector3 defaultHalfExtents = new Vector3(0.8f, 0.3f, 1.2f);

        private Rigidbody _rb;
        private Vector3[] _fallbackHoverOffsets;

        private float _forwardInput = 1f;
        private float _turnInput;
        private float _altitudeInput;
        private bool _isBraking;
        private float _speedMultiplier = 1f;
        private float _turnMultiplier = 1f;
        private float _surfaceDriftMultiplier = 1f;

        private float _baseDrag;
        private float _baseAngularDrag;

        public float CurrentSpeed { get; private set; }
        public Rigidbody Rigidbody => _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _baseDrag = _rb.drag;
            _baseAngularDrag = _rb.angularDrag;
            BuildFallbackHoverPoints();
        }

        private void FixedUpdate()
        {
            if (stats == null)
            {
                return;
            }

            ApplyHoverForces();
            ApplyDriveForces();
            ApplyDrift();
            ApplyStabilization();

            Vector3 planarVelocity = Vector3.ProjectOnPlane(_rb.velocity, Vector3.up);
            CurrentSpeed = planarVelocity.magnitude;
        }

        public void SetDriveInput(float forwardInput, float turnInput)
        {
            _forwardInput = Mathf.Clamp(forwardInput, -1f, 1f);
            _turnInput = Mathf.Clamp(turnInput, -1f, 1f);
        }

        public void SetAltitudeInput(float altitudeInput)
        {
            _altitudeInput = Mathf.Clamp(altitudeInput, -1f, 1f);
        }

        public void SetBraking(bool isBraking)
        {
            _isBraking = isBraking;
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = Mathf.Max(0f, multiplier);
        }

        public void SetTurnMultiplier(float multiplier)
        {
            _turnMultiplier = Mathf.Max(0f, multiplier);
        }

        public void SetSurfaceDriftMultiplier(float multiplier)
        {
            _surfaceDriftMultiplier = Mathf.Clamp(multiplier, 0.1f, 1.5f);
        }

        private void BuildFallbackHoverPoints()
        {
            Vector3 halfExtents = defaultHalfExtents;
            if (TryGetComponent(out BoxCollider boxCollider))
            {
                halfExtents = boxCollider.size * 0.5f;
            }

            _fallbackHoverOffsets = new[]
            {
                new Vector3( halfExtents.x, 0f,  halfExtents.z),
                new Vector3(-halfExtents.x, 0f,  halfExtents.z),
                new Vector3( halfExtents.x, 0f, -halfExtents.z),
                new Vector3(-halfExtents.x, 0f, -halfExtents.z)
            };
        }

        private void ApplyHoverForces()
        {
            float targetHeight = stats.hoverHeight + (_altitudeInput * 0.5f);
            float rayLength = Mathf.Max(targetHeight * raycastLengthMultiplier, targetHeight + 0.5f);
            int pointCount = GetHoverPointCount();

            for (int i = 0; i < pointCount; i++)
            {
                Vector3 point = GetHoverPointWorldPosition(i);
                if (Physics.Raycast(point, -transform.up, out RaycastHit hit, rayLength, groundMask, QueryTriggerInteraction.Ignore))
                {
                    float compression = targetHeight - hit.distance;
                    if (compression > 0f)
                    {
                        float pointVelocityY = Vector3.Dot(_rb.GetPointVelocity(point), transform.up);
                        float springForce = (compression * stats.hoverForce) - (pointVelocityY * stats.hoverDamping);
                        _rb.AddForceAtPosition(transform.up * springForce, point, ForceMode.Acceleration);
                    }
                }
                else
                {
                    _rb.AddForceAtPosition(Physics.gravity, point, ForceMode.Acceleration);
                }
            }
        }

        private void ApplyDriveForces()
        {
            float maxSpeed = stats.maxSpeed * _speedMultiplier;
            Vector3 planarVelocity = Vector3.ProjectOnPlane(_rb.velocity, Vector3.up);

            if (_forwardInput > 0f)
            {
                if (planarVelocity.magnitude < maxSpeed)
                {
                    _rb.AddForce(transform.forward * (_forwardInput * stats.baseSpeed), ForceMode.Acceleration);
                }
            }
            else if (_forwardInput < 0f)
            {
                _rb.AddForce(transform.forward * (_forwardInput * stats.brakeForce), ForceMode.Acceleration);
            }

            _rb.AddTorque(Vector3.up * (_turnInput * stats.turnSpeed * _turnMultiplier), ForceMode.Acceleration);

            if (_isBraking)
            {
                _rb.drag = _baseDrag + stats.brakeForce;
                _rb.angularDrag = _baseAngularDrag + (stats.brakeForce * 0.1f);

                if (planarVelocity.sqrMagnitude > 0.001f)
                {
                    _rb.AddForce(-planarVelocity.normalized * stats.brakeForce, ForceMode.Acceleration);
                }
            }
            else
            {
                _rb.drag = _baseDrag;
                _rb.angularDrag = _baseAngularDrag;
            }
        }

        private void ApplyDrift()
        {
            Vector3 localVelocity = transform.InverseTransformDirection(_rb.velocity);
            float drift = Mathf.Clamp01(stats.driftFactor * _surfaceDriftMultiplier);
            localVelocity.x *= drift;
            _rb.velocity = transform.TransformDirection(localVelocity);
        }

        private void ApplyStabilization()
        {
            Vector3 correctionTorque = Vector3.Cross(transform.up, Vector3.up) * stats.stabilizationForce;
            _rb.AddTorque(correctionTorque, ForceMode.Acceleration);
        }

        private int GetHoverPointCount()
        {
            if (hoverPoints != null && hoverPoints.Length > 0)
            {
                return hoverPoints.Length;
            }

            return _fallbackHoverOffsets?.Length ?? 0;
        }

        private Vector3 GetHoverPointWorldPosition(int index)
        {
            if (hoverPoints != null && hoverPoints.Length > 0)
            {
                return hoverPoints[index].position;
            }

            return transform.TransformPoint(_fallbackHoverOffsets[index]);
        }
    }
}
