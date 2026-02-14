# Codex Task 2: Custom Shaders & Visual Effects

> **Goal**: Write all shader files the game needs for environmental effects, UI, and hovercraft visuals. These are `.shader` and `.hlsl` files that can be written without Unity open.

---

## Context

Metal Pod is a Unity 2022 LTS game using **URP (Universal Render Pipeline)** targeting iOS. All shaders must be mobile-performant. The game has 3 environments (Lava, Ice, Toxic) each needing distinct visual effects, plus vehicle and UI shaders.

**Read these files for context**:
- `IMPLEMENTATION_PLAN.md` — visual requirements
- `AGENT_2_HOVERCRAFT_VEHICLE.md` — vehicle VFX specs
- `AGENT_3_COURSES_AND_HAZARDS.md` — environment art direction
- `AGENT_5_UI_AND_WORKSHOP.md` — UI theme specs

---

## Files to Create

```
Assets/Shaders/
├── Environment/
│   ├── Lava/
│   │   ├── LavaSurface.shader         # Flowing animated lava with emissive glow
│   │   ├── HeatDistortion.shader      # Screen space heat shimmer
│   │   ├── VolcanicRock.shader        # Rock with emissive magma veins
│   │   └── LavaGeyserSpray.shader    # Particle shader for geyser eruptions
│   ├── Ice/
│   │   ├── IceSurface.shader          # Glossy reflective ice with subsurface
│   │   ├── FrostOverlay.shader        # Screen frost vignette effect
│   │   ├── IceCrystal.shader          # Transparent refractive ice crystals
│   │   └── SnowParticle.shader       # Soft particle shader for snow
│   └── Toxic/
│       ├── ToxicSludge.shader         # Animated toxic liquid with glow
│       ├── ToxicScreenEffect.shader   # Green tint + chromatic aberration
│       ├── RustedMetal.shader         # Rust with detail mapping
│       └── ElectricArc.shader        # Animated electricity for fences
├── Hovercraft/
│   ├── ThrusterGlow.shader            # Additive glow for thruster particles
│   ├── ShieldBubble.shader            # Holographic shield effect
│   ├── HovercraftBody.shader          # Vehicle body with customizable colors
│   └── DamageOverlay.shader          # Screen damage flash/tint
├── UI/
│   ├── MetalPanel.shader              # Textured metal UI background
│   ├── GlowBorder.shader             # Animated glow border for selected items
│   └── ScreenBlur.shader             # Background blur for pause menu
└── PostProcessing/
    └── MetalPodPostProcess.shader     # Combined post-processing (optional URP override)
```

---

## Shader Specifications

### IMPORTANT: URP Shader Structure

All shaders must use URP shader format. Base template:
```hlsl
Shader "MetalPod/Category/ShaderName"
{
    Properties
    {
        // Shader properties here
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Shader code here

            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
```

---

### 1. LavaSurface.shader

Animated flowing lava with bright emissive glow.

```
Properties:
  _MainTex ("Lava Texture", 2D)
  _NoiseTex ("Flow Noise", 2D)
  _FlowSpeed ("Flow Speed", Range(0.1, 2.0)) = 0.5
  _FlowDirection ("Flow Direction", Vector) = (1, 0, 0, 0)
  _EmissiveColor ("Emissive Color", Color) = (1, 0.3, 0, 1)  // bright orange
  _EmissiveIntensity ("Emissive Intensity", Range(1, 10)) = 5
  _CrustColor ("Crust Color", Color) = (0.1, 0.05, 0.02, 1) // dark rock
  _CrustThreshold ("Crust Threshold", Range(0, 1)) = 0.4

Behavior:
  - Sample noise texture with UV offset scrolling over time (_Time.y * _FlowSpeed)
  - Use noise value to blend between hot lava (emissive) and cooled crust
  - Above threshold = bright emissive lava, below = dark crust rock
  - Animate the blend threshold slightly over time for "breathing" effect
  - Output emissive color to glow in post-processing bloom
  - MOBILE: Use single texture lookup + simple math, no branching
```

### 2. HeatDistortion.shader (Full-Screen Render Feature)

Heat shimmer effect for lava HeatZone areas. This is a **URP Renderer Feature** or a **post-processing shader** applied via a full-screen pass.

```
Properties:
  _DistortionStrength ("Distortion", Range(0, 0.1)) = 0.02
  _DistortionSpeed ("Speed", Range(0.1, 5.0)) = 1.0
  _NoiseTex ("Noise", 2D)
  _Intensity ("Intensity", Range(0, 1)) = 0  // controlled by script

Behavior:
  - Sample screen texture (_CameraOpaqueTexture in URP)
  - Offset UV by noise texture scrolling vertically
  - Intensity controlled by HeatZone script (0 outside, ramps up inside)
  - Apply slight orange tint to sampled color when intensity > 0
  - MOBILE: Very simple — just UV offset + tint, no multi-pass
```

Implementation as URP ScriptableRendererFeature:
```csharp
// Also create: Assets/Scripts/Rendering/HeatDistortionFeature.cs
// This is a ScriptableRendererFeature that adds a full-screen pass
// It reads a global float "_HeatDistortionIntensity" set by HeatZone script
```

### 3. VolcanicRock.shader

Dark rock with glowing magma veins using emissive mask.

```
Properties:
  _MainTex ("Rock Texture", 2D)
  _EmissiveMask ("Vein Mask", 2D)       // white = vein, black = rock
  _RockColor ("Rock Color", Color) = (0.05, 0.03, 0.02, 1)
  _VeinColor ("Vein Color", Color) = (1, 0.3, 0, 1)
  _VeinIntensity ("Vein Intensity", Range(0, 8)) = 4
  _PulseSpeed ("Pulse Speed", Range(0, 3)) = 1
  _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.3

Behavior:
  - Base rock color from _MainTex
  - Veins glow based on _EmissiveMask * _VeinColor * _VeinIntensity
  - Veins pulse slightly with sin(_Time.y * _PulseSpeed) * _PulseAmount
  - Normal mapped for rocky surface detail (optional _BumpMap)
```

### 4. IceSurface.shader

Glossy, slightly transparent ice with subtle subsurface scatter.

```
Properties:
  _MainTex ("Ice Texture", 2D)
  _BumpMap ("Normal Map", 2D)
  _IceColor ("Ice Color", Color) = (0.7, 0.85, 1, 0.9)
  _Smoothness ("Smoothness", Range(0, 1)) = 0.95
  _FresnelPower ("Fresnel Power", Range(1, 5)) = 3
  _SubsurfaceColor ("Subsurface Color", Color) = (0.3, 0.5, 0.8, 1)
  _SubsurfaceIntensity ("Subsurface", Range(0, 1)) = 0.3
  _ReflectionStrength ("Reflection", Range(0, 1)) = 0.5

Behavior:
  - Very high smoothness for mirror-like reflections
  - Fresnel rim for angled light (bright at edges, see-through at center)
  - Subtle subsurface scatter (light passing through thin ice)
  - Semi-transparent edges
  - Normal map for cracked ice detail
  - MOBILE: Use simplified fresnel (1-NdotV)^power, avoid complex reflections
```

### 5. FrostOverlay.shader (Screen Effect)

Frost vignette on screen edges when in ice environment / BlizzardZone.

```
Properties:
  _FrostTex ("Frost Texture", 2D)        // ice crystal pattern
  _FrostIntensity ("Intensity", Range(0, 1)) = 0
  _FrostColor ("Frost Color", Color) = (0.8, 0.9, 1, 1)
  _VignetteSize ("Vignette", Range(0, 1)) = 0.3

Behavior:
  - Screen-space overlay
  - Frost texture appears from edges inward based on _FrostIntensity
  - At 0: no frost. At 0.3: light frost at corners. At 1.0: nearly full screen
  - Use vignette mask (distance from screen center) * frost texture
  - Slight blue-white tint
  - Controlled by BlizzardZone.cs script
```

### 6. IceCrystal.shader

Transparent refractive crystals for ice environment decoration.

```
Properties:
  _Color ("Color", Color) = (0.6, 0.8, 1, 0.5)
  _Refraction ("Refraction", Range(0, 0.1)) = 0.02
  _Smoothness ("Smoothness", Range(0, 1)) = 0.98

Behavior:
  - Transparent with refraction (offset background sampling)
  - Highly specular for sparkle
  - Tinted blue-white
  - MOBILE: Simple grab-pass refraction or just transparency + fresnel
```

### 7. ToxicSludge.shader

Animated toxic liquid with bubbling and neon glow.

```
Properties:
  _MainTex ("Sludge Texture", 2D)
  _NoiseTex ("Bubble Noise", 2D)
  _SludgeColor ("Color", Color) = (0.2, 0.8, 0.1, 1)   // neon green
  _EmissiveIntensity ("Glow", Range(0, 5)) = 3
  _BubbleSpeed ("Bubble Speed", Range(0.1, 3)) = 1
  _BubbleScale ("Bubble Scale", Range(0.5, 5)) = 2
  _FlowSpeed ("Flow Speed", Range(0.01, 0.5)) = 0.1

Behavior:
  - Similar to lava but green/yellow palette
  - Noise-based bubbling animation (displacement + color variation)
  - Strong emissive glow for neon toxic look
  - UV scrolling for slow flow
  - Surface "pops" from bubble noise threshold animation
```

### 8. ToxicScreenEffect.shader (Screen Effect)

Green tint + chromatic aberration for toxic zones.

```
Properties:
  _Intensity ("Intensity", Range(0, 1)) = 0
  _TintColor ("Tint", Color) = (0.3, 1, 0.2, 0.15)
  _AberrationStrength ("Aberration", Range(0, 0.02)) = 0.005

Behavior:
  - Green color tint overlay (additive blend)
  - Chromatic aberration: offset R and B channels slightly
  - Intensity ramped by ToxicGas.cs when player is in gas
  - At full intensity: strong green tint + noticeable color split
  - MOBILE: Very cheap — just tint + 2 extra texture samples
```

### 9. RustedMetal.shader

Metallic surface with rust detail mapping for toxic environment structures.

```
Properties:
  _MainTex ("Metal Texture", 2D)
  _RustMask ("Rust Mask", 2D)
  _MetalColor ("Metal Color", Color) = (0.5, 0.5, 0.5, 1)
  _RustColor ("Rust Color", Color) = (0.6, 0.3, 0.1, 1)
  _Metallic ("Metallic", Range(0, 1)) = 0.8
  _RustMetallic ("Rust Metallic", Range(0, 1)) = 0.1
  _Smoothness ("Smoothness", Range(0, 1)) = 0.4
  _RustSmoothness ("Rust Smoothness", Range(0, 1)) = 0.1
  _BumpMap ("Normal", 2D)

Behavior:
  - Blend between clean metal and rusted areas based on mask
  - Rust areas: rougher, less metallic, orange-brown color
  - Clean areas: smooth, metallic, reflective
  - Normal map for surface detail
```

### 10. ElectricArc.shader

Animated electricity effect for electric fences.

```
Properties:
  _Color ("Arc Color", Color) = (0.5, 0.7, 1, 1)     // electric blue-white
  _CoreColor ("Core Color", Color) = (1, 1, 1, 1)     // bright white core
  _NoiseTex ("Noise", 2D)
  _ArcSpeed ("Speed", Range(1, 20)) = 10
  _ArcThickness ("Thickness", Range(0.01, 0.2)) = 0.05
  _Intensity ("Intensity", Range(0, 5)) = 3
  _Active ("Active", Range(0, 1)) = 1

Behavior:
  - Noise-based arc path that rapidly changes
  - Bright white core with blue-white falloff
  - Flicker by randomly varying intensity
  - _Active controls on/off (0 = invisible, 1 = full intensity)
  - Additive blending for glow effect
  - Billboard or mesh-based (works on a quad between two posts)
  - MOBILE: Single noise sample + threshold for arc shape
```

### 11. ThrusterGlow.shader

Additive particle shader for hovercraft thrusters.

```
Properties:
  _MainTex ("Particle Texture", 2D)
  _Color ("Tint Color", Color) = (0, 0.87, 1, 1)    // cyan for main thrusters
  _Intensity ("Intensity", Range(0, 5)) = 2

Behavior:
  - Additive blend (Blend One One)
  - Soft particle (fade near opaque geometry to avoid hard clipping)
  - Color * Intensity for brightness
  - Supports vertex color from particle system
  - MOBILE: Simplest possible — additive texture * color * vertex color
```

### 12. ShieldBubble.shader

Holographic shield effect visible during invincibility.

```
Properties:
  _Color ("Shield Color", Color) = (0, 0.5, 1, 0.3)
  _FresnelPower ("Fresnel", Range(1, 5)) = 2
  _PulseSpeed ("Pulse Speed", Range(0, 5)) = 2
  _HexPattern ("Hex Pattern", 2D)
  _PatternScale ("Pattern Scale", Range(1, 20)) = 10
  _HitPoint ("Hit Point", Vector) = (0, 0, 0, 0)     // world pos of last hit
  _HitIntensity ("Hit Intensity", Range(0, 1)) = 0    // fades out over time

Behavior:
  - Transparent sphere around hovercraft
  - Visible mainly at edges (fresnel)
  - Hexagonal pattern texture tiling
  - Subtle pulse animation
  - On hit: bright flash ripple from hit point outward
  - Additive blend for holographic look
```

### 13. HovercraftBody.shader

Vehicle body shader with customizable dual-color support.

```
Properties:
  _MainTex ("Base Texture", 2D)
  _BumpMap ("Normal Map", 2D)
  _MaskTex ("Color Mask", 2D)           // R=primary, G=secondary, B=accent
  _PrimaryColor ("Primary", Color) = (0.83, 0.63, 0.09, 1)  // yellow
  _SecondaryColor ("Secondary", Color) = (0.17, 0.17, 0.17, 1) // dark gray
  _AccentColor ("Accent", Color) = (1, 0.53, 0, 1)     // orange
  _Metallic ("Metallic", Range(0, 1)) = 0.7
  _Smoothness ("Smoothness", Range(0, 1)) = 0.5
  _DamageAmount ("Damage", Range(0, 1)) = 0             // darkens as damaged

Behavior:
  - Use mask texture to define which areas get which color
  - Red channel = primary color area, Green = secondary, Blue = accent
  - Colors controlled by HovercraftCustomization.cs via MaterialPropertyBlock
  - _DamageAmount darkens + desaturates the colors (visual degradation)
  - Standard PBR metallic workflow
  - Normal mapped for panel details, scratches, rivets
```

### 14. DamageOverlay.shader (Screen Effect)

Full-screen damage flash and low-health vignette.

```
Properties:
  _FlashColor ("Flash Color", Color) = (1, 0, 0, 0.3)
  _FlashIntensity ("Flash", Range(0, 1)) = 0           // brief pulse on hit
  _VignetteIntensity ("Vignette", Range(0, 1)) = 0     // persistent at low HP
  _VignetteColor ("Vignette Color", Color) = (0.5, 0, 0, 1)

Behavior:
  - Flash: full screen color overlay, triggered on damage, fades quickly
  - Vignette: red edges that intensify as health drops
  - Both controlled by HovercraftVisuals.cs
  - MOBILE: Single pass, very cheap
```

### 15. MetalPanel.shader (UI)

Metal texture background for UI panels.

```
Properties:
  _MainTex ("Metal Texture", 2D)
  _Color ("Tint", Color) = (0.18, 0.18, 0.18, 1)
  _RivetTex ("Rivet Overlay", 2D)
  _EdgeGlow ("Edge Glow", Range(0, 1)) = 0
  _EdgeColor ("Edge Color", Color) = (1, 0.53, 0, 0.5) // orange glow

Behavior:
  - UI shader (rendered in screen space)
  - Base metal texture with tint
  - Rivet overlay at borders
  - Optional edge glow for selected/active state
  - Must work with Unity UI (Canvas renderer)
  - Tags: "Queue"="Transparent" "RenderType"="Transparent"
```

### 16. GlowBorder.shader (UI)

Animated glow border for selected UI elements.

```
Properties:
  _Color ("Glow Color", Color) = (1, 0.53, 0, 1)
  _GlowWidth ("Width", Range(0.01, 0.1)) = 0.02
  _PulseSpeed ("Pulse", Range(0, 5)) = 2
  _Intensity ("Intensity", Range(0, 3)) = 1.5

Behavior:
  - Renders only at edges of the UI element
  - Pulsing glow animation (sin wave on intensity)
  - Used for active tab, selected upgrade, equipped cosmetic
```

### 17. ScreenBlur.shader (UI)

Background blur for pause menu overlay.

```
Properties:
  _BlurAmount ("Blur", Range(0, 10)) = 3
  _TintColor ("Tint", Color) = (0, 0, 0, 0.5)

Behavior:
  - Captures screen and applies gaussian blur
  - Dark tint overlay
  - MOBILE: Use bilinear downsampling (sample at half-res, then quarter-res) — very cheap
  - Alternative: if too expensive, just use solid dark overlay with alpha
```

---

## Also Create: Renderer Feature Scripts

For screen effects that need URP integration:

```
Assets/Scripts/Rendering/
├── HeatDistortionFeature.cs           # ScriptableRendererFeature for heat shimmer
├── FrostOverlayFeature.cs             # ScriptableRendererFeature for frost vignette
├── ToxicScreenFeature.cs              # ScriptableRendererFeature for toxic tint
├── DamageOverlayFeature.cs            # ScriptableRendererFeature for damage flash
└── ScreenEffectController.cs          # MonoBehaviour that controls all screen effects
```

**ScreenEffectController.cs** — Central controller:
```csharp
public class ScreenEffectController : MonoBehaviour
{
    public static ScreenEffectController Instance;

    // Called by HeatZone, BlizzardZone, ToxicGas, HovercraftVisuals
    public void SetHeatDistortion(float intensity) { Shader.SetGlobalFloat("_HeatDistortionIntensity", intensity); }
    public void SetFrostOverlay(float intensity) { Shader.SetGlobalFloat("_FrostIntensity", intensity); }
    public void SetToxicEffect(float intensity) { Shader.SetGlobalFloat("_ToxicIntensity", intensity); }
    public void TriggerDamageFlash() { /* start coroutine to pulse _DamageFlashIntensity */ }
    public void SetLowHealthVignette(float healthNormalized) { /* map health to vignette intensity */ }
}
```

---

## Performance Budget (iOS)

- Max 2 texture samples per surface shader
- No real-time reflections (use fresnel approximation)
- Screen effects: max 1 extra full-screen pass total (combine if possible)
- Additive particle shaders: single texture + color multiply
- No tessellation, no geometry shaders
- Target: zero impact on 60 FPS budget on iPhone 12

---

## Acceptance Criteria

- [ ] All 17 shader files created and syntactically correct HLSL/ShaderLab
- [ ] All shaders use URP format (`"RenderPipeline" = "UniversalPipeline"`)
- [ ] LavaSurface: flowing animation with emissive glow
- [ ] HeatDistortion: screen space UV distortion
- [ ] IceSurface: glossy reflective with fresnel
- [ ] FrostOverlay: screen frost from edges
- [ ] ToxicSludge: animated toxic liquid with glow
- [ ] ToxicScreenEffect: green tint + chromatic aberration
- [ ] ElectricArc: animated noise-based electricity
- [ ] ThrusterGlow: additive particle shader with soft particles
- [ ] ShieldBubble: holographic fresnel with hex pattern
- [ ] HovercraftBody: dual-color customization via mask texture
- [ ] DamageOverlay: flash + vignette screen effect
- [ ] MetalPanel: UI metal texture with edge glow
- [ ] All 5 ScriptableRendererFeature scripts created
- [ ] ScreenEffectController MonoBehaviour for global shader control
- [ ] All shaders mobile-optimized (max 2 tex samples, no branching, no geometry shaders)
