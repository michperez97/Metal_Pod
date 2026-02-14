using System.Collections;
using UnityEngine;

namespace MetalPod.Workshop
{
    public class WorkshopCameraController : MonoBehaviour
    {
        [Header("View Targets")]
        [SerializeField] private Transform defaultView;
        [SerializeField] private Transform hovercraftView;
        [SerializeField] private Transform mapView;

        [Header("Motion")]
        [SerializeField] private float transitionDuration = 0.55f;
        [SerializeField] private AnimationCurve transitionCurve = null;

        private Coroutine _transitionRoutine;

        private void Awake()
        {
            if (transitionCurve == null || transitionCurve.length == 0)
            {
                transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }
        }

        public void FocusOnDefault()
        {
            TransitionTo(defaultView);
        }

        public void FocusOnHovercraft()
        {
            TransitionTo(hovercraftView != null ? hovercraftView : defaultView);
        }

        public void FocusOnMap()
        {
            TransitionTo(mapView != null ? mapView : defaultView);
        }

        public void SnapTo(Transform target)
        {
            if (target == null)
            {
                return;
            }

            if (_transitionRoutine != null)
            {
                StopCoroutine(_transitionRoutine);
                _transitionRoutine = null;
            }

            transform.SetPositionAndRotation(target.position, target.rotation);
        }

        private void TransitionTo(Transform target)
        {
            if (target == null)
            {
                return;
            }

            if (_transitionRoutine != null)
            {
                StopCoroutine(_transitionRoutine);
            }

            _transitionRoutine = StartCoroutine(TransitionRoutine(target));
        }

        private IEnumerator TransitionRoutine(Transform target)
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            Vector3 endPos = target.position;
            Quaternion endRot = target.rotation;

            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float linear = transitionDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / transitionDuration);
                float curved = transitionCurve.Evaluate(linear);

                transform.position = Vector3.Lerp(startPos, endPos, curved);
                transform.rotation = Quaternion.Slerp(startRot, endRot, curved);
                yield return null;
            }

            transform.SetPositionAndRotation(endPos, endRot);
            _transitionRoutine = null;
        }
    }
}
