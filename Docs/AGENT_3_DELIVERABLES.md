# Agent 3 Deliverables: Courses, Hazards, Environment Direction

## 1. Greybox Test Course (`TestCourse.unity`) Specification

### Layout
- Start -> Straight (50m) -> Wide Left Turn -> Straight with Ramp -> Gap (jump) -> Narrow Corridor (10m) -> S-Curve -> Straight with Walls -> Finish
- Total length: ~300m
- Width: 15m for wide sections, 6m for narrow sections
- Checkpoints: 3
- Collectibles: 5

### Checkpoint Placement
- Checkpoint 1: after the wide left turn
- Checkpoint 2: after the gap jump landing zone
- Checkpoint 3: after the S-curve exit

### Collectible Placement
- Easy (on-path): 2
- Risk/reward (edges/ramp): 3

### Scene Hierarchy
```
TestCourse
|- Environment
|  |- Ground
|  |- Walls
|  |- Ramps
|  |- Gaps
|- Course
|  |- StartLine
|  |- Checkpoint_1
|  |- Checkpoint_2
|  |- Checkpoint_3
|  |- FinishLine
|  |- CourseManager
|- Collectibles
|  |- Collectible_01
|  |- Collectible_02
|  |- Collectible_03
|  |- Collectible_04
|  |- Collectible_05
|- Player
|  |- Hovercraft
|- Camera
|  |- Main Camera
|- Lighting
|  |- Directional Light
|  |- Skybox
```

## 2. Course Layouts (9)

## Lava Courses

### Lava_Course_01: Inferno Gate (Easy/Tutorial)
- Length: ~400m
- Hazards: edge lava pools, 2 slow falling-debris zones
- Checkpoints: 3
- Collectibles: 5
- Medal times: Gold 50s, Silver 65s, Bronze 80s
- Intent: teach edge safety and basic timing

### Lava_Course_02: Magma Run (Medium)
- Length: ~600m
- Hazards: 3 timed lava flow crossings, 4 geysers, 2 falling-debris areas
- Checkpoints: 4
- Collectibles: 8
- Medal times: Gold 70s, Silver 90s, Bronze 110s
- Intent: multi-pattern timing and lane commitment

### Lava_Course_03: Eruption (Hard)
- Length: ~800m
- Hazards: lava hazards set (flows/geysers/heat zones/debris) + scripted eruption at midpoint
- Checkpoints: 5
- Collectibles: 10 (high risk)
- Medal times: Gold 90s, Silver 120s, Bronze 150s
- Climax: 15-second debris rain section in tight lava corridors

## Ice Courses

### Ice_Course_01: Frozen Lake (Medium)
- Hazards: broad ice patches, falling icicles, light blizzard wind
- Checkpoints: 3
- Collectibles: 6
- Medal times: Gold 60s, Silver 80s, Bronze 100s
- Intent: traction loss adaptation and controlled cornering

### Ice_Course_02: Crystal Caverns (Hard)
- Hazards: ice patches, icicles, breakable ice walls, blizzard pockets
- Checkpoints: 4
- Collectibles: 8
- Medal times: Gold 80s, Silver 105s, Bronze 130s
- Intent: shortcut risk/reward via boost break-throughs

### Ice_Course_03: Avalanche Pass (Extreme)
- Hazards: full ice set + avalanche chase + heavy blizzard in final third
- Checkpoints: 5
- Collectibles: 10
- Medal times: Gold 100s, Silver 130s, Bronze 160s
- Climax: sustained survival sprint while avalanche pressure escalates

## Toxic Courses

### Toxic_Course_01: Waste Disposal (Medium)
- Hazards: periodic toxic gas vents, static acid pools, explosive barrels
- Checkpoints: 3
- Collectibles: 6
- Medal times: Gold 65s, Silver 85s, Bronze 105s
- Intent: hazard readability and area-denial routing

### Toxic_Course_02: The Foundry (Hard)
- Hazards: timed presses, intermittent electric fences, rising acid pools, gas vents
- Checkpoints: 4
- Collectibles: 8
- Medal times: Gold 85s, Silver 110s, Bronze 140s
- Intent: timing windows under layered pressure

### Toxic_Course_03: Meltdown (Extreme)
- Hazards: full toxic set + chain barrel routes + rising acid + collapse segments
- Checkpoints: 5
- Collectibles: 10
- Medal times: Gold 110s, Silver 145s, Bronze 180s
- Climax: dynamic barrel-chain path opening/blocking during final run

## 3. Environment Art Direction

## Lava / Volcanic
- Ground: dark basalt volcanic rock (near-black to charcoal)
- Accents: emissive magma veins (orange/red)
- Sky: smoky orange-red with ash drift
- Lighting: warm, low-angle orange key and emissive bounce from lava
- Post FX: emissive bloom on magma, subtle orange grading
- Particles: ash motes, embers, lava bubbling plumes

## Ice / Arctic
- Ground: glossy blue-white ice sheets with snow crust transitions
- Accents: pale-blue crystal formations (mild emissive)
- Sky: cold blue-gray with optional aurora layers
- Lighting: cool key light, blue ambient fill, sharp spec highlights
- Post FX: mild blue grade and frost-edge vignette
- Particles: snow drift, fine crystal spray, breath-like mist near vents

## Toxic / Industrial
- Ground: rusted steel platforms + fractured concrete lanes
- Accents: neon toxic sludge and yellow-black hazard striping
- Sky: polluted yellow-green haze, particulate smog
- Lighting: flicker-prone industrial fixtures and green practicals
- Post FX: slight green grade, controlled chromatic aberration in toxic pockets
- Particles: steam bursts, smoke, drips, suspended debris

## 4. Integration Notes
- All hazard scripts route damage through `MetalPod.Shared.IDamageReceiver` (`TakeDamage(amount, DamageType)`), with no direct hazard references to `HovercraftHealth`.
- `CourseManager` implements `MetalPod.Shared.ICourseEvents` and raises `MetalPod.Shared.EventBus.RaiseCourseCompleted(...)` on successful finish.
