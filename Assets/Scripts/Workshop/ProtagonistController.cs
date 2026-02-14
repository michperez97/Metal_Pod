using UnityEngine;

namespace MetalPod.Workshop
{
    public class ProtagonistController : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private static readonly int IdleHash = Animator.StringToHash("Idle");
        private static readonly int WorkingHash = Animator.StringToHash("Working");
        private static readonly int CelebratingHash = Animator.StringToHash("Celebrating");

        public void SetIdle()
        {
            CrossFade(IdleHash);
        }

        public void SetWorking()
        {
            CrossFade(WorkingHash);
        }

        public void SetCelebrating()
        {
            CrossFade(CelebratingHash);
        }

        private void CrossFade(int stateHash)
        {
            if (animator == null)
            {
                return;
            }

            animator.CrossFade(stateHash, 0.2f);
        }
    }
}
