// Editor window to preview transition effects without entering Play Mode.
// Menu: Metal Pod > Transition Preview

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace MetalPod.Editor
{
    public class TransitionPreviewWindow : EditorWindow
    {
        private enum PreviewType { Fade, Wipe, Dissolve }

        private PreviewType _previewType = PreviewType.Fade;
        private float _progress = 0f;
        private Material _previewMaterial;
        private Color _fadeColor = Color.black;
        private Color _edgeColor = new Color(1f, 0.3f, 0f, 1f);

        [MenuItem("Metal Pod/Transition Preview")]
        public static void ShowWindow()
        {
            GetWindow<TransitionPreviewWindow>("Transition Preview");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Transition Preview", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _previewType = (PreviewType)EditorGUILayout.EnumPopup("Transition Type", _previewType);
            _progress = EditorGUILayout.Slider("Progress", _progress, 0f, 1f);

            if (_previewType == PreviewType.Fade)
                _fadeColor = EditorGUILayout.ColorField("Fade Color", _fadeColor);
            else if (_previewType == PreviewType.Dissolve)
                _edgeColor = EditorGUILayout.ColorField("Edge Color", _edgeColor);

            EditorGUILayout.Space();

            // Preview rect
            Rect previewRect = GUILayoutUtility.GetRect(300, 200, GUILayout.ExpandWidth(true));

            // Draw a simple colored rect to simulate the transition
            EditorGUI.DrawRect(previewRect, Color.gray); // "Scene" background

            Color overlayColor;
            float alpha = _progress;

            switch (_previewType)
            {
                case PreviewType.Fade:
                    overlayColor = new Color(_fadeColor.r, _fadeColor.g, _fadeColor.b, alpha);
                    EditorGUI.DrawRect(previewRect, overlayColor);
                    break;
                case PreviewType.Wipe:
                    float wipeX = previewRect.x + previewRect.width * _progress;
                    Rect wipeRect = new Rect(previewRect.x, previewRect.y,
                        wipeX - previewRect.x, previewRect.height);
                    EditorGUI.DrawRect(wipeRect, Color.black);
                    break;
                case PreviewType.Dissolve:
                    overlayColor = new Color(_edgeColor.r, _edgeColor.g, _edgeColor.b, alpha);
                    EditorGUI.DrawRect(previewRect, overlayColor);
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "This is a simplified preview. Full shader-based transitions are visible in Play Mode.",
                MessageType.Info);
        }
    }
}
#endif
