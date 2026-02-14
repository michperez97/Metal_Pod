# Agent 2: Hovercraft Vehicle System

> **Owner**: Agent 2
> **Priority**: HIGH — Core gameplay depends on this
> **Estimated scope**: ~20% of total project
> **Dependency**: Agent 1 (shared interfaces & ScriptableObjects)

---

## Mission

You own the **hovercraft vehicle** — the central gameplay element. Physics, input, controls, health, visuals, and audio for the player's vehicle are all your responsibility. The hovercraft must feel satisfying to control: responsive but with momentum, driftable, and fun. You also own the hovercraft prefab setup and all vehicle VFX.

---

## Files You OWN (Create / Modify)

```
Assets/
├── Scripts/
│   └── Hovercraft/
│       ├── HovercraftPhysics.cs      ✅ EXISTS - review & polish
│       ├── HovercraftInput.cs        ✅ EXISTS - review & polish
│       ├── HovercraftController.cs   ✅ EXISTS - review & polish
│       ├── HovercraftHealth.cs       ✅ EXISTS - enhance (implement IDamageReceiver)
│       ├── HovercraftStats.cs        ✅ EXISTS - enhance (implement IHovercraftData)
│       ├── HovercraftVisuals.cs      ✅ EXISTS - rewrite (currently a stub)
│       ├── HovercraftAudio.cs        🆕 CREATE
│       └── HovercraftCustomization.cs 🆕 CREATE
├── Prefabs/
│   └── Hovercraft/
│       ├── Hovercraft.prefab         🆕 CREATE (document hierarchy)
│       └── Effects/
│           ├── ThrusterEffect.prefab  🆕 CREATE
│           ├── BoostEffect.prefab     🆕 CREATE
│           ├── DamageSmoke.prefab     🆕 CREATE
│           └── ExplosionEffect.prefab 🆕 CREATE
└── Materials/
    └── Hovercraft/
        ├── HovercraftBody.mat         🆕 CREATE
        ├── HovercraftAccent.mat       🆕 CREATE
        └── ThrusterGlow.mat           🆕 CREATE
```

## Files You MUST NOT Touch

- `Assets/Scripts/Core/*` (Agent 1)
- `Assets/Scripts/Shared/*` (Agent 1)
- `Assets/Scripts/Course/*` (Agent 3)
- `Assets/Scripts/Hazards/*` (Agent 3)
- `Assets/Scripts/Progression/*` (Agent 4)
- `Assets/Scripts/Workshop/*` (Agent 5)
- `Assets/Scripts/UI/*` (Agent 5)
- `Assets/ScriptableObjects/*` (Agent 1)

## Files You REFERENCE (Read-Only — Defined by Agent 1)

```
Assets/Scripts/Shared/IHovercraftData.cs    — implement this interface
Assets/Scripts/Shared/IDamageReceiver.cs    — implement this interface
Assets/Scripts/Shared/GameConstants.cs      — use tags, layers, prefs keys
Assets/Scripts/Shared/EventBus.cs           — listen for upgrade/cosmetic events
Assets/ScriptableObjects/HovercraftStatsSO.cs — data source for all stats
```

---

## What Already Exists (From Previous Codex Pass)

### HovercraftPhysics.cs (EXISTS — production quality)
- 4-point raycast hover with spring-damper
- Forward thrust, turning torque, braking, drift
- Surface drift multiplier (for ice)
- Stabilization torque
- Altitude control
- **STATUS**: Complete. Review for edge cases, add [Header] attributes for inspector clarity.

### HovercraftInput.cs (EXISTS — production quality)
- Tilt from accelerometer with deadzone + sensitivity curve
- Touch zones: left=brake, right-bottom=boost, right-top=special
- Editor keyboard fallback (WASD + Space)
- Settings persistence via PlayerPrefs
- **STATUS**: Complete. Review, ensure it uses GameConstants for PlayerPrefs keys.

### HovercraftController.cs (EXISTS — production quality)
- State machine: Normal, Boosting, Braking, Damaged, Destroyed
- Boost with timer + cooldown
- Integrates Physics, Health, Input
- Speed/handling multipliers from health
- **STATUS**: Complete. Ensure it works with the interface system.

### HovercraftHealth.cs (EXISTS — needs enhancement)
- Health + Shield with shield-first absorption
- Shield regen with delay
- Performance degradation at health thresholds
- Events: OnDamage, OnShieldBreak, OnHealthChanged, OnDestroyed
- **ENHANCE**: Implement `IDamageReceiver` interface, add `DamageType` support, add invincibility frames after respawn

### HovercraftStats.cs (EXISTS — minimal wrapper)
- Just holds reference to HovercraftStatsSO
- **REWRITE**: Implement `IHovercraftData` interface, serve as the runtime stats container that applies upgrade modifiers on top of base SO values

### HovercraftVisuals.cs (EXISTS — stub)
- Only has PlayDamageFeedback (not connected)
- **REWRITE**: Full visual feedback system (see Task 4 below)

---

## Task List

### Task 1: Implement Shared Interfaces

**HovercraftHealth.cs — Implement IDamageReceiver**
```csharp
public class HovercraftHealth : MonoBehaviour, IDamageReceiver
{
    // Existing health/shield logic...

    public void TakeDamage(float amount)
    {
        TakeDamage(amount, DamageType.Physical);
    }

    public void TakeDamage(float amount, DamageType type)
    {
        // Apply damage type modifiers (e.g., shield is weaker vs Electric)
        float modifier = GetDamageTypeModifier(type);
        float finalDamage = amount * modifier;

        // Shield absorbs first, then health (existing logic)
        ApplyDamage(finalDamage);

        // Notify visuals of damage type for correct VFX
        _visuals?.PlayDamageEffect(type);
    }

    private float GetDamageTypeModifier(DamageType type)
    {
        // Can be expanded later, default 1.0 for all
        return type switch
        {
            DamageType.Electric => 1.2f,  // bypasses some shield
            DamageType.Explosive => 1.5f, // high damage
            _ => 1.0f
        };
    }
}
```

**HovercraftStats.cs — Implement IHovercraftData**
```csharp
public class HovercraftStats : MonoBehaviour, IHovercraftData
{
    [SerializeField] private HovercraftStatsSO _baseStats;
    private HovercraftHealth _health;
    private HovercraftController _controller;

    // Runtime modified values (base + upgrades)
    private float _speedMultiplier = 1f;
    private float _handlingMultiplier = 1f;
    private float _shieldMultiplier = 1f;
    private float _boostMultiplier = 1f;

    // IHovercraftData implementation
    public float CurrentHealth => _health.CurrentHealth;
    public float MaxHealth => _baseStats.maxHealth * _shieldMultiplier; // shield mult affects max HP too? Or separate. Decide.
    public float CurrentShield => _health.CurrentShield;
    public float MaxShield => _baseStats.maxShield * _shieldMultiplier;
    public float CurrentSpeed => _controller.CurrentSpeed;
    public float MaxSpeed => _baseStats.maxSpeed * _speedMultiplier;
    public float HealthNormalized => _health.HealthNormalized;
    public float ShieldNormalized => _health.ShieldNormalized;
    public float BoostCooldownNormalized => _controller.BoostCooldownNormalized;
    public bool IsBoosting => _controller.CurrentState == HovercraftState.Boosting;
    public bool IsDestroyed => _controller.CurrentState == HovercraftState.Destroyed;

    // Called by progression system (Agent 4) via EventBus
    public void ApplyUpgradeMultipliers(float speed, float handling, float shield, float boost)
    {
        _speedMultiplier = speed;
        _handlingMultiplier = handling;
        _shieldMultiplier = shield;
        _boostMultiplier = boost;
    }

    // Getters for physics system
    public float GetEffectiveMaxSpeed() => _baseStats.maxSpeed * _speedMultiplier;
    public float GetEffectiveTurnSpeed() => _baseStats.turnSpeed * _handlingMultiplier;
    public float GetEffectiveBoostMultiplier() => _baseStats.boostMultiplier * _boostMultiplier;
    public float GetEffectiveMaxShield() => _baseStats.maxShield * _shieldMultiplier;
}
```

### Task 2: Hovercraft Audio

**HovercraftAudio.cs** — All vehicle sounds:
```csharp
public class HovercraftAudio : MonoBehaviour
{
    [Header("Engine")]
    [SerializeField] private AudioSource _engineSource;
    [SerializeField] private AudioClip _engineLoop;
    [SerializeField] private float _minPitch = 0.8f;
    [SerializeField] private float _maxPitch = 1.5f;
    [SerializeField] private float _minVolume = 0.3f;
    [SerializeField] private float _maxVolume = 1.0f;

    [Header("Actions")]
    [SerializeField] private AudioClip _boostClip;
    [SerializeField] private AudioClip _brakeClip;
    [SerializeField] private AudioClip _collisionClip;
    [SerializeField] private AudioClip _shieldHitClip;
    [SerializeField] private AudioClip _healthHitClip;
    [SerializeField] private AudioClip _shieldBreakClip;
    [SerializeField] private AudioClip _explosionClip;
    [SerializeField] private AudioClip _respawnClip;

    // Engine pitch/volume scales with speed (from HovercraftStats)
    // Play one-shots for boost, brake, damage events
    // Subscribe to HovercraftHealth events and HovercraftController state changes
}
```

- Engine loop: pitch scales from minPitch to maxPitch based on `CurrentSpeed / MaxSpeed`
- Engine volume scales similarly
- Boost: play boost clip, slightly increase engine pitch
- Brake: play brake screech
- Damage: shield hit (energy zap) vs health hit (metal impact)
- Shield break: distinct warning sound
- Destruction: explosion + engine fadeout
- Respawn: power-up sound + engine restart

### Task 3: Hovercraft Customization Bridge

**HovercraftCustomization.cs** — Applies visual customizations to the hovercraft model:
```csharp
public class HovercraftCustomization : MonoBehaviour
{
    [SerializeField] private Renderer _bodyRenderer;
    [SerializeField] private Renderer _accentRenderer;
    [SerializeField] private Transform[] _attachPoints;  // named attachment points for parts
    [SerializeField] private Transform _decalTarget;

    public void ApplyColorScheme(Color primary, Color secondary, Color accent)
    {
        // Apply to material properties
        _bodyRenderer.material.color = primary;
        _accentRenderer.material.color = secondary;
        // accent applied to thruster glow, trim details
    }

    public void ApplyDecal(Texture2D decalTexture)
    {
        // Apply decal to decal projection or target renderer
    }

    public void AttachPart(GameObject partPrefab, string attachPointName)
    {
        // Find named attach point, instantiate part as child
    }

    public void ClearCustomizations()
    {
        // Reset to defaults
    }
}
```

- Listens to `EventBus.OnCosmeticEquipped` to apply changes
- Works with Agent 4's save data and Agent 5's customization UI
- Materials use property blocks (MaterialPropertyBlock) to avoid material instancing overhead

### Task 4: Visual Feedback System

**HovercraftVisuals.cs** — Full rewrite:
```csharp
public class HovercraftVisuals : MonoBehaviour
{
    [Header("Thrusters")]
    [SerializeField] private ParticleSystem[] _mainThrusters;     // blue glow underneath
    [SerializeField] private ParticleSystem[] _sideThrusters;     // orange side jets
    [SerializeField] private ParticleSystem _boostThruster;       // extra boost flame

    [Header("Damage")]
    [SerializeField] private ParticleSystem _sparksEffect;
    [SerializeField] private ParticleSystem _smokeEffect;         // plays when health < 50%
    [SerializeField] private ParticleSystem _fireEffect;          // plays when health < 25%
    [SerializeField] private GameObject _explosionPrefab;

    [Header("Screen Effects")]
    [SerializeField] private float _screenShakeIntensity = 0.3f;
    [SerializeField] private float _screenShakeDuration = 0.2f;

    // Methods:
    void UpdateThrusters(float speedNormalized, bool isBoosting)
    {
        // Scale thruster particle emission rate + speed based on velocity
        // Enable boost thruster when boosting
        // Side thrusters activate on turns
    }

    void PlayDamageEffect(DamageType type)
    {
        // Sparks always
        // Screen shake
        // Flash overlay (brief red tint)
        // Type-specific: Fire=orange flash, Ice=blue flash, Toxic=green flash
    }

    void UpdateDamageState(float healthNormalized)
    {
        // < 0.5: enable smoke
        // < 0.25: enable fire, increase smoke
        // Visual degradation of the hovercraft model (darken materials)
    }

    void PlayExplosion()
    {
        // Instantiate explosion prefab
        // Disable renderers
        // Called on destruction before respawn
    }

    void PlayRespawnEffect()
    {
        // Re-enable renderers
        // Flash shield bubble effect
        // Invincibility visual (slight transparency + shimmer)
    }
}
```

Key visual references from concept art:
- **Blue glow** underneath the hovercraft (hover jets)
- **Orange flame jets** from side/rear exhausts
- **Yellow/black** industrial color scheme as default
- **Skull and "73" decal** — these are customization options
- Heavy, chunky, industrial feel — sparks and metal debris on damage

### Task 5: Hovercraft Prefab Hierarchy Document

Document the exact prefab structure for Unity Editor setup:

```
Hovercraft (Root GameObject)
├── Components: Rigidbody, BoxCollider, HovercraftController,
│   HovercraftStats, HovercraftCustomization
├── Rigidbody Settings:
│   Mass: 10, Drag: 1, Angular Drag: 3
│   Use Gravity: true, Is Kinematic: false
│   Interpolation: Interpolate
│   Collision Detection: Continuous
├── BoxCollider: Approximate hovercraft hull shape
│
├── Model (child) — The 3D hovercraft mesh
│   ├── Components: MeshFilter, MeshRenderer
│   ├── Body_Renderer (for color customization)
│   └── Accent_Renderer (for secondary color)
│
├── Physics (child)
│   ├── Components: HovercraftPhysics
│   ├── HoverPoint_FL (empty transform — front-left hover raycast origin)
│   ├── HoverPoint_FR (empty transform — front-right)
│   ├── HoverPoint_BL (empty transform — back-left)
│   └── HoverPoint_BR (empty transform — back-right)
│   Position hover points at corners, slightly below center of mass
│
├── Input (child)
│   └── Components: HovercraftInput
│
├── Health (child)
│   └── Components: HovercraftHealth
│
├── Visuals (child)
│   ├── Components: HovercraftVisuals
│   ├── Thruster_Main_L (ParticleSystem — blue glow)
│   ├── Thruster_Main_R (ParticleSystem — blue glow)
│   ├── Thruster_Side_L (ParticleSystem — orange flame)
│   ├── Thruster_Side_R (ParticleSystem — orange flame)
│   ├── Thruster_Rear (ParticleSystem — main exhaust)
│   ├── Boost_Effect (ParticleSystem — big flame, default off)
│   ├── Sparks (ParticleSystem — on damage, default off)
│   ├── Smoke (ParticleSystem — low health, default off)
│   └── Fire (ParticleSystem — critical health, default off)
│
├── Audio (child)
│   ├── Components: HovercraftAudio
│   ├── EngineSource (AudioSource — looping)
│   └── SFXSource (AudioSource — one-shot)
│
├── Decal (child)
│   └── DecalProjector or quad mesh for decal display
│
└── AttachPoints (child)
    ├── Attach_Spoiler (empty transform — rear top)
    ├── Attach_Antenna (empty transform — roof)
    └── Attach_Exhaust (empty transform — rear)
```

### Task 6: Particle Effect Specifications

Document particle settings for each effect:

**Main Thrusters (Blue Glow)**:
- Shape: Cone pointing down, angle 15°
- Start Color: Cyan (#00DDFF) → Blue (#0044FF) over lifetime
- Start Size: 0.3 → 0.1 over lifetime
- Emission Rate: 50-200 (scales with speed)
- Lifetime: 0.3s
- Additive blending

**Side Thrusters (Orange Flame)**:
- Shape: Cone pointing outward, angle 10°
- Start Color: Orange (#FF8800) → Red (#FF2200) over lifetime
- Start Size: 0.2 → 0.05 over lifetime
- Emission Rate: 30-100
- Lifetime: 0.2s

**Boost Effect**:
- Shape: Cone pointing backward, angle 25°
- Start Color: White → Orange → Red
- Start Size: 0.5 → 0.2
- Emission Rate: 300
- Speed: 10-20
- Noise: enabled for turbulence

**Damage Sparks**:
- Shape: Sphere
- Start Color: Orange/Yellow
- Start Size: 0.05-0.1
- Emission: Burst of 20-50
- Gravity: 0.5 (sparks fall)
- Lifetime: 0.5s

**Smoke (Damaged)**:
- Shape: Cone up, angle 30°
- Start Color: Dark gray, fading to transparent
- Start Size: 0.3 → 1.0 over lifetime
- Emission: 10-30
- Lifetime: 1.5s
- Speed: 0.5-1.0

---

## Acceptance Criteria

- [ ] HovercraftHealth implements IDamageReceiver with DamageType support
- [ ] HovercraftStats implements IHovercraftData, applies upgrade multipliers
- [ ] HovercraftVisuals fully implemented with thruster scaling, damage VFX, explosion, respawn
- [ ] HovercraftAudio implemented with speed-based engine pitch, all event sounds
- [ ] HovercraftCustomization supports color, decal, and part attachment
- [ ] Prefab hierarchy documented with all component settings
- [ ] All particle effects documented with settings
- [ ] Editor keyboard controls still work for testing
- [ ] No direct references to Agent 3/4/5 code — communicate only via interfaces and EventBus

---

## Integration Contract

**What you provide to other agents:**
- Agent 3 needs: `IDamageReceiver` (on hovercraft collider, for hazards to call `TakeDamage`)
- Agent 5 needs: `IHovercraftData` (on hovercraft, for HUD to read health/speed/boost)
- Agent 4 needs: `HovercraftStats.ApplyUpgradeMultipliers()` (called when upgrades are loaded/purchased)
- Agent 5 needs: `HovercraftCustomization` public methods (called from Workshop UI)

**What you consume from other agents:**
- Agent 1: `IHovercraftData`, `IDamageReceiver`, `DamageType`, `GameConstants`, `HovercraftStatsSO`
- Agent 1: `EventBus.OnUpgradePurchased`, `EventBus.OnCosmeticEquipped`
