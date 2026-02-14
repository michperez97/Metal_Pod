using System;
using System.Collections;
using UnityEngine;

namespace MetalPod.GameCamera
{
    [Serializable]
    public struct CameraWaypoint
    {
        public Vector3 position;
        public Vector3 lookAtOffset;
        public float transitionTime;
        public AnimationCurve easeCurve;
    }

    public class CinematicCamera : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] private CameraWaypoint[] waypoints;
        [SerializeField] private bool loop;

        [Header("Runtime")]
        [SerializeField] private Transform lookAtTarget;

        public event Action OnSequenceComplete;

        private UnityEngine.Camera cameraComponent;
        private Coroutine activeSequence;
        private bool isPlaying;

        public bool IsPlaying => isPlaying;

        private void Awake()
        {
            cameraComponent = GetComponent<UnityEngine.Camera>();
        }

        public void PlaySequence(Transform target = null)
        {
            if (target != null)
            {
                lookAtTarget = target;
            }

            if (activeSequence != null)
            {
                StopCoroutine(activeSequence);
            }

            activeSequence = StartCoroutine(RunSequence());
        }

        public void Stop()
        {
            if (activeSequence != null)
            {
                StopCoroutine(activeSequence);
                activeSequence = null;
            }

            isPlaying = false;
        }

        private IEnumerator RunSequence()
        {
            isPlaying = true;

            if (waypoints == null || waypoints.Length == 0)
            {
                isPlaying = false;
                OnSequenceComplete?.Invoke();
                activeSequence = null;
                yield break;
            }

            do
            {
                for (int i = 0; i < waypoints.Length; i++)
                {
                    CameraWaypoint waypoint = waypoints[i];
                    Vector3 startPosition = transform.position;
                    Quaternion startRotation = transform.rotation;

                    float duration = Mathf.Max(0.01f, waypoint.transitionTime);
                    float elapsed = 0f;

                    while (elapsed < duration)
                    {
                        elapsed += Time.deltaTime;
                        float t = Mathf.Clamp01(elapsed / duration);

                        if (waypoint.easeCurve != null && waypoint.easeCurve.length > 0)
                        {
                            t = waypoint.easeCurve.Evaluate(t);
                        }

                        transform.position = Vector3.Lerp(startPosition, waypoint.position, t);

                        if (lookAtTarget != null)
                        {
                            Vector3 lookPosition = lookAtTarget.position + waypoint.lookAtOffset;
                            Quaternion targetRotation = Quaternion.LookRotation(lookPosition - transform.position);
                            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                        }

                        yield return null;
                    }

                    transform.position = waypoint.position;
                }
            }
            while (loop);

            isPlaying = false;
            OnSequenceComplete?.Invoke();
            activeSequence = null;
        }
    }
}
