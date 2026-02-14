# Metal Pod

Hovercraft obstacle-course game prototype for iOS (Unity).

## Current Status

This repository now contains a Unity-ready folder layout and Phase 1 code scaffolding from `IMPLEMENTATION_PLAN.md`, including:

- Core managers: `GameManager`, `GameStateManager`, `SceneLoader`, `AudioManager`
- Hovercraft systems: input, hover physics, controller, health
- Course systems: checkpoint, timer, finish line, medal evaluation, course manager, collectibles
- Hazard foundations: base hazard + lava/debris/toxic/ice warning scaffolding
- UI foundations: HUD + menu/settings/result manager scripts
- Progression/workshop stubs: save, currency, upgrades, customization, selection
- Unity `.gitignore`

## Open In Unity

1. Create/open a Unity 2022 LTS project at this repo root.
2. Let Unity generate `.meta` files.
3. Install/enable required packages:
   - Input System
   - URP (if using URP pipeline)
4. Create ScriptableObject assets for:
   - `HovercraftStatsSO`
   - `CourseDataSO`
   - `UpgradeDataSO`
   - `HazardDataSO`
5. Wire scene references in inspector (player, timer, HUD, course manager).

## Notes

- Scripts are intentionally modular and data-driven for iterative tuning.
- Some systems are stubs intended for later phase expansion (workshop/progression/UI polish).
