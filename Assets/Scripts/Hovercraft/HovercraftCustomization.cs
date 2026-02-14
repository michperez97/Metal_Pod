using System;
using System.Collections.Generic;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Hovercraft
{
    public class HovercraftCustomization : MonoBehaviour
    {
        [Serializable]
        private struct ColorSchemePreset
        {
            public string cosmeticId;
            public Color primary;
            public Color secondary;
            public Color accent;
        }

        [Serializable]
        private struct DecalPreset
        {
            public string cosmeticId;
            public Texture2D decalTexture;
        }

        [Serializable]
        private struct PartPreset
        {
            public string cosmeticId;
            public GameObject partPrefab;
            public string attachPointName;
        }

        [Header("Renderers")]
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private Renderer accentRenderer;
        [SerializeField] private Renderer decalTargetRenderer;

        [Header("Attachment Points")]
        [SerializeField] private Transform[] attachPoints;

        [Header("Defaults")]
        [SerializeField] private Color defaultPrimary = new Color(0.93f, 0.75f, 0.16f);
        [SerializeField] private Color defaultSecondary = new Color(0.12f, 0.12f, 0.12f);
        [SerializeField] private Color defaultAccent = new Color(1f, 0.55f, 0.1f);
        [SerializeField] private Texture2D defaultDecal;

        [Header("Cosmetic Presets")]
        [SerializeField] private ColorSchemePreset[] colorPresets;
        [SerializeField] private DecalPreset[] decalPresets;
        [SerializeField] private PartPreset[] partPresets;

        private readonly Dictionary<string, Transform> _attachPointLookup = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, GameObject> _attachedPartsByPoint = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
        private MaterialPropertyBlock _bodyBlock;
        private MaterialPropertyBlock _accentBlock;
        private MaterialPropertyBlock _decalBlock;
        private Color _currentPrimary;
        private Color _currentSecondary;
        private Color _currentAccent;
        private Texture2D _currentDecal;

        private void Awake()
        {
            _bodyBlock = new MaterialPropertyBlock();
            _accentBlock = new MaterialPropertyBlock();
            _decalBlock = new MaterialPropertyBlock();

            BuildAttachPointLookup();

            _currentPrimary = defaultPrimary;
            _currentSecondary = defaultSecondary;
            _currentAccent = defaultAccent;
            _currentDecal = defaultDecal;
            ApplyColorProperties();
            ApplyDecalProperties();
        }

        private void OnEnable()
        {
            EventBus.OnCosmeticEquipped += HandleCosmeticEquipped;
        }

        private void OnDisable()
        {
            EventBus.OnCosmeticEquipped -= HandleCosmeticEquipped;
        }

        public void ApplyColorScheme(Color primary, Color secondary, Color accent)
        {
            _currentPrimary = primary;
            _currentSecondary = secondary;
            _currentAccent = accent;
            ApplyColorProperties();
        }

        public void ApplyDecal(Texture2D decalTexture)
        {
            _currentDecal = decalTexture;
            ApplyDecalProperties();
        }

        public void AttachPart(GameObject partPrefab, string attachPointName)
        {
            if (partPrefab == null || string.IsNullOrWhiteSpace(attachPointName))
            {
                return;
            }

            if (!_attachPointLookup.TryGetValue(attachPointName, out Transform attachPoint) || attachPoint == null)
            {
                return;
            }

            if (_attachedPartsByPoint.TryGetValue(attachPointName, out GameObject existing) && existing != null)
            {
                Destroy(existing);
            }

            GameObject instance = Instantiate(partPrefab, attachPoint);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            _attachedPartsByPoint[attachPointName] = instance;
        }

        public void ClearCustomizations()
        {
            _currentPrimary = defaultPrimary;
            _currentSecondary = defaultSecondary;
            _currentAccent = defaultAccent;
            _currentDecal = defaultDecal;

            ApplyColorProperties();
            ApplyDecalProperties();

            foreach (KeyValuePair<string, GameObject> pair in _attachedPartsByPoint)
            {
                if (pair.Value != null)
                {
                    Destroy(pair.Value);
                }
            }

            _attachedPartsByPoint.Clear();
        }

        private void HandleCosmeticEquipped(string cosmeticId)
        {
            if (string.IsNullOrWhiteSpace(cosmeticId))
            {
                return;
            }

            for (int i = 0; i < colorPresets.Length; i++)
            {
                if (!string.Equals(colorPresets[i].cosmeticId, cosmeticId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                ApplyColorScheme(colorPresets[i].primary, colorPresets[i].secondary, colorPresets[i].accent);
                return;
            }

            for (int i = 0; i < decalPresets.Length; i++)
            {
                if (!string.Equals(decalPresets[i].cosmeticId, cosmeticId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                ApplyDecal(decalPresets[i].decalTexture);
                return;
            }

            for (int i = 0; i < partPresets.Length; i++)
            {
                if (!string.Equals(partPresets[i].cosmeticId, cosmeticId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                AttachPart(partPresets[i].partPrefab, partPresets[i].attachPointName);
                return;
            }
        }

        private void BuildAttachPointLookup()
        {
            _attachPointLookup.Clear();
            if (attachPoints == null)
            {
                return;
            }

            for (int i = 0; i < attachPoints.Length; i++)
            {
                Transform attachPoint = attachPoints[i];
                if (attachPoint == null || string.IsNullOrWhiteSpace(attachPoint.name))
                {
                    continue;
                }

                _attachPointLookup[attachPoint.name] = attachPoint;
            }
        }

        private void ApplyColorProperties()
        {
            if (bodyRenderer != null)
            {
                bodyRenderer.GetPropertyBlock(_bodyBlock);
                _bodyBlock.SetColor("_Color", _currentPrimary);
                _bodyBlock.SetColor("_BaseColor", _currentPrimary);
                bodyRenderer.SetPropertyBlock(_bodyBlock);
            }

            if (accentRenderer != null)
            {
                accentRenderer.GetPropertyBlock(_accentBlock);
                _accentBlock.SetColor("_Color", _currentSecondary);
                _accentBlock.SetColor("_BaseColor", _currentSecondary);
                _accentBlock.SetColor("_EmissionColor", _currentAccent);
                accentRenderer.SetPropertyBlock(_accentBlock);
            }
        }

        private void ApplyDecalProperties()
        {
            if (decalTargetRenderer == null)
            {
                return;
            }

            decalTargetRenderer.GetPropertyBlock(_decalBlock);
            _decalBlock.SetTexture("_MainTex", _currentDecal);
            _decalBlock.SetTexture("_BaseMap", _currentDecal);
            decalTargetRenderer.SetPropertyBlock(_decalBlock);
        }
    }
}
