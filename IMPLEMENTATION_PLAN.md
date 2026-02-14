# Metal Pod - Implementation Plan

> **Hovercraft Obstacle Course Game**
> Platform: iOS (Unity Engine) | Target: May 2026
> Planner: Claude | Executor: Codex

---

## Project Overview

An iOS single-player obstacle course racing game where players control a customizable hovercraft ("Metal Pod") through hazardous environments (lava, ice, toxic wasteland). Heavy metal aesthetic throughout. Physics-based controls with tilt steering + touch input. Workshop hub for upgrades and customization. Medal-based progression system.

---

## Project Structure

```
Metal_Pod/
├── Assets/
│   ├── Art/                        # Concept art reference
│   ├── Scripts/
│   │   ├── Core/                   # Game manager, state machine, singletons
│   │   │   ├── GameManager.cs
│   │   │   ├── GameStateManager.cs
│   │   │   ├── SceneLoader.cs
│   │   │   └── AudioManager.cs
│   │   ├── Hovercraft/             # Vehicle systems
│   │   │   ├── HovercraftController.cs
│   │   │   ├── HovercraftPhysics.cs
│   │   │   ├── HovercraftInput.cs
│   │   │   ├── HovercraftHealth.cs
│   │   │   ├── HovercraftStats.cs
│   │   │   └── HovercraftVisuals.cs
│   │   ├── Course/                 # Level & hazard systems
│   │   │   ├── CourseManager.cs
│   │   │   ├── Checkpoint.cs
│   │   │   ├── CourseTimer.cs
│   │   │   ├── MedalSystem.cs
│   │   │   ├── Collectible.cs
│   │   │   └── FinishLine.cs
│   │   ├── Hazards/                # Environmental hazards
│   │   │   ├── HazardBase.cs
│   │   │   ├── DamageZone.cs
│   │   │   ├── LavaFlow.cs
│   │   │   ├── FallingDebris.cs
│   │   │   ├── ToxicZone.cs
│   │   │   ├── IcePatch.cs
│   │   │   └── HazardWarning.cs
│   │   ├── Progression/            # Save, unlock, currency
│   │   │   ├── ProgressionManager.cs
│   │   │   ├── SaveSystem.cs
│   │   │   ├── CurrencyManager.cs
│   │   │   └── CourseUnlockData.cs
│   │   ├── Workshop/               # Hub & upgrades
│   │   │   ├── WorkshopManager.cs
│   │   │   ├── UpgradeSystem.cs
│   │   │   ├── CustomizationSystem.cs
│   │   │   └── CourseSelectionUI.cs
│   │   └── UI/                     # All UI controllers
│   │       ├── MainMenuUI.cs
│   │       ├── HUD.cs
│   │       ├── PauseMenuUI.cs
│   │       ├── ResultsScreenUI.cs
│   │       ├── SettingsUI.cs
│   │       └── UIManager.cs
│   ├── Prefabs/
│   │   ├── Hovercraft/
│   │   ├── Hazards/
│   │   ├── Environment/
│   │   ├── UI/
│   │   └── Effects/
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   ├── Workshop.unity
│   │   ├── Lava_Course_01.unity
│   │   ├── Lava_Course_02.unity
│   │   ├── Lava_Course_03.unity
│   │   ├── Ice_Course_01.unity
│   │   ├── Ice_Course_02.unity
│   │   ├── Ice_Course_03.unity
│   │   ├── Toxic_Course_01.unity
│   │   ├── Toxic_Course_02.unity
│   │   └── Toxic_Course_03.unity
│   ├── Materials/
│   ├── Textures/
│   ├── Audio/
│   │   ├── Music/
│   │   ├── SFX/
│   │   └── Ambient/
│   ├── Animations/
│   ├── Shaders/
│   ├── ScriptableObjects/
│   │   ├── HovercraftStatsSO.cs
│   │   ├── CourseDataSO.cs
│   │   ├── UpgradeDataSO.cs
│   │   ├── HazardDataSO.cs
│   │   └── EnvironmentDataSO.cs
│   └── Config/
│       ├── GameConfig.asset
│       ├── ControlConfig.asset
│       └── BalanceConfig.asset
├── Packages/
├── ProjectSettings/
├── Art/                            # Source concept art (existing)
├── Hovercraft_Game_SRS.docx        # Requirements document (existing)
├── IMPLEMENTATION_PLAN.md          # This file
└── README.md
```

---

## Phase 1: Core Mechanics

**Goal**: Playable hovercraft with physics, controls, collisions, and health in a test course.

### Task 1.1 — Unity Project Setup
- Create Unity 2022 LTS project targeting iOS
- Configure project settings:
  - Target iOS 15.0+
  - Set render pipeline (URP for mobile performance)
  - Configure quality settings for 60 FPS target
  - Set up input system (New Input System package)
  - Configure accelerometer access
- Set up folder structure as defined above
- Create `.gitignore` for Unity project (exclude Library/, Temp/, Build/, etc.)
- Import reference concept art into `Assets/Art/`

### Task 1.2 — ScriptableObject Data Architecture
Create data-driven architecture using ScriptableObjects:

**HovercraftStatsSO.cs**
```csharp
[CreateAssetMenu(fileName = "HovercraftStats", menuName = "MetalPod/HovercraftStats")]
public class HovercraftStatsSO : ScriptableObject
{
    [Header("Movement")]
    public float baseSpeed = 20f;
    public float maxSpeed = 40f;
    public float boostMultiplier = 1.5f;
    public float boostDuration = 3f;
    public float boostCooldown = 5f;
    public float brakeForce = 15f;
    public float turnSpeed = 3f;

    [Header("Hover Physics")]
    public float hoverHeight = 2f;
    public float hoverForce = 65f;
    public float hoverDamping = 5f;
    public int raycastCount = 4;       // corners of the craft

    [Header("Durability")]
    public float maxHealth = 100f;
    public float maxShield = 50f;
    public float shieldRegenRate = 5f;  // per second
    public float shieldRegenDelay = 3f; // seconds after last hit

    [Header("Handling")]
    public float driftFactor = 0.95f;
    public float tiltSensitivity = 1f;
    public float stabilizationForce = 10f;
}
```

**CourseDataSO.cs**
```csharp
[CreateAssetMenu(fileName = "CourseData", menuName = "MetalPod/CourseData")]
public class CourseDataSO : ScriptableObject
{
    public string courseId;
    public string courseName;
    public string description;
    public EnvironmentType environmentType;
    public int courseIndex;              // order within environment
    public string sceneName;

    [Header("Medal Thresholds (seconds)")]
    public float goldTime;
    public float silverTime;
    public float bronzeTime;

    [Header("Unlock Requirements")]
    public int requiredMedals;           // total medals needed to unlock
    public CourseDataSO prerequisiteCourse;

    [Header("Preview")]
    public Sprite previewImage;
    public string[] hazardDescriptions;
    public DifficultyLevel difficulty;
}

public enum EnvironmentType { Lava, Ice, Toxic }
public enum DifficultyLevel { Easy, Medium, Hard, Extreme }
```

**UpgradeDataSO.cs**
```csharp
[CreateAssetMenu(fileName = "UpgradeData", menuName = "MetalPod/UpgradeData")]
public class UpgradeDataSO : ScriptableObject
{
    public string upgradeId;
    public string upgradeName;
    public string description;
    public UpgradeCategory category;
    public Sprite icon;

    [Header("Levels")]
    public UpgradeLevel[] levels;

    [Header("Stat Modifications")]
    public StatModifier[] statModifiers;
}

public enum UpgradeCategory { Speed, Handling, Shield, Boost }

[System.Serializable]
public class UpgradeLevel
{
    public int cost;
    public float statMultiplier;
    public string description;
}

[System.Serializable]
public class StatModifier
{
    public string statName;
    public float valuePerLevel;
}
```

### Task 1.3 — Hovercraft Physics System
**HovercraftPhysics.cs** — Core hover mechanics using Rigidbody:
- Implement 4-point raycast hover system (one ray per corner of craft)
- Each ray applies upward force proportional to distance from ground
- Apply spring-damper formula: `force = (hoverHeight - hitDistance) * hoverForce - rb.velocity.y * hoverDamping`
- Apply gravity when no ground detected (freefall)
- Implement momentum and inertia using Rigidbody drag/angular drag
- Implement drift factor: reduce sideways grip for sliding feel
- Implement stabilization torque to keep craft level when not turning
- Altitude control: adjust hover height target based on input

Key physics parameters (tunable via HovercraftStatsSO):
- Hover height: 2 units
- Hover force: 65
- Damping: 5
- Drift factor: 0.95 (1 = no drift, 0 = full drift)

### Task 1.4 — Input System
**HovercraftInput.cs** — Input handling:
- **Tilt steering**: Read `Input.acceleration` (or New Input System accelerometer)
  - Map device tilt X-axis to turning
  - Apply deadzone (±0.05) and sensitivity curve
  - Adjustable sensitivity stored in PlayerPrefs
- **Touch zones**: Divide screen into control regions
  - Right side bottom: Boost button (tap to activate)
  - Left side bottom: Brake zone (hold to brake)
  - Special ability button: top corner
- **Altitude**: Tilt Y-axis for subtle altitude adjustments
- Provide haptic feedback via iOS Taptic Engine on boost, damage, collisions

### Task 1.5 — Hovercraft Controller
**HovercraftController.cs** — Ties physics + input together:
- Forward thrust: constant force in forward direction, scaled by speed stat
- Turning: apply torque based on tilt input * turnSpeed
- Boost: temporary speed multiplier with cooldown timer
- Brake: increased drag + counter-force
- State management: Normal, Boosting, Braking, Damaged, Destroyed
- Reference HovercraftStatsSO for all values

### Task 1.6 — Health & Damage System
**HovercraftHealth.cs**:
- Health and Shield float values (from HovercraftStatsSO)
- Damage pipeline: Shield absorbs first → then Health
- Shield regeneration after delay (configurable)
- Damage affects performance: below 50% health = reduced max speed, below 25% = reduced handling
- Visual feedback: screen shake, flash overlay, particle sparks
- Destruction: trigger explosion VFX → respawn at last checkpoint
- Events: `OnDamage`, `OnShieldBreak`, `OnHealthChanged`, `OnDestroyed`

### Task 1.7 — Test Course (Greybox)
- Create a simple greybox test level:
  - Flat terrain with ramps, gaps, walls, and narrow passages
  - Start line and finish line triggers
  - 3 checkpoints placed throughout
  - Basic collision geometry (box/mesh colliders)
  - Simple skybox and directional light
- Purpose: validate hover physics, controls, and collision feel
- No environment art needed — primitive shapes only

### Task 1.8 — Checkpoint System
**Checkpoint.cs**:
- Trigger collider that saves respawn position + rotation
- Visual indicator (pillar/ring that changes color when activated)
- Track last activated checkpoint per run

**CourseManager.cs**:
- Manages course lifecycle: countdown → racing → finished/failed
- Handles respawning at checkpoints on destruction
- Tracks completion conditions

### Task 1.9 — Basic HUD
**HUD.cs** — Minimal gameplay overlay:
- Health bar (red, depleting left to right)
- Shield bar (blue, above health)
- Speed indicator (numeric or gauge)
- Timer (mm:ss.ms format)
- Boost cooldown indicator
- Hazard warning zone (for Phase 2)

### Task 1.10 — Game State Manager
**GameStateManager.cs**:
- States: `MainMenu`, `Workshop`, `Loading`, `Racing`, `Paused`, `Results`
- Manages transitions between states
- Handles pause/resume (Time.timeScale)
- Scene loading coordination

**GameManager.cs** (Singleton):
- Persistent across scenes (DontDestroyOnLoad)
- Holds references to active player data, current course
- Initializes subsystems

---

## Phase 2: First Environment (Lava/Volcanic)

**Goal**: Complete lava environment with 3 courses, hazards, medal system, visual effects.

### Task 2.1 — Lava Environment Art Direction
- Color palette: deep reds, oranges, dark rock, glowing magma veins
- Skybox: smoky orange/red sky with volcanic ash particles
- Terrain: black volcanic rock with lava rivers and pools
- Lighting: warm orange point lights from lava, volumetric fog
- Post-processing: bloom on lava, heat distortion (shader), orange ambient

### Task 2.2 — Hazard Base System
**HazardBase.cs** (Abstract):
```csharp
public abstract class HazardBase : MonoBehaviour
{
    public HazardDataSO hazardData;
    public float damagePerSecond;
    public float damagePerHit;
    public bool isActive = true;

    protected virtual void OnTriggerEnter(Collider other) { /* apply hit damage */ }
    protected virtual void OnTriggerStay(Collider other) { /* apply DoT damage */ }
    public virtual void Activate() { isActive = true; }
    public virtual void Deactivate() { isActive = false; }
}
```

### Task 2.3 — Lava-Specific Hazards
Implement hazard classes inheriting from HazardBase:

1. **LavaFlow.cs**: Animated lava streams crossing the path
   - Scrolling UV texture on lava surface
   - Damage on contact (high DPS)
   - Glow/heat particle emission
   - Some flows are timed (intermittent)

2. **VolcanicEruption.cs**: Periodic eruptions launching debris
   - Warning indicator before eruption (ground shake + audio cue)
   - Spawns falling rock projectiles in area
   - Projectiles have arc trajectory + shadow indicator on ground

3. **FallingDebris.cs**: Rocks falling from ceiling/cliffs
   - Shadow/circle indicator on ground before impact
   - Physics-enabled debris on impact
   - Single-hit damage

4. **HeatZone.cs**: Areas of extreme heat (damage over time)
   - Visual: heat shimmer shader effect
   - Gradual damage while in zone
   - Screen distortion feedback

5. **LavaGeyser.cs**: Geysers that erupt periodically
   - Timed eruption cycle
   - Vertical burst that can launch/damage hovercraft
   - Warning: bubbling before eruption

### Task 2.4 — Hazard Warning System
**HazardWarning.cs**:
- Directional indicator arrows on HUD pointing toward incoming hazards
- Color-coded by threat level (yellow = caution, red = imminent)
- Audio cue for approaching hazards
- Screen edge glow in direction of hazard

### Task 2.5 — Medal System
**MedalSystem.cs**:
```csharp
public enum Medal { None, Bronze, Silver, Gold }

public class MedalSystem
{
    public Medal EvaluatePerformance(float completionTime, CourseDataSO courseData)
    {
        if (completionTime <= courseData.goldTime) return Medal.Gold;
        if (completionTime <= courseData.silverTime) return Medal.Silver;
        if (completionTime <= courseData.bronzeTime) return Medal.Bronze;
        return Medal.None;
    }
}
```

### Task 2.6 — Course Timer
**CourseTimer.cs**:
- Start on race begin (after countdown)
- Pause during respawn transitions
- Display on HUD in mm:ss.ms
- Stop on course completion
- Feed final time to medal evaluation

### Task 2.7 — Collectibles
**Collectible.cs**:
- Floating pickups scattered off the main path (risk/reward)
- Types: Currency bonus, small health restore, shield recharge
- Spin animation + glow effect
- Magnet pull when hovercraft is nearby
- Track collected per run

### Task 2.8 — Results Screen
**ResultsScreenUI.cs**:
- Display after course completion:
  - Completion time
  - Medal earned (with animation for new best)
  - Currency awarded (base + medal bonus + collectible bonus)
  - Damage taken / health remaining
  - Collectibles found (X/Y)
- Buttons: Retry, Next Course, Return to Workshop
- Compare against personal best

### Task 2.9 — Lava Course 1 (Tutorial/Easy)
- Purpose: Introduce core mechanics
- Length: ~60 seconds at normal speed
- Hazards: Simple lava pools (static), slow falling debris
- Layout: Wide paths, gentle curves, clear sightlines
- Medal times: Gold 50s, Silver 65s, Bronze 80s
- 5 collectibles placed on slightly risky paths
- 3 checkpoints

### Task 2.10 — Lava Course 2 (Medium)
- Introduce timed lava flows and geysers
- Narrower paths with more turns
- Vertical sections (ramps over lava rivers)
- Medal times: Gold 70s, Silver 90s, Bronze 110s
- 8 collectibles with some requiring altitude control
- 4 checkpoints

### Task 2.11 — Lava Course 3 (Hard)
- All lava hazard types active
- Volcanic eruption sequences (scripted event mid-course)
- Tight corridors with lava on both sides
- Moving platforms over lava
- Medal times: Gold 90s, Silver 120s, Bronze 150s
- 10 collectibles in very risky positions
- 5 checkpoints

---

## Phase 3: Progression Systems

**Goal**: Workshop hub, upgrade system, currency, customization, save/load, protagonist.

### Task 3.1 — Save System
**SaveSystem.cs**:
- Use JSON serialization to Application.persistentDataPath
- Save data structure:
```csharp
[System.Serializable]
public class SaveData
{
    public int currency;
    public Dictionary<string, int> upgradeLevels;       // upgradeId -> level
    public Dictionary<string, float> bestTimes;          // courseId -> time
    public Dictionary<string, int> bestMedals;           // courseId -> medal enum
    public Dictionary<string, bool> unlockedCourses;     // courseId -> unlocked
    public List<string> ownedCosmetics;                  // cosmeticId list
    public string equippedColor;
    public string equippedDecal;
    public int totalMedals;
    public float totalPlayTime;
}
```
- Auto-save after course completion
- Auto-save after purchases
- Load on game start
- Handle missing/corrupted save gracefully (reset to defaults)

### Task 3.2 — Currency System
**CurrencyManager.cs**:
- Award currency on course completion:
  - Base reward per course (first completion bonus)
  - Medal bonuses: Bronze +25%, Silver +50%, Gold +100%
  - Collectible bonuses
  - Replay reward (reduced, ~50% of base)
- Display currency in workshop UI
- Deduct on upgrade purchase
- Events: `OnCurrencyChanged`

### Task 3.3 — Upgrade System
**UpgradeSystem.cs**:
- Categories with tiered levels (1-5):
  - **Speed**: Increases max speed + acceleration
  - **Handling**: Improves turn rate + reduces drift
  - **Shields**: Increases max shield + regen rate
  - **Boost**: Increases boost power + reduces cooldown
- Each level costs progressively more (e.g., 100, 250, 500, 1000, 2000)
- Apply upgrades by modifying HovercraftStatsSO values at runtime
- Preview stat changes before purchase (show current → new values)
- Persist upgrade levels in save data

### Task 3.4 — Workshop Scene
**WorkshopManager.cs**:
- 3D workshop environment:
  - Garage/workshop setting matching concept art (industrial, tools, parts)
  - Heavy metal posters, amplifiers, guitar on wall (aesthetic)
  - Hovercraft displayed on lift/platform in center
  - Protagonist character working on/near hovercraft
- Camera: fixed angle showing workshop + hovercraft
- UI overlays for upgrade menu, course selection, customization
- Smooth transitions to/from gameplay scenes

### Task 3.5 — Customization System
**CustomizationSystem.cs**:
- **Colors**: Unlock and apply color schemes to hovercraft
  - Default: Yellow/Black (from concept art)
  - Purchasable: Red/Black, Blue/Silver, Green/Black, Chrome, etc.
- **Decals**: Skull, flames, lightning, racing stripes, numbers
  - Some unlocked via medals, others purchased
- **Parts**: Visual-only attachments (exhaust types, antenna, spoiler)
- Apply changes to material properties in real-time
- Save equipped cosmetics

### Task 3.6 — Course Selection UI
**CourseSelectionUI.cs**:
- Environment tabs (Lava, Ice, Toxic) - locked environments grayed out
- Course list per environment showing:
  - Course name + preview image
  - Difficulty indicator
  - Best time / Best medal (or "Locked" with requirements)
  - Hazard type icons
- Lock icon + requirement text for locked courses
- "Launch" button for selected course
- Unlock logic: minimum total medals across all courses

### Task 3.7 — Protagonist Character
- 18-year-old character model (low-poly mobile-appropriate)
- Heavy metal aesthetic: band t-shirt, jeans, boots, messy hair
- Workshop animations:
  - Idle: leaning against workbench
  - Working: wrench/tool animation on hovercraft
  - Celebrating: fist pump (on medal achievement)
- Character is visible in workshop only (not during racing)

### Task 3.8 — Main Menu
**MainMenuUI.cs**:
- Background: workshop environment (blurred or 3D)
- Heavy metal styled title: "METAL POD"
- Options:
  - **Continue** (if save exists) → Workshop
  - **New Game** → Tutorial/first course
  - **Settings** → Settings menu
- Metal-themed UI: riveted panels, industrial fonts, orange/dark palette

### Task 3.9 — Settings Menu
**SettingsUI.cs**:
- **Controls**: Tilt sensitivity slider (0.5x - 2.0x)
- **Audio**: Master volume, Music volume, SFX volume
- **Graphics**: Quality preset (Low/Medium/High)
- **Haptics**: Toggle on/off
- **Invert tilt**: Toggle
- Save settings to PlayerPrefs
- Accessible from Main Menu and Pause Menu

---

## Phase 4: Additional Content

**Goal**: Ice and Toxic environments with 3-5 courses each, unique hazards.

### Task 4.1 — Ice/Arctic Environment
- Color palette: whites, pale blues, deep navy, ice crystal sparkles
- Skybox: blizzard/aurora borealis
- Terrain: ice sheets, snow-covered rock, frozen caverns
- Lighting: cool blue-white, aurora volumetric effects
- Special physics: reduced friction on ice surfaces (modify drift factor)
- Post-processing: slight blue tint, frost vignette at screen edges

### Task 4.2 — Ice-Specific Hazards
1. **IcePatch.cs**: Slippery surfaces reducing control
   - Modify hovercraft drift factor when over ice
   - Visual: glossy reflective surface

2. **FallingIcicle.cs**: Stalactites that break and fall
   - Cracking audio + visual cue before fall
   - Shadow indicator on ground

3. **BlizzardZone.cs**: Reduced visibility areas
   - Particle system: heavy snow
   - Camera fog increase
   - Wind force pushing hovercraft sideways

4. **IceWall.cs**: Breakable ice barriers
   - Can be broken with boost (rewards risk-taking)
   - Block path if not boosted through → must find alternate route

5. **Avalanche.cs**: Scripted wall of snow/ice from behind
   - Forces forward momentum (can't slow down)
   - Timer-based: survive until avalanche dissipates

### Task 4.3 — Ice Courses (3 courses)
- **Ice Course 1 (Medium)**: Open frozen lake with icicle falls, gentle blizzard
- **Ice Course 2 (Hard)**: Cavern system with tight ice corridors, ice walls, patches
- **Ice Course 3 (Extreme)**: Mountain pass with avalanche sequence, full blizzard, all ice hazards

### Task 4.4 — Toxic/Industrial Wasteland Environment
- Color palette: sickly greens, rusted oranges, dark browns, neon warning signs
- Skybox: polluted haze, green-tinged clouds
- Terrain: rusted metal platforms, concrete ruins, toxic sludge pools
- Lighting: fluorescent greens, industrial overhead lights (flickering)
- Post-processing: slight green tint, chromatic aberration in toxic zones

### Task 4.5 — Toxic-Specific Hazards
1. **ToxicGas.cs**: Clouds of corrosive gas
   - Damage over time while inside
   - Obscures vision (green fog)
   - Periodic bursts from vents

2. **AcidPool.cs**: Corrosive liquid pools
   - High damage on contact
   - Splashing particle effects
   - Some pools have rising/falling levels

3. **IndustrialPress.cs**: Crushing machinery
   - Timed pistons that slam down
   - Must time passage through gaps
   - Instant destruction on hit

4. **ElectricFence.cs**: Electrified barriers
   - Intermittent on/off pattern
   - Stun effect on hovercraft (brief control loss)
   - Sparking visual + audio

5. **BarrelExplosion.cs**: Explosive barrels scattered around
   - Chain reaction possible
   - Area damage
   - Can be used strategically (clear obstacles)

### Task 4.6 — Toxic Courses (3 courses)
- **Toxic Course 1 (Medium)**: Abandoned factory with gas vents and acid pools
- **Toxic Course 2 (Hard)**: Industrial complex with presses, electric fences, rising acid
- **Toxic Course 3 (Extreme)**: Collapsing refinery — everything is hazardous, chain explosions

### Task 4.7 — Course Unlock Progression
Define the full unlock chain:
```
Lava 1: Unlocked by default (tutorial)
Lava 2: Complete Lava 1 with any medal
Lava 3: Complete Lava 2 + 3 total medals
Ice 1:  Complete Lava 3 + 5 total medals
Ice 2:  Complete Ice 1 + 7 total medals
Ice 3:  Complete Ice 2 + 9 total medals
Toxic 1: Complete Ice 3 + 12 total medals
Toxic 2: Complete Toxic 1 + 15 total medals
Toxic 3: Complete Toxic 2 + 18 total medals
```

### Task 4.8 — Environment Transition Effects
- Loading screen themed to destination environment
- Brief cinematic: hovercraft arriving at new environment
- Environmental audio transition (ambience crossfade)

---

## Phase 5: Polish & Testing

**Goal**: UI refinement, audio, visual polish, performance, bug fixes, iOS submission prep.

### Task 5.1 — Audio Implementation
- **Music**: Heavy metal soundtrack (or royalty-free metal tracks)
  - Main menu theme
  - Workshop ambient + light music
  - Per-environment racing tracks (high energy)
  - Results screen fanfare (medal-dependent)
- **SFX**:
  - Hovercraft: engine hum (pitch-shifted by speed), boost whoosh, brake screech
  - Hazards: lava bubbling, ice cracking, acid sizzling, explosions, electrical zaps
  - UI: button clicks, purchase confirmation, medal earned chime
  - Damage: impact thuds, shield hit (energy sound), destruction explosion
- **Ambient**:
  - Lava: fire crackling, distant rumbling
  - Ice: wind howling, ice creaking
  - Toxic: industrial hum, dripping, machinery
- AudioManager with pooled AudioSources, volume controls

### Task 5.2 — Visual Polish
- Particle effects for all hazards and hovercraft
- Hovercraft thruster effects (matching concept art — blue glow underneath, orange side jets)
- Environment-specific post-processing profiles
- Screen shake on impacts (cinemachine impulse)
- Damage visual feedback (cracks overlay, smoke from hovercraft)
- Medal award celebration effects
- UI animations (slide-in panels, number count-up for currency)

### Task 5.3 — UI Refinement
- Implement heavy metal UI theme consistently:
  - Riveted metal panel backgrounds
  - Industrial/gothic font
  - Orange/amber accent color on dark backgrounds
  - Skull and gear motifs in decorative elements
- Ensure 44x44pt minimum touch targets
- Test on all target screen sizes (iPhone 12 through current)
- Support notch/Dynamic Island safe areas
- Smooth transitions between all UI states

### Task 5.4 — Performance Optimization
- Profile on target devices (iPhone 12 baseline)
- Maintain 60 FPS:
  - LOD system for environment geometry
  - Texture compression (ASTC for iOS)
  - Particle budget limits
  - Object pooling for hazards and collectibles
  - Occlusion culling for courses
  - Shader optimization (mobile-friendly shaders)
- Load time optimization:
  - Addressable assets for level content
  - Async scene loading with progress bar
  - Target < 3 second load times
- Memory management:
  - Unload unused assets between scenes
  - Monitor memory warnings
  - Target < 500MB runtime memory

### Task 5.5 — Bug Fixing & Edge Cases
- Handle app lifecycle events (background/foreground, phone calls)
- Handle low memory warnings (reduce quality, unload caches)
- Validate save data integrity on load
- Test all checkpoint respawn scenarios
- Test all hazard interaction combinations
- Verify medal thresholds are achievable but challenging
- Fix any physics edge cases (clipping, stuck states, infinite falls)

### Task 5.6 — Balance & Playtesting
- Verify difficulty curve across all 9 courses
- Balance upgrade costs vs. currency earn rates
- Ensure all medal times are fair but challenging
- Verify hazard damage values create tension without frustration
- Test full progression: can a player unlock everything without grinding excessively?
- Adjust as needed based on playtesting

### Task 5.7 — iOS Submission Preparation
- App Store assets:
  - App icon (1024x1024)
  - Screenshots for required device sizes
  - App preview video (30 seconds)
  - App description and keywords
- Configure Xcode project:
  - Bundle identifier
  - Signing certificates
  - Capabilities (no special entitlements needed)
  - Privacy manifest (no data collection)
- App Store Review Guidelines compliance check
- Build archive and test via TestFlight
- Submit for review

---

## Architecture Notes for Codex

### Key Design Patterns
1. **Singleton** for managers (GameManager, AudioManager) — use DontDestroyOnLoad
2. **ScriptableObjects** for all data (stats, courses, upgrades) — easy to tune without code changes
3. **Observer pattern** via C# events for decoupled communication (health changes, currency updates, etc.)
4. **Object pooling** for frequently spawned objects (hazards, projectiles, collectibles, particles)
5. **State machine** for game states and hovercraft states

### Critical Technical Decisions
- **Physics**: Use Rigidbody with custom forces, NOT CharacterController. Hovercraft needs real physics.
- **Hover system**: 4-point raycast with spring-damper. This is the heart of the game feel.
- **Input**: Use Unity's New Input System for accelerometer access and touch handling.
- **Save data**: JSON to persistentDataPath. Do NOT use PlayerPrefs for game saves (too limited). PlayerPrefs only for settings.
- **UI**: Unity UI (Canvas-based) with a UIManager coordinating all panels.
- **Scenes**: One scene per course + Workshop + MainMenu. Use async loading.

### Naming Conventions
- PascalCase for classes, methods, properties, events
- camelCase for local variables and parameters
- _camelCase for private fields
- UPPER_SNAKE for constants
- Suffix ScriptableObjects with `SO`
- Suffix UI scripts with `UI`
- Suffix MonoBehaviours that are managers with `Manager`

### Testing Priority
After each phase, validate:
1. Does the hovercraft feel good to control?
2. Are hazards readable and fair?
3. Is performance within budget?
4. Does save/load work reliably?
5. Are all UI flows navigable?

---

## Dependency Graph

```
Phase 1 (Core Mechanics)
  ├── 1.1 Project Setup
  ├── 1.2 ScriptableObject Architecture
  ├── 1.3 Hovercraft Physics ← depends on 1.1, 1.2
  ├── 1.4 Input System ← depends on 1.1
  ├── 1.5 Hovercraft Controller ← depends on 1.3, 1.4
  ├── 1.6 Health & Damage ← depends on 1.2
  ├── 1.7 Test Course ← depends on 1.1
  ├── 1.8 Checkpoint System ← depends on 1.7
  ├── 1.9 Basic HUD ← depends on 1.6
  └── 1.10 Game State Manager ← depends on 1.1

Phase 2 (Lava Environment) ← depends on Phase 1
  ├── 2.1 Lava Art Direction
  ├── 2.2 Hazard Base System
  ├── 2.3 Lava Hazards ← depends on 2.2
  ├── 2.4 Hazard Warning ← depends on 2.2
  ├── 2.5 Medal System
  ├── 2.6 Course Timer
  ├── 2.7 Collectibles
  ├── 2.8 Results Screen ← depends on 2.5, 2.6
  ├── 2.9 Lava Course 1 ← depends on 2.1, 2.3
  ├── 2.10 Lava Course 2 ← depends on 2.9
  └── 2.11 Lava Course 3 ← depends on 2.10

Phase 3 (Progression) ← depends on Phase 2
  ├── 3.1 Save System
  ├── 3.2 Currency System ← depends on 3.1
  ├── 3.3 Upgrade System ← depends on 3.1, 3.2
  ├── 3.4 Workshop Scene ← depends on 3.3
  ├── 3.5 Customization ← depends on 3.1
  ├── 3.6 Course Selection UI ← depends on 3.1
  ├── 3.7 Protagonist Character
  ├── 3.8 Main Menu
  └── 3.9 Settings Menu

Phase 4 (Additional Content) ← depends on Phase 3
  ├── 4.1 Ice Environment Art
  ├── 4.2 Ice Hazards ← depends on 2.2
  ├── 4.3 Ice Courses ← depends on 4.1, 4.2
  ├── 4.4 Toxic Environment Art
  ├── 4.5 Toxic Hazards ← depends on 2.2
  ├── 4.6 Toxic Courses ← depends on 4.4, 4.5
  ├── 4.7 Unlock Progression ← depends on 4.3, 4.6
  └── 4.8 Environment Transitions

Phase 5 (Polish) ← depends on Phase 4
  ├── 5.1 Audio
  ├── 5.2 Visual Polish
  ├── 5.3 UI Refinement
  ├── 5.4 Performance Optimization
  ├── 5.5 Bug Fixing
  ├── 5.6 Balance & Playtesting
  └── 5.7 iOS Submission
```

---

*This plan is designed for Codex to execute sequentially. Each task is self-contained with enough detail to implement independently. Phase boundaries are natural testing/validation checkpoints.*
