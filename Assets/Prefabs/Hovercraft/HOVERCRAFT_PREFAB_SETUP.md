# Hovercraft Prefab Setup (Agent 2)

## Prefab Hierarchy

```text
Hovercraft (Root GameObject)
├── Components:
│   Rigidbody
│   BoxCollider
│   HovercraftController
│   HovercraftStats
│   HovercraftCustomization
│
├── Model (child)
│   ├── MeshFilter
│   ├── MeshRenderer
│   ├── Body_Renderer
│   └── Accent_Renderer
│
├── Physics (child)
│   ├── HovercraftPhysics
│   ├── HoverPoint_FL
│   ├── HoverPoint_FR
│   ├── HoverPoint_BL
│   └── HoverPoint_BR
│
├── Input (child)
│   └── HovercraftInput
│
├── Health (child)
│   └── HovercraftHealth
│
├── Visuals (child)
│   ├── HovercraftVisuals
│   ├── Thruster_Main_L
│   ├── Thruster_Main_R
│   ├── Thruster_Side_L
│   ├── Thruster_Side_R
│   ├── Thruster_Rear
│   ├── Boost_Effect
│   ├── Sparks
│   ├── Smoke
│   └── Fire
│
├── Audio (child)
│   ├── HovercraftAudio
│   ├── EngineSource (AudioSource, loop)
│   └── SFXSource (AudioSource, one-shot)
│
├── Decal (child)
│   └── DecalProjector or quad
│
└── AttachPoints (child)
    ├── Attach_Spoiler
    ├── Attach_Antenna
    └── Attach_Exhaust
```

## Rigidbody + Collider Baseline

- `Mass`: `10`
- `Drag`: `1`
- `Angular Drag`: `3`
- `Use Gravity`: `true`
- `Is Kinematic`: `false`
- `Interpolation`: `Interpolate`
- `Collision Detection`: `Continuous`
- `BoxCollider`: fit to hull footprint, with hover points near corners

## Visual Direction Defaults

- Primary body color: industrial yellow (`#EDBF2A`)
- Secondary color: near-black steel (`#1F1F1F`)
- Accent/thruster color: hot orange (`#FF8C1A`)
- Default feel: heavy, worn, chunky metal

## Particle Specs

### Main Thrusters (Blue Glow)

- Shape: cone (downward), `15` degrees
- Start Color: cyan (`#00DDFF`) to blue (`#0044FF`) over lifetime
- Start Size: `0.3 -> 0.1`
- Emission: `50-200` (scaled by speed)
- Lifetime: `0.3`
- Material: additive

### Side Thrusters (Orange Flame)

- Shape: cone (outward), `10` degrees
- Start Color: orange (`#FF8800`) to red (`#FF2200`)
- Start Size: `0.2 -> 0.05`
- Emission: `30-100` (scaled by steering)
- Lifetime: `0.2`

### Boost Effect

- Shape: cone (rear), `25` degrees
- Start Color: white to orange to red
- Start Size: `0.5 -> 0.2`
- Emission: `300` while boosting
- Speed: `10-20`
- Noise: enabled for turbulence

### Damage Sparks

- Shape: sphere
- Start Color: yellow/orange
- Start Size: `0.05-0.1`
- Burst: `20-50`
- Gravity modifier: `0.5`
- Lifetime: `0.5`

### Smoke (Damaged)

- Shape: cone (up), `30` degrees
- Start Color: dark gray to transparent
- Start Size: `0.3 -> 1.0`
- Emission: `10-30`
- Lifetime: `1.5`
- Speed: `0.5-1.0`

### Fire (Critical)

- Trigger threshold: health below `25%`
- Color: orange/red with short lifetime flicker
- Pair with smoke for critical damage readability
