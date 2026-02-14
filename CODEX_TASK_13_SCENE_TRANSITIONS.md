# Codex Task 13: Scene Transitions & Loading System

> **Goal**: Create a visual scene transition system with fade, wipe, and environment-themed transitions. Enhances the existing `SceneLoader.cs` with proper visual polish between scenes.

---

## Context

The existing `SceneLoader.cs` (`Assets/Scripts/Core/SceneLoader.cs`) handles async scene loading and has a `BindLoadingScreen(Action<bool>)` callback, but there are **no visual transitions** — scenes just pop in/out. The `LoadingScreenUI.cs` exists but has no fade or animation backing it.

This task creates:
- A transition overlay system (Canvas-based, renders on top of everything)
- Multiple transition types (fade, directional wipe, environment-themed)
- Integration hooks for `SceneLoader` without modifying it (event-driven)
- A loading progress bar with tips/flavor text during long loads

**Read these files**:
- `Assets/Scripts/Core/SceneLoader.cs` — Events: `OnLoadingScreenShown`, `OnLoadingScreenHidden`, `OnScenePreloaded`, method `BindLoadingScreen`
- `Assets/Scripts/UI/LoadingScreenUI.cs` — Existing loading screen (review for integration)
- `Assets/Scripts/Core/GameManager.cs` — `SceneLoader` property
- `Assets/Scripts/Shared/GameConstants.cs` — Scene name constants
- `Assets/Scripts/UI/HeavyMetalTheme.cs` — Theme colors/fonts for UI consistency
- `Assets/Shaders/UI/` — Existing UI shaders

---

## Files to Create

```
Assets/Scripts/Transitions/
├── SceneTransitionManager.cs     # Singleton controlling all transitions
├── TransitionBase.cs             # Abstract base for transition effects
├── FadeTransition.cs             # Simple fade to black / from black
├── WipeTransition.cs             # Directional wipe (left, right, up, down)
├── EnvironmentTransition.cs      # Themed transitions (lava dissolve, ice freeze, toxic drip)
├── TransitionConfig.cs           # Maps scene names to transition types
└── LoadingTips.cs                # Loading screen flavor text

Assets/Shaders/Transitions/
├── TransitionFade.shader         # Simple alpha fade
├── TransitionWipe.shader         # Directional wipe with soft edge
└── TransitionDissolve.shader     # Noise-based dissolve for environment themes

Assets/Scripts/Editor/
└── TransitionPreviewWindow.cs    # Editor window to preview transitions
```

**DO NOT modify** any existing files.

---

## Architecture

```
SceneTransitionManager (Singleton on _Persistent scene)
  ├── Canvas (ScreenSpaceOverlay, sortOrder 9999)
  │   ├── TransitionImage (full-screen RawImage with transition material)
  │   └── LoadingPanel
  │       ├── ProgressBar
  │       ├── TipText
  │       └── EnvironmentIcon
  ├── TransitionIn(type, duration, callback) → plays transition covering screen
  ├── TransitionOut(type, duration, callback) → plays transition revealing screen
  ├── TransitionBetweenScenes(sceneName) → full in→load→out sequence
  └── TransitionConfig maps scene names to transition types:
      ├── MainMenu → FadeTransition (black)
      ├── Workshop → FadeTransition (black)
      ├── Lava courses → EnvironmentTransition (lava/fire)
      ├── Ice courses → EnvironmentTransition (ice/frost)
      └── Toxic courses → EnvironmentTransition (toxic/acid green)
```

---

## Detailed Specifications

### TransitionBase.cs

```csharp
// Abstract base class for all scene transitions.
// Subclasses implement the visual effect by controlling a Material.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MetalPod.Transitions
{
    public abstract class TransitionBase
    {
        protected Material TransitionMaterial;
        protected RawImage TargetImage;

        public bool IsPlaying { get; protected set; }

        public void SetTarget(RawImage image, Material mat)
        {
            TargetImage = image;
            TransitionMaterial = mat;
        }

        /// <summary>
        /// Play transition IN (screen becomes covered).
        /// Progress goes from 0 (fully visible) to 1 (fully covered).
        /// </summary>
        public abstract IEnumerator PlayIn(float duration, Action onComplete);

        /// <summary>
        /// Play transition OUT (screen becomes revealed).
        /// Progress goes from 1 (fully covered) to 0 (fully visible).
        /// </summary>
        public abstract IEnumerator PlayOut(float duration, Action onComplete);

        /// <summary>
        /// Set the transition progress directly (for scrubbing in editor preview).
        /// </summary>
        public abstract void SetProgress(float t);
    }
}
```

### FadeTransition.cs

```csharp
// Simple fade to/from a solid color (default: black).
// Uses TransitionFade.shader.

using System;
using System.Collections;
using UnityEngine;

namespace MetalPod.Transitions
{
    public class FadeTransition : TransitionBase
    {
        private static readonly int ProgressProp = Shader.PropertyToID("_Progress");
        private static readonly int ColorProp = Shader.PropertyToID("_FadeColor");

        private Color _fadeColor;

        public FadeTransition(Color fadeColor)
        {
            _fadeColor = fadeColor;
        }

        public override IEnumerator PlayIn(float duration, Action onComplete)
        {
            IsPlaying = true;
            TransitionMaterial.SetColor(ColorProp, _fadeColor);
            TargetImage.enabled = true;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetProgress(t);
                yield return null;
            }
            SetProgress(1f);
            IsPlaying = false;
            onComplete?.Invoke();
        }

        public override IEnumerator PlayOut(float duration, Action onComplete)
        {
            IsPlaying = true;
            TransitionMaterial.SetColor(ColorProp, _fadeColor);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = 1f - Mathf.Clamp01(elapsed / duration);
                SetProgress(t);
                yield return null;
            }
            SetProgress(0f);
            TargetImage.enabled = false;
            IsPlaying = false;
            onComplete?.Invoke();
        }

        public override void SetProgress(float t)
        {
            TransitionMaterial.SetFloat(ProgressProp, t);
        }
    }
}
```

### WipeTransition.cs

```csharp
// Directional wipe transition with soft edge.
// Uses TransitionWipe.shader.

using System;
using System.Collections;
using UnityEngine;

namespace MetalPod.Transitions
{
    public enum WipeDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    public class WipeTransition : TransitionBase
    {
        private static readonly int ProgressProp = Shader.PropertyToID("_Progress");
        private static readonly int DirectionProp = Shader.PropertyToID("_Direction");
        private static readonly int SoftnessProp = Shader.PropertyToID("_Softness");

        private WipeDirection _direction;
        private float _softness;

        public WipeTransition(WipeDirection direction, float softness = 0.1f)
        {
            _direction = direction;
            _softness = softness;
        }

        private Vector2 GetDirectionVector()
        {
            switch (_direction)
            {
                case WipeDirection.Left: return new Vector2(-1f, 0f);
                case WipeDirection.Right: return new Vector2(1f, 0f);
                case WipeDirection.Up: return new Vector2(0f, 1f);
                case WipeDirection.Down: return new Vector2(0f, -1f);
                default: return new Vector2(1f, 0f);
            }
        }

        public override IEnumerator PlayIn(float duration, Action onComplete)
        {
            IsPlaying = true;
            TransitionMaterial.SetVector(DirectionProp, GetDirectionVector());
            TransitionMaterial.SetFloat(SoftnessProp, _softness);
            TargetImage.enabled = true;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Ease in-out for smooth feel
                t = t * t * (3f - 2f * t);
                SetProgress(t);
                yield return null;
            }
            SetProgress(1f);
            IsPlaying = false;
            onComplete?.Invoke();
        }

        public override IEnumerator PlayOut(float duration, Action onComplete)
        {
            IsPlaying = true;
            TransitionMaterial.SetVector(DirectionProp, GetDirectionVector());
            TransitionMaterial.SetFloat(SoftnessProp, _softness);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = 1f - Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);
                SetProgress(t);
                yield return null;
            }
            SetProgress(0f);
            TargetImage.enabled = false;
            IsPlaying = false;
            onComplete?.Invoke();
        }

        public override void SetProgress(float t)
        {
            TransitionMaterial.SetFloat(ProgressProp, t);
        }
    }
}
```

### EnvironmentTransition.cs

```csharp
// Environment-themed dissolve transition.
// Lava: fire/ember dissolve (orange/red)
// Ice: frost freeze pattern (white/blue)
// Toxic: acid drip dissolve (green/black)
// Uses TransitionDissolve.shader with a noise texture.

using System;
using System.Collections;
using UnityEngine;

namespace MetalPod.Transitions
{
    public enum EnvironmentTheme
    {
        Lava,
        Ice,
        Toxic,
        Neutral
    }

    public class EnvironmentTransition : TransitionBase
    {
        private static readonly int ProgressProp = Shader.PropertyToID("_Progress");
        private static readonly int EdgeColorProp = Shader.PropertyToID("_EdgeColor");
        private static readonly int EdgeWidthProp = Shader.PropertyToID("_EdgeWidth");
        private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");

        private EnvironmentTheme _theme;

        private static readonly Color LavaEdge = new Color(1f, 0.3f, 0f, 1f);   // Orange
        private static readonly Color LavaBase = new Color(0.15f, 0f, 0f, 1f);   // Dark red
        private static readonly Color IceEdge = new Color(0.6f, 0.85f, 1f, 1f);  // Light blue
        private static readonly Color IceBase = new Color(0.9f, 0.95f, 1f, 1f);  // Near white
        private static readonly Color ToxicEdge = new Color(0.2f, 1f, 0f, 1f);   // Bright green
        private static readonly Color ToxicBase = new Color(0.05f, 0.1f, 0f, 1f);// Dark green

        public EnvironmentTransition(EnvironmentTheme theme)
        {
            _theme = theme;
        }

        private void ApplyThemeColors()
        {
            Color edge, baseColor;
            switch (_theme)
            {
                case EnvironmentTheme.Lava:
                    edge = LavaEdge; baseColor = LavaBase; break;
                case EnvironmentTheme.Ice:
                    edge = IceEdge; baseColor = IceBase; break;
                case EnvironmentTheme.Toxic:
                    edge = ToxicEdge; baseColor = ToxicBase; break;
                default:
                    edge = Color.white; baseColor = Color.black; break;
            }

            TransitionMaterial.SetColor(EdgeColorProp, edge);
            TransitionMaterial.SetColor(BaseColorProp, baseColor);
            TransitionMaterial.SetFloat(EdgeWidthProp, 0.08f);
        }

        public override IEnumerator PlayIn(float duration, Action onComplete)
        {
            IsPlaying = true;
            ApplyThemeColors();
            TargetImage.enabled = true;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetProgress(t);
                yield return null;
            }
            SetProgress(1f);
            IsPlaying = false;
            onComplete?.Invoke();
        }

        public override IEnumerator PlayOut(float duration, Action onComplete)
        {
            IsPlaying = true;
            ApplyThemeColors();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = 1f - Mathf.Clamp01(elapsed / duration);
                SetProgress(t);
                yield return null;
            }
            SetProgress(0f);
            TargetImage.enabled = false;
            IsPlaying = false;
            onComplete?.Invoke();
        }

        public override void SetProgress(float t)
        {
            TransitionMaterial.SetFloat(ProgressProp, t);
        }
    }
}
```

### SceneTransitionManager.cs

```csharp
// Singleton managing scene transitions. Lives on _Persistent scene.
// Call: SceneTransitionManager.Instance.TransitionToScene("SceneName")
// Automatically selects transition type based on scene name.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MetalPod.Shared;

namespace MetalPod.Transitions
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Canvas transitionCanvas;
        [SerializeField] private RawImage transitionImage;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMPro.TextMeshProUGUI tipText;

        [Header("Materials")]
        [SerializeField] private Material fadeMaterial;
        [SerializeField] private Material wipeMaterial;
        [SerializeField] private Material dissolveMaterial;

        [Header("Timing")]
        [SerializeField] private float defaultTransitionInDuration = 0.5f;
        [SerializeField] private float defaultTransitionOutDuration = 0.5f;
        [SerializeField] private float minimumLoadingDisplayTime = 1f;

        private TransitionBase _currentTransition;
        private Coroutine _activeTransition;
        private bool _isTransitioning;

        public bool IsTransitioning => _isTransitioning;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (transitionCanvas != null)
            {
                transitionCanvas.sortingOrder = 9999;
            }
            if (transitionImage != null)
                transitionImage.enabled = false;
            if (loadingPanel != null)
                loadingPanel.SetActive(false);
        }

        /// <summary>
        /// Full scene transition: transition in → load scene → transition out.
        /// Uses SceneLoader from GameManager if available, else falls back to direct load.
        /// </summary>
        public void TransitionToScene(string sceneName, Action onComplete = null)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] Transition already in progress.");
                return;
            }

            TransitionBase transition = GetTransitionForScene(sceneName);
            if (_activeTransition != null) StopCoroutine(_activeTransition);
            _activeTransition = StartCoroutine(TransitionSequence(sceneName, transition, onComplete));
        }

        /// <summary>
        /// Play only a transition in (cover screen), then invoke callback.
        /// Useful for custom flows.
        /// </summary>
        public void PlayTransitionIn(TransitionBase transition, float duration, Action onComplete = null)
        {
            transition.SetTarget(transitionImage, GetMaterialForTransition(transition));
            StartCoroutine(transition.PlayIn(duration, onComplete));
        }

        /// <summary>
        /// Play only a transition out (reveal screen), then invoke callback.
        /// </summary>
        public void PlayTransitionOut(TransitionBase transition, float duration, Action onComplete = null)
        {
            transition.SetTarget(transitionImage, GetMaterialForTransition(transition));
            StartCoroutine(transition.PlayOut(duration, onComplete));
        }

        private IEnumerator TransitionSequence(string sceneName, TransitionBase transition, Action onComplete)
        {
            _isTransitioning = true;

            Material mat = GetMaterialForTransition(transition);
            transition.SetTarget(transitionImage, mat);

            // Phase 1: Transition In (cover screen)
            bool transInDone = false;
            yield return transition.PlayIn(defaultTransitionInDuration, () => transInDone = true);

            // Phase 2: Show loading panel and load scene
            if (loadingPanel != null) loadingPanel.SetActive(true);
            if (progressBar != null) progressBar.value = 0f;

            // Pick a random loading tip
            if (tipText != null)
                tipText.text = LoadingTips.GetRandomTip();

            float loadStart = Time.unscaledTime;

            // Load via SceneLoader if available
            var sceneLoader = MetalPod.Core.GameManager.Instance?.SceneLoader;
            bool loadComplete = false;

            if (sceneLoader != null)
            {
                sceneLoader.LoadSceneAsync(sceneName,
                    onProgress: p => { if (progressBar != null) progressBar.value = p; },
                    onComplete: () => loadComplete = true,
                    showLoadingScreen: false);
            }
            else
            {
                // Fallback direct load
                var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
                while (op != null && !op.isDone)
                {
                    if (progressBar != null) progressBar.value = op.progress;
                    yield return null;
                }
                loadComplete = true;
            }

            // Wait for load
            while (!loadComplete) yield return null;

            // Ensure minimum display time for loading screen
            float elapsed = Time.unscaledTime - loadStart;
            if (elapsed < minimumLoadingDisplayTime)
            {
                yield return new WaitForSecondsRealtime(minimumLoadingDisplayTime - elapsed);
            }

            if (progressBar != null) progressBar.value = 1f;

            // Phase 3: Hide loading panel, Transition Out (reveal screen)
            if (loadingPanel != null) loadingPanel.SetActive(false);

            bool transOutDone = false;
            yield return transition.PlayOut(defaultTransitionOutDuration, () => transOutDone = true);

            _isTransitioning = false;
            _activeTransition = null;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Map scene names to transition types.
        /// </summary>
        private TransitionBase GetTransitionForScene(string sceneName)
        {
            // Environment-themed transitions for courses
            if (sceneName.Contains("Inferno") || sceneName.Contains("Molten") || sceneName.Contains("Magma"))
                return new EnvironmentTransition(EnvironmentTheme.Lava);

            if (sceneName.Contains("Frozen") || sceneName.Contains("Glacial") || sceneName.Contains("Arctic"))
                return new EnvironmentTransition(EnvironmentTheme.Ice);

            if (sceneName.Contains("Rust") || sceneName.Contains("Chemical") || sceneName.Contains("Biohazard"))
                return new EnvironmentTransition(EnvironmentTheme.Toxic);

            if (sceneName == GameConstants.SCENE_WORKSHOP)
                return new WipeTransition(WipeDirection.Right);

            if (sceneName == GameConstants.SCENE_MAIN_MENU)
                return new FadeTransition(Color.black);

            // Default
            return new FadeTransition(Color.black);
        }

        private Material GetMaterialForTransition(TransitionBase transition)
        {
            if (transition is FadeTransition) return fadeMaterial;
            if (transition is WipeTransition) return wipeMaterial;
            if (transition is EnvironmentTransition) return dissolveMaterial;
            return fadeMaterial;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
```

### LoadingTips.cs

```csharp
// Static collection of loading screen tips and flavor text.
// Heavy metal themed to match the game aesthetic.

namespace MetalPod.Transitions
{
    public static class LoadingTips
    {
        private static readonly string[] Tips = new[]
        {
            "Tilt your device to steer. Lean into it!",
            "Tap the right side of the screen to activate boost.",
            "Tap the left side to brake — useful for tight corners.",
            "Upgrade Speed to increase your top velocity.",
            "Armor upgrades help you survive more hazard hits.",
            "Handling upgrades make steering more responsive.",
            "Boost upgrades give you longer, more powerful bursts.",
            "Gold medals require near-perfect runs. Practice makes perfect.",
            "Checkpoints save your position. Hit every one!",
            "Collect bolts during races to fund your upgrades.",
            "Replaying courses still earns bolts (at 50% rate).",
            "Shield absorbs damage before your health takes hits.",
            "Below 50% health, your speed is reduced.",
            "Below 25% health, handling also suffers.",
            "Lava geysers erupt on a timer. Learn the pattern.",
            "Ice patches reduce your traction. Keep your speed steady.",
            "Toxic gas clouds deal damage over time. Don't linger.",
            "Exploding barrels have a warning flash before detonation.",
            "Electric fences cycle on and off. Time your pass.",
            "The Workshop protagonist celebrates when you buy upgrades!",
            "Each environment has unique hazards. Adapt your strategy.",
            "Metal never dies. Neither should your hovercraft.",
            "No ads. No microtransactions. Just pure racing.",
        };

        private static System.Random _random = new System.Random();

        public static string GetRandomTip()
        {
            return Tips[_random.Next(Tips.Length)];
        }
    }
}
```

### TransitionConfig.cs

```csharp
// ScriptableObject mapping scenes to transition configurations.
// Optional alternative to the hardcoded mapping in SceneTransitionManager.

using UnityEngine;

namespace MetalPod.Transitions
{
    [CreateAssetMenu(fileName = "TransitionConfig", menuName = "MetalPod/Transition Config")]
    public class TransitionConfig : ScriptableObject
    {
        [System.Serializable]
        public class SceneTransitionMapping
        {
            public string sceneName;
            public TransitionType transitionType;
            public EnvironmentTheme environmentTheme;
            public WipeDirection wipeDirection;
            public Color fadeColor = Color.black;
            public float transitionInDuration = 0.5f;
            public float transitionOutDuration = 0.5f;
        }

        public enum TransitionType
        {
            Fade,
            Wipe,
            EnvironmentDissolve
        }

        public SceneTransitionMapping[] mappings;
        public SceneTransitionMapping defaultMapping;
    }
}
```

---

## SHADERS

### TransitionFade.shader

```hlsl
Shader "MetalPod/Transitions/Fade"
{
    Properties
    {
        _Progress ("Progress", Range(0, 1)) = 0
        _FadeColor ("Fade Color", Color) = (0, 0, 0, 1)
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            float _Progress;
            float4 _FadeColor;

            Varyings vert(Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.uv = i.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                return half4(_FadeColor.rgb, _Progress);
            }
            ENDHLSL
        }
    }
}
```

### TransitionWipe.shader

```hlsl
Shader "MetalPod/Transitions/Wipe"
{
    Properties
    {
        _Progress ("Progress", Range(0, 1)) = 0
        _Direction ("Direction", Vector) = (1, 0, 0, 0)
        _Softness ("Edge Softness", Range(0, 0.5)) = 0.1
        _Color ("Color", Color) = (0, 0, 0, 1)
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            float _Progress;
            float2 _Direction;
            float _Softness;
            float4 _Color;

            Varyings vert(Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.uv = i.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // Project UV onto direction to get 0..1 gradient
                float2 dir = normalize(_Direction);
                float2 centeredUV = i.uv - 0.5;
                float proj = dot(centeredUV, dir) + 0.5; // Remap to 0..1

                // Wipe edge
                float edge = smoothstep(_Progress - _Softness, _Progress + _Softness, proj);
                float alpha = 1.0 - edge;

                return half4(_Color.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
```

### TransitionDissolve.shader

```hlsl
Shader "MetalPod/Transitions/Dissolve"
{
    Properties
    {
        _Progress ("Progress", Range(0, 1)) = 0
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _EdgeColor ("Edge Color", Color) = (1, 0.3, 0, 1)
        _BaseColor ("Base Color", Color) = (0, 0, 0, 1)
        _EdgeWidth ("Edge Width", Range(0, 0.2)) = 0.08
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);
            float _Progress;
            float4 _EdgeColor;
            float4 _BaseColor;
            float _EdgeWidth;

            Varyings vert(Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.uv = i.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.uv).r;

                // Dissolve threshold
                float threshold = _Progress;
                float diff = noise - threshold;

                // Alpha: fully opaque where dissolved
                float alpha = step(diff, 0.0);

                // Edge glow
                float edgeMask = 1.0 - smoothstep(0.0, _EdgeWidth, abs(diff));

                half3 color = lerp(_BaseColor.rgb, _EdgeColor.rgb, edgeMask * alpha);

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
```

### TransitionPreviewWindow.cs

```csharp
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
```

---

## Acceptance Criteria

- [ ] `TransitionBase.cs` — Abstract base with PlayIn/PlayOut/SetProgress
- [ ] `FadeTransition.cs` — Fade to/from color
- [ ] `WipeTransition.cs` — Directional wipe with softness
- [ ] `EnvironmentTransition.cs` — Themed dissolve for Lava/Ice/Toxic
- [ ] `SceneTransitionManager.cs` — Singleton with auto-theme selection, full transition sequence
- [ ] `LoadingTips.cs` — 20+ heavy metal themed tips
- [ ] `TransitionConfig.cs` — ScriptableObject for scene-to-transition mapping
- [ ] `TransitionFade.shader` — URP-compatible fade shader
- [ ] `TransitionWipe.shader` — URP-compatible directional wipe shader
- [ ] `TransitionDissolve.shader` — URP-compatible noise dissolve with edge glow
- [ ] `TransitionPreviewWindow.cs` — Editor preview tool
- [ ] All C# in `MetalPod.Transitions` or `MetalPod.Editor` namespaces
- [ ] Shaders use URP HLSL (not legacy CG)
- [ ] Uses `Time.unscaledDeltaTime` for transitions (works during Time.timeScale = 0)
- [ ] No modifications to existing files
- [ ] Compiles without errors
