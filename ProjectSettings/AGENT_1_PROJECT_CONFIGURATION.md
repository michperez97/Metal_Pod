# Agent 1 Project Configuration Contract

Apply these Unity settings before integrating Agent 2-5 systems.

## Tags

Create tags:

- `Player`
- `Checkpoint`
- `Hazard`
- `Collectible`
- `FinishLine`

## Layers

Create layers:

- `Hovercraft` (8)
- `Ground` (9)
- `Hazard` (10)
- `Collectible` (11)

## Physics Layer Collision Matrix

Recommended matrix (`Project Settings > Physics`):

- `Hovercraft x Ground`: Enabled
- `Hovercraft x Hazard`: Enabled
- `Hovercraft x Collectible`: Enabled (collectibles should use trigger colliders)
- `Hovercraft x Hovercraft`: Disabled (single-player scope)
- `Ground x Hazard`: Disabled
- `Ground x Collectible`: Disabled
- `Hazard x Hazard`: Disabled
- `Collectible x Collectible`: Disabled

## Quality Presets

Define 3 tiers: `Low`, `Medium`, `High`.

### Low
- Texture Quality: Half Res
- Shadow Distance: 25
- Shadow Cascades: 0
- Anti-Aliasing: Disabled
- VSync: Off

### Medium
- Texture Quality: Full Res
- Shadow Distance: 50
- Shadow Cascades: 2
- Anti-Aliasing: 2x MSAA
- VSync: Off

### High
- Texture Quality: Full Res
- Shadow Distance: 80
- Shadow Cascades: 4
- Anti-Aliasing: 4x MSAA
- VSync: Off

## Input System

In `Project Settings > Player > Active Input Handling`:

- Set to `Input System Package (New)`

Create an `Input Actions` asset with at least:

- `Steer` (Value/Vector2): accelerometer or tilt source
- `Throttle` (Value/Axis)
- `Brake` (Button)
- `Boost` (Button)
- `Pause` (Button)

For iOS:

- Enable accelerometer support
- Set target framerate from `GameConfigSO.targetFrameRate` (default `60`)
