# Agent 3: Courses, Environments & Hazards

> **Owner**: Agent 3
> **Priority**: HIGH — This is the game content
> **Estimated scope**: ~25% of total project (largest agent)
> **Dependency**: Agent 1 (interfaces, ScriptableObjects), Agent 2 (IDamageReceiver on hovercraft)

---

## Mission

You own all **level content, environmental hazards, and course systems**. This includes the course manager, checkpoints, collectibles, the medal system, the finish line, and ALL hazard implementations across all 3 environments (Lava, Ice, Toxic). You also define level layouts and environmental art direction. You are the content-heaviest agent.

---

## Files You OWN (Create / Modify)

```
Assets/
├── Scripts/
│   ├── Course/
│   │   ├── CourseManager.cs          ✅ EXISTS - review & enhance
│   │   ├── Checkpoint.cs             ✅ EXISTS - review & polish
│   │   ├── CourseTimer.cs            ✅ EXISTS - good
│   │   ├── MedalSystem.cs            ✅ EXISTS - good
│   │   ├── Collectible.cs            ✅ EXISTS - enhance (add magnet pull)
│   │   └── FinishLine.cs             ✅ EXISTS - good
│   └── Hazards/
│       ├── HazardBase.cs             ✅ EXISTS - enhance (add DamageType)
│       ├── DamageZone.cs             ✅ EXISTS - enhance (generic zone)
│       ├── HazardWarning.cs          ✅ EXISTS - good
│       │
│       ├── Lava/                     🆕 CREATE subfolder
│       │   ├── LavaFlow.cs           ✅ EXISTS - move here, review
│       │   ├── VolcanicEruption.cs   🆕 CREATE
│       │   ├── LavaGeyser.cs         🆕 CREATE
│       │   └── HeatZone.cs           🆕 CREATE
│       │
│       ├── Ice/                      🆕 CREATE subfolder
│       │   ├── IcePatch.cs           ✅ EXISTS - move here, review
│       │   ├── FallingIcicle.cs      🆕 CREATE
│       │   ├── BlizzardZone.cs       🆕 CREATE
│       │   ├── IceWall.cs            🆕 CREATE
│       │   └── Avalanche.cs          🆕 CREATE
│       │
│       ├── Toxic/                    🆕 CREATE subfolder
│       │   ├── ToxicGas.cs           ✅ EXISTS (stub) - rewrite
│       │   ├── AcidPool.cs           🆕 CREATE
│       │   ├── IndustrialPress.cs    🆕 CREATE
│       │   ├── ElectricFence.cs      🆕 CREATE
│       │   └── BarrelExplosion.cs    🆕 CREATE
│       │
│       └── FallingDebris.cs          ✅ EXISTS - shared across envs
│
├── Prefabs/
│   ├── Course/
│   │   ├── Checkpoint.prefab
│   │   ├── FinishLine.prefab
│   │   ├── StartLine.prefab
│   │   └── Collectible.prefab
│   ├── Hazards/
│   │   ├── Lava/
│   │   ├── Ice/
│   │   └── Toxic/
│   └── Environment/
│       ├── Lava/
│       ├── Ice/
│       └── Toxic/
│
├── Scenes/
│   ├── TestCourse.unity              🆕 GREYBOX (Phase 1)
│   ├── Lava_Course_01.unity          🆕
│   ├── Lava_Course_02.unity          🆕
│   ├── Lava_Course_03.unity          🆕
│   ├── Ice_Course_01.unity           🆕
│   ├── Ice_Course_02.unity           🆕
│   ├── Ice_Course_03.unity           🆕
│   ├── Toxic_Course_01.unity         🆕
│   ├── Toxic_Course_02.unity         🆕
│   └── Toxic_Course_03.unity         🆕
│
└── Materials/
    ├── Environments/
    │   ├── Lava/
    │   ├── Ice/
    │   └── Toxic/
    └── Hazards/
```

## Files You MUST NOT Touch

- `Assets/Scripts/Core/*` (Agent 1)
- `Assets/Scripts/Shared/*` (Agent 1)
- `Assets/Scripts/Hovercraft/*` (Agent 2)
- `Assets/Scripts/Progression/*` (Agent 4)
- `Assets/Scripts/Workshop/*` (Agent 5)
- `Assets/Scripts/UI/*` (Agent 5)

## Files You REFERENCE (Read-Only)

```
Assets/Scripts/Shared/IDamageReceiver.cs   — call TakeDamage() on hovercraft
Assets/Scripts/Shared/ICourseEvents.cs     — implement this on CourseManager
Assets/Scripts/Shared/GameConstants.cs     — tags, layers
Assets/Scripts/Shared/EventBus.cs          — raise OnCourseCompleted
Assets/ScriptableObjects/CourseDataSO.cs   — data for each course
Assets/ScriptableObjects/HazardDataSO.cs   — data for each hazard type
```

---

## What Already Exists

### CourseManager.cs (EXISTS — enhance)
- States: Ready, Countdown, Racing, Respawning, Finished, Failed
- Countdown with tick events, checkpoint tracking, respawn logic
- **ENHANCE**: Implement `ICourseEvents` interface, integrate with EventBus (raise OnCourseCompleted), add course-specific intro camera sequence

### Checkpoint.cs (EXISTS — good)
- Trigger detection, color change, spawn point tracking
- **ENHANCE**: Add sequential validation (can't skip checkpoints), add checkpoint number display

### CourseTimer.cs (EXISTS — good)
- Elapsed time, pause/resume, formatted display
- **STATUS**: Complete.

### MedalSystem.cs (EXISTS — good)
- Static EvaluatePerformance method
- **STATUS**: Complete.

### Collectible.cs (EXISTS — enhance)
- Types: Currency, Health, Shield. Trigger collection, spin animation
- **ENHANCE**: Add magnetic pull when hovercraft is within range, add glow particle effect, add collection burst VFX

### FinishLine.cs (EXISTS — good)
- Trigger detection, OnPlayerFinished event
- **STATUS**: Complete.

### HazardBase.cs (EXISTS — enhance)
- Abstract base with DamagePerSecond, DamagePerHit, trigger handling
- **ENHANCE**: Add DamageType field, use `IDamageReceiver.TakeDamage(amount, type)` instead of direct health access

### LavaFlow.cs (EXISTS — good)
- Intermittent flow, UV scrolling
- **ENHANCE**: Move to Lava subfolder, add DamageType.Fire

### FallingDebris.cs (EXISTS — good)
- Physics projectile, collision damage, lifetime
- **STATUS**: Good. Keep as shared hazard (used in lava and ice)

### IcePatch.cs (EXISTS — good)
- Drift multiplier on enter/exit
- **ENHANCE**: Move to Ice subfolder

### ToxicZone.cs (EXISTS — stub)
- Inherits HazardBase, no additional logic
- **REWRITE**: As ToxicGas.cs in Toxic subfolder with visibility reduction + particles

### HazardWarning.cs (EXISTS — good)
- Detection radius, closest hazard tracking, threat levels
- **STATUS**: Complete.

---

## Task List

### Task 1: Enhance HazardBase with DamageType

```csharp
public abstract class HazardBase : MonoBehaviour
{
    [SerializeField] protected HazardDataSO _hazardData;
    [SerializeField] protected float _damagePerSecond = 10f;
    [SerializeField] protected float _damagePerHit = 25f;
    [SerializeField] protected DamageType _damageType = DamageType.Physical;
    [SerializeField] protected bool _isActive = true;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!_isActive) return;
        if (other.CompareTag(GameConstants.TAG_PLAYER))
        {
            var receiver = other.GetComponentInParent<IDamageReceiver>();
            receiver?.TakeDamage(_damagePerHit, _damageType);
        }
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (!_isActive) return;
        if (other.CompareTag(GameConstants.TAG_PLAYER))
        {
            var receiver = other.GetComponentInParent<IDamageReceiver>();
            receiver?.TakeDamage(_damagePerSecond * Time.deltaTime, _damageType);
        }
    }
}
```

### Task 2: CourseManager — Implement ICourseEvents

Ensure CourseManager implements `ICourseEvents` and raises `EventBus.RaiseCourseCompleted()` on finish.

### Task 3: Collectible Enhancement

Add magnet pull:
```csharp
private void Update()
{
    if (_collected) return;

    // Find player within magnet range
    var player = FindPlayerInRange(_magnetRange); // 5 units
    if (player != null)
    {
        // Lerp toward player
        transform.position = Vector3.MoveTowards(
            transform.position, player.position, _magnetSpeed * Time.deltaTime);
    }

    // Spin animation
    transform.Rotate(Vector3.up, _spinSpeed * Time.deltaTime);
}
```

### Task 4: LAVA HAZARDS

**VolcanicEruption.cs**
```csharp
// Periodic eruptions that launch debris projectiles in an area
public class VolcanicEruption : HazardBase
{
    [Header("Eruption Settings")]
    [SerializeField] private float _erupsionInterval = 8f;    // seconds between eruptions
    [SerializeField] private float _warningDuration = 2f;     // warning before eruption
    [SerializeField] private int _debrisCount = 5;            // projectiles per eruption
    [SerializeField] private float _debrisSpreadRadius = 10f;
    [SerializeField] private GameObject _debrisPrefab;        // FallingDebris prefab

    [Header("Warning")]
    [SerializeField] private ParticleSystem _rumbleParticles;
    [SerializeField] private AudioClip _warningRumble;
    [SerializeField] private AudioClip _eruptionSound;

    // Cycle: Idle → Warning (shake + rumble) → Eruption (spawn debris) → Cooldown → Idle
    // Debris spawns at height with random XZ offset within spread radius
    // Each debris has a shadow indicator on the ground below it
    // DamageType: Fire
}
```

**LavaGeyser.cs**
```csharp
// Ground geysers that erupt periodically with upward force
public class LavaGeyser : HazardBase
{
    [Header("Geyser Settings")]
    [SerializeField] private float _cycleTime = 5f;
    [SerializeField] private float _activeDuration = 1.5f;
    [SerializeField] private float _upwardForce = 30f;        // launches hovercraft
    [SerializeField] private float _geyserHeight = 8f;

    [Header("Warning")]
    [SerializeField] private ParticleSystem _bubblingEffect;  // pre-eruption bubbles
    [SerializeField] private float _bubbleDuration = 1.5f;

    [Header("Active")]
    [SerializeField] private ParticleSystem _geyserEffect;    // full eruption column
    [SerializeField] private AudioClip _eruptSound;

    // Cycle: Dormant → Bubbling (warning) → Active (damage + upward force) → Dormant
    // When active: apply upward force to hovercraft Rigidbody + fire damage
    // Visual: column of lava/steam particles
    // DamageType: Fire
}
```

**HeatZone.cs**
```csharp
// Invisible zone of extreme heat - damage over time + visual distortion
public class HeatZone : HazardBase
{
    [Header("Heat Settings")]
    [SerializeField] private float _heatIntensity = 1f;       // multiplier for DOT

    [Header("Visuals")]
    [SerializeField] private ParticleSystem _heatHaze;        // shimmer particles
    // Will need a heat distortion shader effect on camera (post-processing)
    // Screen edges glow orange while in zone

    // DamageType: Fire
    // Low but constant damage — encourages speed through the zone
    // Visual: heat shimmer distortion shader on screen
}
```

### Task 5: ICE HAZARDS

**FallingIcicle.cs**
```csharp
// Stalactites that crack and fall when player approaches
public class FallingIcicle : HazardBase
{
    [Header("Icicle Settings")]
    [SerializeField] private float _triggerRadius = 8f;       // detection range
    [SerializeField] private float _crackDuration = 1f;       // time from crack to fall
    [SerializeField] private bool _respawns = true;
    [SerializeField] private float _respawnDelay = 10f;

    [Header("Visuals")]
    [SerializeField] private Renderer _icicleRenderer;
    [SerializeField] private ParticleSystem _crackParticles;  // ice dust
    [SerializeField] private ParticleSystem _shatterEffect;   // on impact
    [SerializeField] private AudioClip _crackSound;
    [SerializeField] private AudioClip _shatterSound;

    // States: Intact → Cracking (visual + audio cue) → Falling (physics) → Shattered → Respawn
    // Shadow indicator on ground
    // DamageType: Physical (ice + physical impact)
}
```

**BlizzardZone.cs**
```csharp
// Area with heavy snow reducing visibility + wind force
public class BlizzardZone : HazardBase
{
    [Header("Blizzard Settings")]
    [SerializeField] private float _windForce = 5f;
    [SerializeField] private Vector3 _windDirection = Vector3.right;
    [SerializeField] private float _visibilityRange = 15f;    // fog distance in zone

    [Header("Visuals")]
    [SerializeField] private ParticleSystem _snowParticles;
    [SerializeField] private AudioClip _windLoop;

    // OnTriggerStay: apply wind force to hovercraft Rigidbody
    // On enter: reduce camera fog distance
    // On exit: restore camera fog
    // No direct damage — danger is from reduced visibility + wind pushing into hazards
    // Wind direction can vary (set per instance)
}
```

**IceWall.cs**
```csharp
// Breakable ice barrier — boost through or find alternate route
public class IceWall : MonoBehaviour  // NOT HazardBase — it's an obstacle, not damage zone
{
    [Header("Settings")]
    [SerializeField] private float _breakSpeedThreshold = 25f; // min speed to break through
    [SerializeField] private bool _requiresBoost = true;

    [Header("Visuals")]
    [SerializeField] private GameObject _intactModel;
    [SerializeField] private GameObject _shatteredPrefab;     // fractured ice pieces
    [SerializeField] private ParticleSystem _shatterEffect;
    [SerializeField] private AudioClip _shatterSound;
    [SerializeField] private AudioClip _bounceSound;

    // On collision:
    //   If player is boosting OR speed > threshold: break wall, spawn shatter VFX
    //   Else: bounce player back, play bounce sound, minor damage
    // Rewards risk-taking with boost
    // Some ice walls hide shortcuts
}
```

**Avalanche.cs**
```csharp
// Scripted event — wall of snow/ice chasing player from behind
public class Avalanche : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _speed = 18f;              // slightly faster than base hovercraft
    [SerializeField] private float _startDelay = 3f;          // delay after trigger
    [SerializeField] private float _duration = 15f;           // how long it lasts

    [Header("Kill Zone")]
    [SerializeField] private BoxCollider _killZone;           // instant destruction if caught

    [Header("Visuals")]
    [SerializeField] private ParticleSystem _avalancheWall;
    [SerializeField] private AudioClip _rumbleLoop;
    [SerializeField] private float _cameraShakeIntensity = 0.5f;

    // Triggered by entering a trigger zone
    // Moves forward at constant speed behind the player
    // Player must maintain speed to outrun it
    // If caught: instant destruction → respawn at next checkpoint (avalanche stops)
    // Creates intense pressure sequence
}
```

### Task 6: TOXIC HAZARDS

**ToxicGas.cs** (rewrite existing ToxicZone stub)
```csharp
// Clouds of corrosive gas — DOT + visibility reduction
public class ToxicGas : HazardBase
{
    [Header("Gas Settings")]
    [SerializeField] private bool _isPeriodic = false;
    [SerializeField] private float _ventInterval = 4f;        // if periodic
    [SerializeField] private float _ventDuration = 2f;

    [Header("Visuals")]
    [SerializeField] private ParticleSystem _gasCloud;
    [SerializeField] private Color _fogColor = new Color(0.2f, 0.8f, 0.1f, 0.5f);
    [SerializeField] private AudioClip _hissSound;

    // DamageType: Toxic
    // Continuous DOT while in cloud
    // Green fog overlay on camera
    // Some are constant, others are periodic (vents)
    // Particle system: green/yellow billowing clouds
}
```

**AcidPool.cs**
```csharp
// Corrosive liquid pools — high damage, some with rising/falling levels
public class AcidPool : HazardBase
{
    [Header("Pool Settings")]
    [SerializeField] private bool _hasRisingLevel = false;
    [SerializeField] private float _minLevel = 0f;
    [SerializeField] private float _maxLevel = 2f;
    [SerializeField] private float _riseSpeed = 0.5f;

    [Header("Visuals")]
    [SerializeField] private Renderer _acidSurface;
    [SerializeField] private ParticleSystem _bubbles;
    [SerializeField] private ParticleSystem _splashEffect;    // on hovercraft contact
    [SerializeField] private AudioClip _sizzleSound;

    // DamageType: Toxic
    // High DPS — don't linger
    // Rising/falling pools: animate Y position of acid surface
    // Splash particles when hovercraft touches
    // UV scroll on acid material for animated surface
}
```

**IndustrialPress.cs**
```csharp
// Timed crushing machinery — instant kill
public class IndustrialPress : MonoBehaviour
{
    [Header("Press Settings")]
    [SerializeField] private float _openDuration = 3f;        // time press is open (safe)
    [SerializeField] private float _closeDuration = 1f;       // time press is closed
    [SerializeField] private float _closeSpeed = 10f;         // how fast it slams
    [SerializeField] private float _openSpeed = 2f;           // how fast it opens

    [Header("Components")]
    [SerializeField] private Transform _pressHead;            // moving part
    [SerializeField] private Vector3 _openPosition;
    [SerializeField] private Vector3 _closedPosition;
    [SerializeField] private BoxCollider _crushZone;

    [Header("Audio/Visual")]
    [SerializeField] private AudioClip _slamSound;
    [SerializeField] private AudioClip _hydraulicSound;
    [SerializeField] private ParticleSystem _steamEffect;

    // Cycle: Open (safe to pass) → Closing (fast slam) → Closed (blocked) → Opening (slow)
    // If hovercraft is in crushZone during close: instant destruction
    // Timing-based challenge — watch the pattern, go when open
    // Visual: steam puffs, shaking on slam, warning lights (red when closing)
}
```

**ElectricFence.cs**
```csharp
// Intermittent electric barriers — stun + damage
public class ElectricFence : HazardBase
{
    [Header("Fence Settings")]
    [SerializeField] private float _onDuration = 2f;
    [SerializeField] private float _offDuration = 3f;
    [SerializeField] private float _stunDuration = 1f;        // control loss duration

    [Header("Visuals")]
    [SerializeField] private Renderer _fenceRenderer;
    [SerializeField] private Material _activeMaterial;        // glowing electric
    [SerializeField] private Material _inactiveMaterial;      // dim/off
    [SerializeField] private ParticleSystem _sparkEffect;
    [SerializeField] private AudioClip _electricHum;
    [SerializeField] private AudioClip _zapSound;

    // DamageType: Electric
    // On/Off cycle — learn the pattern
    // When ON and hovercraft touches: damage + stun (brief control loss)
    // Stun: disable player input for stunDuration, play screen static effect
    // Visual: electricity arcing between posts when active
}
```

**BarrelExplosion.cs**
```csharp
// Explosive barrels — area damage, chain reactions
public class BarrelExplosion : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _explosionRadius = 5f;
    [SerializeField] private float _explosionDamage = 40f;
    [SerializeField] private float _explosionForce = 20f;
    [SerializeField] private float _chainReactionDelay = 0.2f;

    [Header("Trigger")]
    [SerializeField] private float _triggerSpeed = 15f;       // player speed to trigger
    [SerializeField] private bool _triggerOnAnyContact = false;

    [Header("Visuals")]
    [SerializeField] private GameObject _barrelModel;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private AudioClip _explosionSound;

    // On hovercraft collision (if fast enough or triggerOnAnyContact):
    //   Explode: area damage to player, physics force push
    //   Check for other barrels in explosionRadius → trigger chain reaction with delay
    //   Spawn explosion VFX + sound
    //   Destroy barrel (or respawn after delay)
    // Can be used strategically to clear breakable obstacles
    // DamageType: Explosive
}
```

### Task 7: GREYBOX TEST COURSE

Create a simple greybox test course to validate Phase 1 systems:

**Layout (TestCourse.unity)**:
```
Start → Straight (50m) → Wide Left Turn → Straight with Ramp →
Gap (jump over) → Narrow Corridor (10m) → S-Curve →
Straight with Walls → Finish

Total length: ~300m
Width: 15m (wide sections), 6m (narrow)
3 Checkpoints: After turn, after gap, after S-curve
5 Collectibles: 2 easy (on path), 3 risky (on edges/ramp)
```

Scene structure:
```
TestCourse (Scene)
├── Environment
│   ├── Ground (plane with grid material)
│   ├── Walls (box colliders)
│   ├── Ramps (angled boxes)
│   └── Gaps (missing ground sections)
├── Course
│   ├── StartLine (trigger + StartLine prefab)
│   ├── Checkpoint_1
│   ├── Checkpoint_2
│   ├── Checkpoint_3
│   ├── FinishLine (trigger + FinishLine prefab)
│   └── CourseManager (with CourseTimer, references to checkpoints)
├── Collectibles
│   ├── Collectible_01 through _05
├── Player
│   └── Hovercraft (prefab instance from Agent 2)
├── Camera
│   └── Main Camera (follow cam behind hovercraft)
└── Lighting
    ├── Directional Light
    └── Skybox (default)
```

### Task 8: LAVA COURSES (3 courses)

**Lava_Course_01 — "Inferno Gate" (Easy/Tutorial)**
```
Environment: Open volcanic plains, lava rivers on sides
Length: ~400m, 60s gold time
Hazards: Static lava pools (edges), slow falling debris (2 spots)
Teaching: Basic controls, avoiding edges, timing through debris
Checkpoints: 3
Collectibles: 5
Medal times: Gold 50s, Silver 65s, Bronze 80s
```

**Lava_Course_02 — "Magma Run" (Medium)**
```
Environment: Rocky canyon with lava rivers crossing the path
Length: ~600m, 70s gold time
Hazards: Timed lava flows (3), geysers (4), falling debris areas (2)
New challenge: Timed crossings, geyser avoidance
Checkpoints: 4
Collectibles: 8
Medal times: Gold 70s, Silver 90s, Bronze 110s
```

**Lava_Course_03 — "Eruption" (Hard)**
```
Environment: Active volcano, tight corridors over lava lake
Length: ~800m, 90s gold time
Hazards: ALL lava hazards, volcanic eruption sequence at midpoint,
         tight corridors with lava on both sides, heat zones
Climax: Scripted eruption — debris rain for 15 seconds, must navigate through
Checkpoints: 5
Collectibles: 10 (very risky placements)
Medal times: Gold 90s, Silver 120s, Bronze 150s
```

### Task 9: ICE COURSES (3 courses)

**Ice_Course_01 — "Frozen Lake" (Medium)**
```
Environment: Frozen lake with ice caves around edges
Hazards: Ice patches (large areas), falling icicles, gentle wind
Checkpoints: 3
Collectibles: 6
Medal times: Gold 60s, Silver 80s, Bronze 100s
```

**Ice_Course_02 — "Crystal Caverns" (Hard)**
```
Environment: Inside frozen cave system, tight tunnels
Hazards: Ice patches, icicles, ice walls (breakable shortcuts), blizzard zones
Checkpoints: 4
Collectibles: 8
Medal times: Gold 80s, Silver 105s, Bronze 130s
```

**Ice_Course_03 — "Avalanche Pass" (Extreme)**
```
Environment: Mountain pass, open but treacherous
Hazards: ALL ice hazards, avalanche sequence (30s survival), full blizzard
Climax: Avalanche chases player through final third of course
Checkpoints: 5
Collectibles: 10
Medal times: Gold 100s, Silver 130s, Bronze 160s
```

### Task 10: TOXIC COURSES (3 courses)

**Toxic_Course_01 — "Waste Disposal" (Medium)**
```
Environment: Abandoned waste processing plant
Hazards: Toxic gas vents (periodic), acid pools (static), explosive barrels
Checkpoints: 3
Collectibles: 6
Medal times: Gold 65s, Silver 85s, Bronze 105s
```

**Toxic_Course_02 — "The Foundry" (Hard)**
```
Environment: Active industrial complex
Hazards: Industrial presses (timed), electric fences (intermittent),
         rising acid pools, gas vents
Checkpoints: 4
Collectibles: 8
Medal times: Gold 85s, Silver 110s, Bronze 140s
```

**Toxic_Course_03 — "Meltdown" (Extreme)**
```
Environment: Collapsing refinery
Hazards: ALL toxic hazards, chain barrel explosions, rising acid throughout,
         sections of floor collapsing, everything going wrong
Climax: Final section has barrel chain reactions clearing/blocking paths dynamically
Checkpoints: 5
Collectibles: 10
Medal times: Gold 110s, Silver 145s, Bronze 180s
```

### Task 11: Environment Art Direction Documents

For each environment, define:

**Lava/Volcanic**:
- Ground: Dark volcanic rock (dark gray/black)
- Accent: Glowing magma veins (bright orange/red emissive)
- Sky: Smoky orange/red with ash particles
- Lighting: Warm orange point lights from lava, orange ambient
- Post-processing: Bloom on lava emissives, slight orange tint
- Particles: Ash floating, ember sparks, lava bubbles

**Ice/Arctic**:
- Ground: Ice sheets (glossy blue-white), snow-covered rock
- Accent: Crystal formations (pale blue, slightly emissive)
- Sky: Dark blue-gray, aurora borealis (if possible)
- Lighting: Cool blue-white directional, blue ambient
- Post-processing: Slight blue tint, frost vignette
- Particles: Snowflakes, ice crystals, breath mist

**Toxic/Industrial**:
- Ground: Rusted metal platforms, cracked concrete
- Accent: Toxic sludge (neon green emissive), warning signs (yellow/black)
- Sky: Polluted haze, sickly yellow-green clouds
- Lighting: Flickering fluorescent, green point lights from toxic sources
- Post-processing: Slight green tint, chromatic aberration in toxic zones
- Particles: Smoke, steam, dripping liquid, floating debris

---

## Acceptance Criteria

- [ ] HazardBase enhanced with DamageType support via IDamageReceiver
- [ ] CourseManager implements ICourseEvents, raises EventBus events
- [ ] Collectible has magnet pull and collection VFX
- [ ] All 5 Lava hazards implemented (LavaFlow, VolcanicEruption, LavaGeyser, HeatZone + FallingDebris)
- [ ] All 5 Ice hazards implemented (IcePatch, FallingIcicle, BlizzardZone, IceWall, Avalanche)
- [ ] All 5 Toxic hazards implemented (ToxicGas, AcidPool, IndustrialPress, ElectricFence, BarrelExplosion)
- [ ] Greybox test course scene documented with full hierarchy
- [ ] All 9 course layouts documented with hazard placement, medal times, collectible count
- [ ] 3 environment art direction documents complete
- [ ] All hazards use IDamageReceiver — no direct reference to HovercraftHealth

---

## Integration Contract

**What you provide to other agents:**
- Agent 5 needs: `ICourseEvents` (on CourseManager, for HUD timer + results screen)
- Agent 4 needs: `EventBus.OnCourseCompleted` (for save system to record)
- Agent 2 needs: Hazards calling `IDamageReceiver.TakeDamage()` on hovercraft

**What you consume from other agents:**
- Agent 1: `IDamageReceiver`, `ICourseEvents`, `GameConstants`, `EventBus`, `CourseDataSO`, `HazardDataSO`
- Agent 2: Hovercraft has `IDamageReceiver` component (you call TakeDamage on it), Rigidbody (for force application from geysers/wind/explosions)
