using UnityEngine;

namespace MetalPod.Tutorial
{
    public class TutorialHighlight : MonoBehaviour
    {
        [SerializeField] private string highlightId;

        public string HighlightId => highlightId;
    }
}
