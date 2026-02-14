# _Persistent Bootstrap Scene Setup (Agent 1)

This repository cannot safely author a full Unity scene asset from CLI because script `.meta` GUIDs are editor-generated. Create a scene named `_Persistent.unity` in `Assets/Scenes/` and apply this hierarchy exactly.

## Scene Hierarchy

- `__App` (root)
- `GameManagers` (child of `__App`)
- `AudioSources` (child of `GameManagers`)
- `LoadingOverlay` (child of `__App`, optional canvas)

## Components

Attach these to `GameManagers` in this order:

1. `GameManager`
2. `GameStateManager`
3. `SceneLoader`
4. `AudioManager`

## Inspector Wiring

### `GameManager`
- `Game Config`: assign a `GameConfigSO` asset
- `Game State Manager`: drag `GameManagers` component reference
- `Scene Loader`: drag `GameManagers` component reference
- `Audio Manager`: drag `GameManagers` component reference
- `Load Main Menu On Start`: enabled

### `AudioManager`
- `Music Source`: `AudioSources/MusicSource_A` (`AudioSource`)
- `Secondary Music Source`: `AudioSources/MusicSource_B` (`AudioSource`)
- `Ambient Source`: `AudioSources/AmbientSource` (`AudioSource`)
- `Sfx Source`: `AudioSources/SFXSource_0` (`AudioSource`)
- `SFX Pool Size`: `8`
- `Music Crossfade Seconds`: `1.0`

### `SceneLoader`
- Optional: bind loading overlay callback from UI bootstrap script via `BindLoadingScreen`.

## Runtime Boot Sequence

1. `_Persistent` scene is the first scene in Build Settings.
2. `GameManager` initializes `EventBus`, manager references, and config defaults.
3. `GameManager` auto-loads `MainMenu` scene (single mode) when active scene is `_Persistent`.
4. `GameManagers` object persists via `DontDestroyOnLoad`.

## Build Settings Requirement

Add these scenes to Build Settings in this order:

1. `_Persistent`
2. `MainMenu`
3. `Workshop`
4. Course scenes referenced by `CourseDataSO.sceneName`
