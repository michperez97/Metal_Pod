# Codex Task 21: Critical Bug Fixes from Round 4 Code Review

> **Goal**: Fix 4 critical issues found during code review of Round 4 agent output. All fixes are surgical — small, targeted changes to existing files.

---

## Context

A code review of Tasks 11–15 identified 4 critical bugs that need fixing before the systems are production-ready. Each fix is small (1–15 lines changed) but important.

**Read these files before making changes:**
- `Assets/Scripts/Transitions/SceneTransitionManager.cs` — Fixes C1
- `Assets/Scripts/Transitions/EnvironmentTransition.cs` — Fix C2
- `Assets/Scripts/Transitions/TransitionBase.cs` — Fix C1 (understand the base class)
- `Assets/Scripts/Debug/DebugCommands.cs` — Fixes C3, C4
- `Assets/Scripts/Hovercraft/HovercraftHealth.cs` — Fix C4

---

## Fix C1: Shared Material Mutation (SceneTransitionManager.cs)

### Problem
Transition classes call `.SetFloat()` / `.SetColor()` on serialized `Material` assets directly. In the Unity Editor, this **permanently modifies** the `.mat` asset files at runtime, causing changes to persist after exiting Play Mode. In builds, if two transitions share a material reference, property writes will conflict.

### Fix
In `SceneTransitionManager.Awake()`, instantiate runtime copies of the materials. Destroy them in `OnDestroy()`.

### Exact Changes

In `Assets/Scripts/Transitions/SceneTransitionManager.cs`:

**After** line 45 (`Instance = this;`), **before** the `if (transitionCanvas != null)` block, add:

```csharp
            // Instantiate runtime material copies to avoid modifying asset files
            if (fadeMaterial != null) fadeMaterial = new Material(fadeMaterial);
            if (wipeMaterial != null) wipeMaterial = new Material(wipeMaterial);
            if (dissolveMaterial != null) dissolveMaterial = new Material(dissolveMaterial);
```

In `OnDestroy()`, **before** `if (Instance == this) Instance = null;`, add cleanup:

```csharp
            // Clean up instantiated materials
            if (fadeMaterial != null) Destroy(fadeMaterial);
            if (wipeMaterial != null) Destroy(wipeMaterial);
            if (dissolveMaterial != null) Destroy(dissolveMaterial);
```

The full `OnDestroy` should become:
```csharp
        private void OnDestroy()
        {
            // Clean up instantiated materials
            if (fadeMaterial != null) Destroy(fadeMaterial);
            if (wipeMaterial != null) Destroy(wipeMaterial);
            if (dissolveMaterial != null) Destroy(dissolveMaterial);

            if (Instance == this) Instance = null;
        }
```

---

## Fix C2: Dissolve Noise Texture Fallback (EnvironmentTransition.cs)

### Problem
The dissolve shader (`TransitionDissolve.shader`) samples `_NoiseTex` for the dissolve pattern. The C# code never assigns this texture. Without a noise texture assigned on the material asset, the shader default is `"white"` (all 1.0), causing the dissolve to degenerate into a hard snap instead of a pattern dissolve.

### Fix
Generate a procedural Perlin noise texture at runtime and assign it to the material when no noise texture is present.

### Exact Changes

In `Assets/Scripts/Transitions/EnvironmentTransition.cs`:

**Add** a new static field for the noise texture property ID after line 25:

```csharp
        private static readonly int NoiseTexProp = Shader.PropertyToID("_NoiseTex");
```

**Add** a static method to generate a procedural noise texture:

```csharp
        private static Texture2D _cachedNoise;

        private static Texture2D GetOrCreateNoiseTexture()
        {
            if (_cachedNoise != null) return _cachedNoise;

            const int size = 256;
            _cachedNoise = new Texture2D(size, size, TextureFormat.R8, false);
            _cachedNoise.wrapMode = TextureWrapMode.Repeat;
            _cachedNoise.filterMode = FilterMode.Bilinear;

            var pixels = new Color[size * size];
            float scale = 8f;
            float offsetX = UnityEngine.Random.Range(0f, 100f);
            float offsetY = UnityEngine.Random.Range(0f, 100f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / size * scale + offsetX;
                    float ny = (float)y / size * scale + offsetY;
                    float value = Mathf.PerlinNoise(nx, ny);
                    // Add a second octave for more detail
                    value = value * 0.7f + Mathf.PerlinNoise(nx * 2.5f, ny * 2.5f) * 0.3f;
                    pixels[y * size + x] = new Color(value, value, value, 1f);
                }
            }

            _cachedNoise.SetPixels(pixels);
            _cachedNoise.Apply();
            return _cachedNoise;
        }
```

**Modify** `ApplyThemeColors()` — at the **end** of the method, after the `SetFloat(EdgeWidthProp, 0.08f)` line, add:

```csharp
            // Ensure a noise texture is assigned for the dissolve pattern
            if (TransitionMaterial.GetTexture(NoiseTexProp) == null)
            {
                TransitionMaterial.SetTexture(NoiseTexProp, GetOrCreateNoiseTexture());
            }
```

---

## Fix C3: Timescale Zero Freeze (DebugCommands.cs)

### Problem
The `timescale` command sets `Time.fixedDeltaTime = 0.02f * scale`. When `scale` is 0, `fixedDeltaTime` becomes 0, causing Unity to run `FixedUpdate` infinitely per frame, **freezing the engine**.

### Fix
One-line change. Clamp the `fixedDeltaTime` multiplier to a minimum of `0.01f`.

### Exact Changes

In `Assets/Scripts/Debug/DebugCommands.cs`, line 107:

**Replace:**
```csharp
                    Time.fixedDeltaTime = 0.02f * Mathf.Max(0f, scale);
```

**With:**
```csharp
                    Time.fixedDeltaTime = 0.02f * Mathf.Max(0.01f, scale);
```

That's the entire fix.

---

## Fix C4: God Mode Reflection Fragile Under IL2CPP (DebugCommands.cs + HovercraftHealth.cs)

### Problem
The `god` command uses reflection to find the `private set` accessor of `IsInvincible`. While this works on Mono, IL2CPP stripping in release-dev builds may remove the setter, causing the command to silently fail with "Unable to toggle invincibility."

### Fix
Add a public `SetInvincible(bool)` method to `HovercraftHealth.cs` (guarded by `#if DEVELOPMENT_BUILD || UNITY_EDITOR` so it's stripped from release), then use that method directly in `DebugCommands.cs` instead of reflection.

### Exact Changes

**Step 1:** In `Assets/Scripts/Hovercraft/HovercraftHealth.cs`, add the following method **after** the existing `ActivateInvincibility(float)` method (after the closing brace around line 218):

```csharp
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        /// <summary>
        /// Debug-only: directly set invincibility state. Stripped from release builds.
        /// </summary>
        public void SetInvincible(bool enabled)
        {
            IsInvincible = enabled;
            _invincibilityTimer = enabled ? 99999f : 0f;
        }
#endif
```

**Step 2:** In `Assets/Scripts/Debug/DebugCommands.cs`, **replace** the entire `TrySetGodMode` method (lines 732–757) with:

```csharp
        private static bool TrySetGodMode(HovercraftHealth health, bool enabled)
        {
            if (health == null)
            {
                return false;
            }

            health.SetInvincible(enabled);
            return true;
        }
```

This is safe because both files are compiled under the same `#if DEVELOPMENT_BUILD || UNITY_EDITOR` guard — `DebugCommands.cs` is already fully wrapped in that guard (line 1), and the new `SetInvincible` method is also guarded.

---

## Files Modified

```
Assets/Scripts/Transitions/SceneTransitionManager.cs  ← Fix C1 (material instantiation)
Assets/Scripts/Transitions/EnvironmentTransition.cs    ← Fix C2 (noise texture fallback)
Assets/Scripts/Debug/DebugCommands.cs                  ← Fix C3 (timescale clamp) + Fix C4 (remove reflection)
Assets/Scripts/Hovercraft/HovercraftHealth.cs          ← Fix C4 (add SetInvincible method)
```

---

## Acceptance Criteria

- [ ] `SceneTransitionManager.Awake()` instantiates material copies (not modifying asset materials)
- [ ] `SceneTransitionManager.OnDestroy()` destroys the instantiated material copies
- [ ] `EnvironmentTransition` generates and assigns a procedural Perlin noise texture when `_NoiseTex` is null
- [ ] Noise texture is cached statically (only generated once)
- [ ] `timescale 0` command does NOT freeze Unity (`fixedDeltaTime` stays > 0)
- [ ] `god` command uses direct method call, not reflection
- [ ] `HovercraftHealth.SetInvincible(bool)` exists, guarded by `#if DEVELOPMENT_BUILD || UNITY_EDITOR`
- [ ] No other files are modified beyond the 4 listed above
- [ ] All changes compile without errors
- [ ] Existing tests still pass (no behavior change in non-debug paths)
