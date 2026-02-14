# Codex Task 6: Particle System Generator

> **Goal**: Write an Editor script that programmatically creates and configures ALL particle systems in the game — hovercraft thrusters, damage effects, explosions, and environment-specific particles. When run in Unity, every VFX is ready to go.

---

## Context

The hovercraft prefab has empty ParticleSystem placeholder GameObjects (Thruster_Main_L, Thruster_Main_R, Boost_Effect, Sparks, Smoke, etc.) but they have no configured settings. Hazards also need particle effects. This script fills everything with production-quality particle settings.

**Read these files for VFX specs**:
- `AGENT_2_HOVERCRAFT_VEHICLE.md` — Thruster specs (blue glow, orange flames, boost, sparks, smoke)
- `AGENT_3_COURSES_AND_HAZARDS.md` — Hazard VFX (lava bubbles, eruption debris, ice crystals, toxic gas clouds, electric sparks, acid splashes, explosion)
- `Assets/Scripts/Hovercraft/HovercraftVisuals.cs` — References to particle systems
- `Assets/Shaders/Hovercraft/ThrusterGlow.shader` — Additive particle shader
- `Assets/Shaders/` — Other shaders particles should use

---

## Files to Create

```
Assets/Scripts/Editor/
├── ParticleSystemGenerator.cs          # Main generator with menu items
└── ParticlePresets.cs                  # Reusable particle configuration presets
```

Also create prefab-ready particle configurations for:
```
Assets/Prefabs/Effects/
├── Hovercraft/
│   ├── (configured by setup wizard on hovercraft prefab)
├── Hazards/
│   ├── Lava/
│   │   ├── LavaBubbles.prefab         # Ambient lava surface bubbles
│   │   ├── LavaSplash.prefab          # On contact with lava
│   │   ├── EruptionDebris.prefab      # Volcanic eruption projectiles
│   │   ├── GeyserSpray.prefab         # Geyser eruption column
│   │   ├── HeatShimmer.prefab         # Subtle heat haze particles
│   │   ├── EmberFloat.prefab          # Ambient floating embers
│   │   └── AshFall.prefab             # Ambient ash falling
│   ├── Ice/
│   │   ├── SnowFall.prefab            # Ambient snow particles
│   │   ├── BlizzardHeavy.prefab       # Dense blizzard particles
│   │   ├── IceShatter.prefab          # Icicle/wall breaking
│   │   ├── IceDust.prefab             # Subtle ice crystal dust
│   │   ├── FrostBreath.prefab         # Cold breath/mist effect
│   │   └── AvalancheCloud.prefab      # Massive snow/ice wall
│   └── Toxic/
│       ├── ToxicCloud.prefab           # Green gas cloud
│       ├── AcidBubble.prefab           # Acid pool bubbling
│       ├── AcidSplash.prefab           # On contact with acid
│       ├── ElectricSpark.prefab        # Electric fence sparks
│       ├── SteamVent.prefab            # Industrial steam
│       ├── ExplosionFire.prefab        # Barrel explosion
│       └── ExplosionSmoke.prefab       # Post-explosion smoke
├── UI/
│   ├── MedalBurst.prefab              # Celebration particles on medal earn
│   └── CurrencyPickup.prefab          # Currency sparkle on collect
└── Common/
    ├── CollectibleGlow.prefab          # Collectible ambient glow
    ├── CollectibleBurst.prefab         # On collection
    ├── CheckpointActivate.prefab       # Checkpoint activation flash
    └── RespawnEffect.prefab            # Player respawn shimmer
```

---

## ParticleSystemGenerator.cs

```csharp
using UnityEditor;
using UnityEngine;

public class ParticleSystemGenerator
{
    [MenuItem("Metal Pod/Effects/Generate All Particle Prefabs")]
    public static void GenerateAll()
    {
        EditorUtility.DisplayProgressBar("Generating Particles", "Creating effects...", 0);
        try
        {
            GenerateHovercraftParticles();     // Configure existing prefab particles
            GenerateLavaEffects();
            GenerateIceEffects();
            GenerateToxicEffects();
            GenerateCommonEffects();
            GenerateUIEffects();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    [MenuItem("Metal Pod/Effects/Configure Hovercraft Particles")]
    public static void GenerateHovercraftParticles() { /* ... */ }

    [MenuItem("Metal Pod/Effects/Generate Lava Effects")]
    public static void GenerateLavaEffects() { /* ... */ }

    [MenuItem("Metal Pod/Effects/Generate Ice Effects")]
    public static void GenerateIceEffects() { /* ... */ }

    [MenuItem("Metal Pod/Effects/Generate Toxic Effects")]
    public static void GenerateToxicEffects() { /* ... */ }
}
```

---

## Particle Specifications

### HOVERCRAFT PARTICLES

**Main Thrusters (×2, blue glow underneath)**
```
Module: Main
  Duration: Infinity (looping)
  Start Lifetime: 0.2 - 0.4
  Start Speed: 2 - 5
  Start Size: 0.15 - 0.35
  Start Color: Gradient from Cyan (#00DDFF) to Blue (#0066FF)
  Gravity Modifier: -0.1 (slight upward drift)
  Simulation Space: World
  Max Particles: 100

Module: Emission
  Rate over Time: 80 (scales with speed via script)

Module: Shape
  Shape: Cone
  Angle: 12
  Radius: 0.15
  Emit from: Base
  Rotation: pointing DOWN (local -Y)

Module: Color over Lifetime
  Gradient: Cyan (alpha 1.0) → Blue (alpha 0.0)

Module: Size over Lifetime
  Curve: 1.0 → 0.3 (shrinks)

Module: Renderer
  Render Mode: Billboard
  Material: ThrusterGlow shader material (additive)
  Sort Mode: By Distance
```

**Side Thrusters (×2, orange flames from sides)**
```
Module: Main
  Duration: Infinity (looping)
  Start Lifetime: 0.15 - 0.3
  Start Speed: 3 - 8
  Start Size: 0.1 - 0.25
  Start Color: Gradient Orange (#FF8800) to Red (#FF2200)
  Gravity Modifier: 0
  Max Particles: 60

Module: Emission
  Rate over Time: 40

Module: Shape
  Shape: Cone
  Angle: 8
  Radius: 0.1
  Rotation: pointing OUTWARD from side

Module: Color over Lifetime
  Orange (alpha 1.0) → Dark Red (alpha 0.0)

Module: Size over Lifetime
  1.0 → 0.2

Module: Renderer
  Material: ThrusterGlow material (additive, orange tint)
```

**Boost Effect (big flame, rear exhaust)**
```
Module: Main
  Duration: Infinity (looping, enabled/disabled by script)
  Start Lifetime: 0.3 - 0.6
  Start Speed: 8 - 15
  Start Size: 0.3 - 0.6
  Start Color: White → Orange → Red gradient
  Max Particles: 200

Module: Emission
  Rate over Time: 150

Module: Shape
  Shape: Cone
  Angle: 20
  Radius: 0.2
  Rotation: pointing BACKWARD

Module: Color over Lifetime
  White (1.0) → Orange (0.8) → Dark Red (0.0)

Module: Size over Lifetime
  1.2 → 0.1

Module: Noise
  Enabled: true
  Strength: 0.5
  Frequency: 3
  (Adds turbulence for realistic flame look)

Module: Renderer
  Material: ThrusterGlow material (additive)
```

**Damage Sparks (burst on hit)**
```
Module: Main
  Duration: 0.5
  Looping: false
  Start Lifetime: 0.3 - 0.6
  Start Speed: 5 - 15
  Start Size: 0.03 - 0.08
  Start Color: Orange (#FFAA00) / Yellow (#FFDD00) random
  Gravity Modifier: 2.0 (sparks fall quickly)
  Max Particles: 50

Module: Emission
  Bursts: [Time: 0, Count: 20-40]

Module: Shape
  Shape: Sphere
  Radius: 0.3

Module: Color over Lifetime
  Orange (1.0) → Dark (0.0)

Module: Renderer
  Render Mode: Stretched Billboard
  Speed Scale: 0.1
  Length Scale: 2
  Material: Additive particle material
```

**Smoke (when health < 50%)**
```
Module: Main
  Duration: Infinity (looping, controlled by script)
  Start Lifetime: 1.0 - 2.0
  Start Speed: 0.5 - 1.5
  Start Size: 0.2 - 0.5 (grows via Size over Lifetime)
  Start Color: Dark Gray (#333333, alpha 0.6)
  Gravity Modifier: -0.3 (rises)
  Max Particles: 30

Module: Emission
  Rate over Time: 10 (increases at lower health)

Module: Shape
  Shape: Sphere
  Radius: 0.5
  Emit from: Surface

Module: Color over Lifetime
  Dark Gray (0.6 alpha) → Transparent (0.0)

Module: Size over Lifetime
  0.5 → 2.0 (expands as it rises)

Module: Renderer
  Material: Soft particle material (alpha blended, not additive)
```

**Fire (when health < 25%)**
```
Module: Main
  Duration: Infinity
  Start Lifetime: 0.3 - 0.6
  Start Speed: 1 - 3
  Start Size: 0.15 - 0.3
  Start Color: Orange/Red
  Gravity Modifier: -1 (rises fast)
  Max Particles: 40

Module: Emission
  Rate over Time: 30

Module: Shape
  Cone pointing UP, angle 25

Module: Color over Lifetime
  Yellow → Orange → Dark Red → Transparent

Module: Size over Lifetime
  0.8 → 1.5 → 0

Module: Noise
  Strength: 1.0
  Frequency: 2
```

**Explosion (on destruction)**
```
Module: Main
  Duration: 1.5
  Looping: false
  Start Lifetime: 0.5 - 1.5
  Start Speed: 5 - 20
  Start Size: 0.5 - 2.0
  Start Color: White/Orange/Red random
  Gravity Modifier: 0.5
  Max Particles: 150

Module: Emission
  Bursts: [Time: 0, Count: 80-120]

Module: Shape
  Shape: Sphere
  Radius: 0.5

Module: Color over Lifetime
  White → Orange → Dark Gray → Transparent

Module: Size over Lifetime
  1.0 → 2.5 → 0

Sub-Emitters:
  On Birth: Spawn Sparks sub-system (20 sparks)
  On Birth: Spawn Smoke sub-system (lingering smoke)
```

**Respawn Shimmer**
```
Module: Main
  Duration: 2.0
  Looping: false
  Start Lifetime: 0.5 - 1.0
  Start Speed: 1 - 3
  Start Size: 0.1 - 0.2
  Start Color: Cyan (#00CCFF)
  Max Particles: 60

Module: Emission
  Rate over Time: 40 (first second), then 0

Module: Shape
  Shape: Sphere (radius = hovercraft size, ~2)

Module: Color over Lifetime
  Cyan (1.0) → White (0.5) → Transparent

Module: Size over Lifetime
  0.5 → 1.5 → 0
```

---

### LAVA ENVIRONMENT PARTICLES

**LavaBubbles (ambient surface bubbles)**
```
Looping, Rate: 5-10, Lifetime: 1-2, Size: 0.2-0.5
Shape: Box (flat, size of lava pool), emit from volume
Color: Orange → Dark Red, Start Speed: 0.5-1 (upward)
Pop at end of lifetime (size curve: grow then shrink fast)
```

**LavaSplash (on hovercraft contact)**
```
Burst: 15-25 particles, Lifetime: 0.3-0.6
Shape: Hemisphere (upward), Speed: 5-10
Color: Bright Orange → Red, Gravity: 3
Stretched billboards for liquid look
```

**EruptionDebris (volcanic eruption projectiles)**
```
Burst: 30-50, Lifetime: 2-4
Shape: Cone pointing UP, angle 40, Speed: 10-25
Size: 0.3-0.8, Color: Dark rock with orange glow
Gravity: 1.5 (arcing trajectories)
Trail: enabled (orange trail behind each rock)
Collision: World (bounce on ground)
```

**GeyserSpray (eruption column)**
```
Looping (when active), Rate: 200, Lifetime: 0.5-1.0
Shape: Cone UP, angle 5, Speed: 15-30
Size: 0.2-0.5, Color: Orange → Yellow → White steam
Height controlled by height parameter
Noise: strong turbulence at top
```

**EmberFloat (ambient floating embers)**
```
Looping, Rate: 3-8, Lifetime: 3-6
Shape: Large box (20x5x20), emit from volume
Size: 0.02-0.06, Color: Orange with flicker
Speed: 0.3-0.8 (gentle upward drift)
Gravity: -0.05
Noise: gentle swaying
```

**AshFall (ambient falling ash)**
```
Looping, Rate: 10-20, Lifetime: 4-8
Shape: Large box above camera (30x1x30, height 20)
Size: 0.02-0.05, Color: Gray
Speed: 0.5-1.5 (downward)
Gravity: 0.1
Noise: gentle horizontal drift
```

---

### ICE ENVIRONMENT PARTICLES

**SnowFall (ambient gentle snow)**
```
Looping, Rate: 20-40, Lifetime: 5-10
Shape: Large box above (30x1x30, height 25)
Size: 0.03-0.08, Color: White (alpha 0.7)
Speed: 0.5-1.0, Gravity: 0.2
Noise: gentle horizontal drift (wind)
Renderer: Billboard, soft particle
```

**BlizzardHeavy (dense storm)**
```
Looping, Rate: 200-400, Lifetime: 2-4
Shape: Large box, size matches blizzard zone
Size: 0.05-0.15, Color: White (alpha 0.5)
Speed: 5-15 (mostly horizontal = wind direction)
Gravity: 0.3
Noise: strong, frequency 2
Stretched Billboard: speed scale 0.2 for wind streaks
```

**IceShatter (breaking icicle/wall)**
```
Burst: 30-60, Lifetime: 0.5-1.5
Shape: Sphere (small), Speed: 5-15
Size: 0.05-0.3, Color: Pale Blue / White
Gravity: 3 (ice chunks fall fast)
Rotation: random
Mesh particles if possible (small triangles) or stretched billboards
```

**IceDust (ambient crystal sparkle)**
```
Looping, Rate: 5-10, Lifetime: 2-4
Shape: Sphere (radius 10), Speed: 0.1-0.3
Size: 0.01-0.03, Color: White/Pale Blue with flicker
Gravity: 0, Noise: very gentle
Additive blending for sparkle
```

**AvalancheCloud (massive moving wall)**
```
Looping (when active), Rate: 500, Lifetime: 1-3
Shape: Box (wide, tall), moves with Avalanche script
Size: 1-5, Color: White → Gray
Speed: 2-8 (forward + outward spread)
Noise: intense turbulence
Gravity: 0.5
This is the visual body of the avalanche
```

---

### TOXIC ENVIRONMENT PARTICLES

**ToxicCloud (gas zones)**
```
Looping, Rate: 15-30, Lifetime: 2-4
Shape: Box (size of gas zone), emit from volume
Size: 1-3 (big billowy clouds), Color: Green (#44FF00, alpha 0.3) → Dark Green (alpha 0)
Speed: 0.2-0.5 (slow drift)
Noise: moderate, slow
Alpha-blended soft particles
```

**AcidBubble (acid pool surface)**
```
Looping, Rate: 3-8, Lifetime: 0.5-1.5
Shape: Box (flat, pool surface)
Size: 0.1-0.3, Color: Neon Green
Speed: 0.5-1 (upward pop)
Size over Lifetime: grow then shrink fast (bubble pop)
```

**AcidSplash (on contact)**
```
Burst: 10-20, Lifetime: 0.3-0.5
Shape: Hemisphere UP, Speed: 3-8
Size: 0.05-0.15, Color: Bright Green
Gravity: 4 (falls back quick)
Stretched billboard for droplet look
```

**ElectricSpark (fence active)**
```
Looping (when fence is ON), Rate: 30-50, Lifetime: 0.05-0.15
Shape: Line (between fence posts)
Size: 0.02-0.05, Color: White/Pale Blue
Speed: 10-30 (fast, erratic)
Noise: maximum intensity, high frequency (chaotic)
Additive blending
Very short lifetime = flickering sparks
```

**SteamVent (industrial atmosphere)**
```
Looping (periodic via script), Rate: 50 (during burst), Lifetime: 1-2
Shape: Cone UP, angle 15
Size: 0.3-1.0, Color: White (alpha 0.4) → Transparent
Speed: 3-8
Noise: moderate
Plays in short bursts (0.5s on, 3s off)
```

**ExplosionFire (barrel explosion)**
```
Burst: 50-80, Duration: 0.8
Lifetime: 0.3-0.8, Size: 0.5-2.0
Color: White → Yellow → Orange → Black
Speed: 5-15, Shape: Sphere
Noise: strong turbulence
Size over Lifetime: grow then shrink

Sub-emitter: ExplosionSmoke
```

**ExplosionSmoke (post-explosion)**
```
Follows ExplosionFire after 0.3s delay
Rate: 20, Lifetime: 2-4
Size: 1-4 (grows), Color: Dark Gray → Transparent
Speed: 0.5-2, Gravity: -0.2 (rises)
Lingers for several seconds after explosion
```

---

### COMMON / UI PARTICLES

**CollectibleGlow (ambient glow around collectibles)**
```
Looping, Rate: 5, Lifetime: 1-2
Shape: Sphere (radius 0.3), Speed: 0.1-0.3
Size: 0.05-0.1
Color by collectible type: Gold(currency), Green(health), Blue(shield)
Additive blending, gentle orbit
```

**CollectibleBurst (on collection)**
```
Burst: 20-30, Duration: 0.5
Lifetime: 0.3-0.6, Speed: 3-8
Shape: Sphere, Size: 0.05-0.15
Color matches collectible type
Gravity: -0.5 (floats up slightly)
```

**CheckpointActivate (flash on checkpoint)**
```
Burst: 40, Duration: 0.8
Lifetime: 0.5-1.0, Speed: 2-5
Shape: Cylinder (ring shape around checkpoint)
Color: Green → White → Transparent
Size: 0.1-0.3
Rises upward
```

**MedalBurst (results screen celebration)**
```
Burst: 60-100, Duration: 1.5
Lifetime: 1-3, Speed: 5-15
Shape: Cone UP, angle 60
Size: 0.1-0.3
Color: matches medal (Gold=#FFD700, Silver=#C0C0C0, Bronze=#CD7F32)
Gravity: 2 (confetti falls)
Rotation: random (tumbling confetti)
```

---

## Implementation Approach

For each particle effect, the generator should:

1. Create a new GameObject
2. Get or add ParticleSystem component
3. Configure each module via `ParticleSystem.MainModule`, `ParticleSystem.EmissionModule`, etc.
4. Set up materials (create simple materials using existing shaders, or use Unity's default particle materials)
5. Save as prefab via `PrefabUtility.SaveAsPrefabAsset()`

Example:
```csharp
private static GameObject CreateParticleEffect(string name, string savePath)
{
    var go = new GameObject(name);
    var ps = go.AddComponent<ParticleSystem>();

    var main = ps.main;
    main.duration = 5f;
    main.loop = true;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.0f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
    main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
    main.startColor = new Color(0, 0.87f, 1f, 1f); // cyan
    main.maxParticles = 100;
    main.simulationSpace = ParticleSystemSimulationSpace.World;

    var emission = ps.emission;
    emission.rateOverTime = 50;

    var shape = ps.shape;
    shape.shapeType = ParticleSystemShapeType.Cone;
    shape.angle = 15;
    shape.radius = 0.2f;

    var colorOverLifetime = ps.colorOverLifetime;
    colorOverLifetime.enabled = true;
    var gradient = new Gradient();
    gradient.SetKeys(
        new GradientColorKey[] {
            new GradientColorKey(Color.cyan, 0f),
            new GradientColorKey(Color.blue, 1f)
        },
        new GradientAlphaKey[] {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(0f, 1f)
        }
    );
    colorOverLifetime.color = gradient;

    // Create and assign material
    var mat = new Material(Shader.Find("MetalPod/Hovercraft/ThrusterGlow"));
    if (mat.shader == null) mat = new Material(Shader.Find("Particles/Standard Unlit"));
    mat.SetColor("_Color", Color.cyan);
    var renderer = go.GetComponent<ParticleSystemRenderer>();
    renderer.material = mat;
    renderer.renderMode = ParticleSystemRenderMode.Billboard;

    // Save material
    string matPath = savePath.Replace(".prefab", "_Mat.mat");
    EnsureDirectory(matPath);
    AssetDatabase.CreateAsset(mat, matPath);

    // Save prefab
    EnsureDirectory(savePath);
    PrefabUtility.SaveAsPrefabAsset(go, savePath);
    Object.DestroyImmediate(go);

    return AssetDatabase.LoadAssetAtPath<GameObject>(savePath);
}
```

For the **hovercraft prefab specifically**: don't create new prefabs — instead load the existing hovercraft prefab, find the placeholder particle GameObjects by name, and configure their ParticleSystems in place.

---

## Acceptance Criteria

- [ ] Menu item "Metal Pod/Effects/Generate All Particle Prefabs" works
- [ ] All hovercraft particles configured (thrusters ×2, side thrusters ×2, boost, sparks, smoke, fire, explosion, respawn)
- [ ] 7 Lava effect prefabs created
- [ ] 5 Ice effect prefabs created
- [ ] 7 Toxic effect prefabs created
- [ ] 4 Common effect prefabs (collectible glow/burst, checkpoint, respawn)
- [ ] 2 UI effect prefabs (medal burst, currency pickup)
- [ ] All materials created and assigned (using project shaders where available, fallback to built-in)
- [ ] Total: ~35 particle effect configurations
- [ ] Each prefab saved to correct path under Assets/Prefabs/Effects/
- [ ] Particle counts within mobile budget (max 500 total visible at once per GameConstants)
