# Codex Task 1: Unity Auto-Setup Wizard

> **Goal**: Write a comprehensive Unity Editor script that auto-configures the entire project when opened in Unity for the first time. One click → playable game.

---

## Context

The Metal Pod project has ~75 C# scripts fully implemented but ZERO Unity scenes, prefabs, or .asset files. When the developer opens this in Unity 2022 LTS, they need a single menu item that builds everything automatically.

**Read these files first** to understand the full architecture:
- `IMPLEMENTATION_PLAN.md` — overall project plan
- `AGENT_1_CORE_INFRASTRUCTURE.md` — shared contracts and managers
- `AGENT_2_HOVERCRAFT_VEHICLE.md` — vehicle prefab hierarchy
- `Assets/Scripts/Shared/GameConstants.cs` — tags, layers
- `Assets/ScriptableObjects/HovercraftStatsSO.cs` — all SO definitions
- `Assets/Scripts/Progression/Editor/Agent4DataAssetGenerator.cs` — existing partial asset generator

---

## File to Create

```
Assets/Scripts/Editor/MetalPodSetupWizard.cs
```

This is a Unity Editor script (must be inside an `Editor` folder). It should NOT touch any files owned by other systems — it only creates Unity-specific assets (scenes, prefabs, project settings, .asset files).

---

## Requirements

### 1. Menu Entry
```csharp
[MenuItem("Metal Pod/Full Project Setup")]
public static void RunFullSetup()
```

Also provide individual menu items for each subsection:
```csharp
[MenuItem("Metal Pod/Setup/1. Configure Project Settings")]
[MenuItem("Metal Pod/Setup/2. Generate ScriptableObject Assets")]
[MenuItem("Metal Pod/Setup/3. Create Hovercraft Prefab")]
[MenuItem("Metal Pod/Setup/4. Create Persistent Scene")]
[MenuItem("Metal Pod/Setup/5. Create Main Menu Scene")]
[MenuItem("Metal Pod/Setup/6. Create Workshop Scene")]
[MenuItem("Metal Pod/Setup/7. Create Test Course Scene")]
[MenuItem("Metal Pod/Setup/8. Create All Course Scenes")]
```

### 2. Configure Project Settings

```csharp
private static void ConfigureProjectSettings()
{
    // Tags
    AddTag("Player");
    AddTag("Checkpoint");
    AddTag("Hazard");
    AddTag("Collectible");
    AddTag("FinishLine");

    // Layers
    SetLayer(8, "Hovercraft");
    SetLayer(9, "Ground");
    SetLayer(10, "Hazard");
    SetLayer(11, "Collectible");

    // Physics collision matrix:
    // Hovercraft collides with: Ground, Hazard, Collectible
    // Hazard does NOT collide with: Collectible
    // Ground collides with: everything
    Physics.IgnoreLayerCollision(10, 11, true); // Hazard vs Collectible

    // Target frame rate
    // Application.targetFrameRate handled at runtime by GameManager

    // Player Settings
    PlayerSettings.companyName = "Crocobyte";
    PlayerSettings.productName = "Metal Pod";
    PlayerSettings.bundleVersion = "0.1.0";
    // iOS settings
    PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
    PlayerSettings.iOS.targetOSVersionString = "15.0";

    // Quality Settings - 3 levels
    // Low: no shadows, no post-processing, reduced particles
    // Medium: soft shadows, basic post-processing
    // High: full shadows, full post-processing, max particles
}
```

Use `SerializedObject` on `TagManager` asset for tags/layers:
```csharp
private static void AddTag(string tag)
{
    var tagManager = new SerializedObject(
        AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
    var tags = tagManager.FindProperty("tags");

    // Check if tag exists
    for (int i = 0; i < tags.arraySize; i++)
    {
        if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
    }

    tags.InsertArrayElementAtIndex(tags.arraySize);
    tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
    tagManager.ApplyModifiedProperties();
}

private static void SetLayer(int index, string name)
{
    var tagManager = new SerializedObject(
        AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
    var layers = tagManager.FindProperty("layers");
    layers.GetArrayElementAtIndex(index).stringValue = name;
    tagManager.ApplyModifiedProperties();
}
```

### 3. Generate ALL ScriptableObject .asset Files

Create every .asset file the game needs with correct values. Use `ScriptableObject.CreateInstance<T>()` + `AssetDatabase.CreateAsset()`.

**DefaultHovercraftStats.asset** (at `Assets/ScriptableObjects/Data/DefaultHovercraftStats.asset`):
```csharp
var stats = ScriptableObject.CreateInstance<HovercraftStatsSO>();
stats.baseSpeed = 20f;
stats.maxSpeed = 40f;
stats.boostMultiplier = 1.5f;
stats.boostDuration = 3f;
stats.boostCooldown = 5f;
stats.brakeForce = 15f;
stats.turnSpeed = 3f;
stats.hoverHeight = 2f;
stats.hoverForce = 65f;
stats.hoverDamping = 5f;
stats.raycastCount = 4;
stats.maxHealth = 100f;
stats.maxShield = 50f;
stats.shieldRegenRate = 5f;
stats.shieldRegenDelay = 3f;
stats.driftFactor = 0.95f;
stats.tiltSensitivity = 1f;
stats.stabilizationForce = 10f;
AssetDatabase.CreateAsset(stats, "Assets/ScriptableObjects/Data/DefaultHovercraftStats.asset");
```

**GameConfig.asset** (at `Assets/ScriptableObjects/Data/GameConfig.asset`):
```csharp
var config = ScriptableObject.CreateInstance<GameConfigSO>();
config.startingCurrency = 0;
config.firstCourseId = "lava_01";
config.respawnDelay = 2f;
config.respawnInvincibilityDuration = 3f;
config.countdownSeconds = 3;
config.targetFrameRate = 60;
config.maxParticleCount = 500;
config.defaultTiltSensitivity = 1f;
config.minTiltSensitivity = 0.5f;
config.maxTiltSensitivity = 2f;
config.tiltDeadzone = 0.05f;
AssetDatabase.CreateAsset(config, "Assets/ScriptableObjects/Data/GameConfig.asset");
```

**9 Course Assets** (at `Assets/ScriptableObjects/Data/Courses/`):
```
lava_01: "Inferno Gate", Lava, index=0, gold=50, silver=65, bronze=80, medals=0, prereq=null, Easy
lava_02: "Magma Run", Lava, index=1, gold=70, silver=90, bronze=110, medals=0, prereq=lava_01, Medium
lava_03: "Eruption", Lava, index=2, gold=90, silver=120, bronze=150, medals=3, prereq=lava_02, Hard
ice_01: "Frozen Lake", Ice, index=0, gold=60, silver=80, bronze=100, medals=5, prereq=lava_03, Medium
ice_02: "Crystal Caverns", Ice, index=1, gold=80, silver=105, bronze=130, medals=7, prereq=ice_01, Hard
ice_03: "Avalanche Pass", Ice, index=2, gold=100, silver=130, bronze=160, medals=9, prereq=ice_02, Extreme
toxic_01: "Waste Disposal", Toxic, index=0, gold=65, silver=85, bronze=105, medals=12, prereq=ice_03, Medium
toxic_02: "The Foundry", Toxic, index=1, gold=85, silver=110, bronze=140, medals=15, prereq=toxic_01, Hard
toxic_03: "Meltdown", Toxic, index=2, gold=110, silver=145, bronze=180, medals=18, prereq=toxic_02, Extreme
```

Each course needs `sceneName` set to e.g. `"Lava_Course_01"`.
Each course's `prerequisiteCourse` must reference the actual CourseDataSO asset. Create them in order and wire prerequisites after all are created.

**4 Upgrade Assets** (at `Assets/ScriptableObjects/Data/Upgrades/`):
```
Upgrade_Speed: id="speed", name="Thruster Output", category=Speed
  levels: [{cost:100,mult:1.1},{cost:250,mult:1.2},{cost:500,mult:1.3},{cost:1000,mult:1.4},{cost:2000,mult:1.5}]
  statModifiers: [{statName:"maxSpeed", valuePerLevel:4f}]

Upgrade_Handling: id="handling", name="Gyro Stabilizers", category=Handling
  levels: [{cost:100,mult:1.08},{cost:250,mult:1.16},{cost:500,mult:1.24},{cost:1000,mult:1.32},{cost:2000,mult:1.4}]
  statModifiers: [{statName:"turnSpeed", valuePerLevel:0.24f}]

Upgrade_Shield: id="shield", name="Energy Barrier", category=Shield
  levels: [{cost:150,mult:1.12},{cost:350,mult:1.24},{cost:600,mult:1.36},{cost:1200,mult:1.48},{cost:2500,mult:1.6}]
  statModifiers: [{statName:"maxShield", valuePerLevel:6f}]

Upgrade_Boost: id="boost", name="Nitro Injector", category=Boost
  levels: [{cost:100,mult:1.1},{cost:250,mult:1.2},{cost:500,mult:1.3},{cost:1000,mult:1.4},{cost:2000,mult:1.5}]
  statModifiers: [{statName:"boostMultiplier", valuePerLevel:0.1f}]
```

**3 Environment Assets** (at `Assets/ScriptableObjects/Data/Environments/`):
```
LavaEnvironment: id="lava", name="Volcanic Wasteland", type=Lava, requiredMedals=0
  primaryColor=#FF4400, secondaryColor=#1A0000

IceEnvironment: id="ice", name="Frozen Abyss", type=Ice, requiredMedals=5
  primaryColor=#0088FF, secondaryColor=#001133

ToxicEnvironment: id="toxic", name="Industrial Wasteland", type=Toxic, requiredMedals=12
  primaryColor=#44FF00, secondaryColor=#0A1A00
```

Wire each environment's `courses` array to its 3 course assets.

**10 Cosmetic Assets** (at `Assets/ScriptableObjects/Data/Cosmetics/`):
```
Color_Default: id="default", name="Hazard Yellow", type=ColorScheme, cost=0
  primary=#D4A017, secondary=#2A2A2A, accent=#FF8800

Color_RedBlack: id="red_black", name="Hellfire", type=ColorScheme, cost=200
  primary=#CC0000, secondary=#1A1A1A, accent=#FF4400

Color_BlueSilver: id="blue_silver", name="Frost Runner", type=ColorScheme, cost=200
  primary=#2244AA, secondary=#C0C0C0, accent=#00AAFF

Color_GreenBlack: id="green_black", name="Toxic Avenger", type=ColorScheme, cost=200
  primary=#228B22, secondary=#1A1A1A, accent=#44FF00

Color_Chrome: id="chrome", name="Chrome Crusher", type=ColorScheme, cost=500
  primary=#E0E0E0, secondary=#808080, accent=#FFFFFF

Decal_Skull: id="decal_skull", name="Death's Head", type=Decal, cost=100
Decal_Flames: id="decal_flames", name="Hellfire", type=Decal, cost=100
Decal_Lightning: id="decal_lightning", name="Thunder Strike", type=Decal, cost=150
Decal_RacingStripes: id="decal_stripes", name="Speed Demon", type=Decal, cost=75
Decal_Number73: id="decal_73", name="Lucky 73", type=Decal, cost=0
```

### 4. Create Hovercraft Prefab Programmatically

```csharp
private static GameObject CreateHovercraftPrefab()
{
    var root = new GameObject("Hovercraft");
    root.tag = "Player";
    root.layer = LayerMask.NameToLayer("Hovercraft");

    // Rigidbody
    var rb = root.AddComponent<Rigidbody>();
    rb.mass = 10f;
    rb.linearDamping = 1f;
    rb.angularDamping = 3f;
    rb.useGravity = true;
    rb.interpolation = RigidbodyInterpolation.Interpolate;
    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

    // BoxCollider (hull approximation)
    var col = root.AddComponent<BoxCollider>();
    col.size = new Vector3(2f, 1f, 4f);
    col.center = new Vector3(0f, 0.5f, 0f);

    // Core scripts
    root.AddComponent<HovercraftController>();
    root.AddComponent<HovercraftStats>();

    // Model placeholder (cube until real model is imported)
    var model = GameObject.CreatePrimitive(PrimitiveType.Cube);
    model.name = "Model";
    model.transform.SetParent(root.transform);
    model.transform.localPosition = new Vector3(0, 0.5f, 0);
    model.transform.localScale = new Vector3(2f, 0.8f, 4f);
    Object.DestroyImmediate(model.GetComponent<BoxCollider>()); // remove duplicate collider

    // Physics child with hover points
    var physics = new GameObject("Physics");
    physics.transform.SetParent(root.transform);
    physics.AddComponent<HovercraftPhysics>();

    var hoverPoints = new string[] { "HoverPoint_FL", "HoverPoint_FR", "HoverPoint_BL", "HoverPoint_BR" };
    var hoverPositions = new Vector3[] {
        new Vector3(-0.8f, 0f, 1.5f),   // front-left
        new Vector3(0.8f, 0f, 1.5f),    // front-right
        new Vector3(-0.8f, 0f, -1.5f),  // back-left
        new Vector3(0.8f, 0f, -1.5f)    // back-right
    };
    for (int i = 0; i < 4; i++)
    {
        var hp = new GameObject(hoverPoints[i]);
        hp.transform.SetParent(physics.transform);
        hp.transform.localPosition = hoverPositions[i];
    }

    // Input child
    var input = new GameObject("Input");
    input.transform.SetParent(root.transform);
    input.AddComponent<HovercraftInput>();

    // Health child
    var health = new GameObject("Health");
    health.transform.SetParent(root.transform);
    health.AddComponent<HovercraftHealth>();

    // Visuals child
    var visuals = new GameObject("Visuals");
    visuals.transform.SetParent(root.transform);
    visuals.AddComponent<HovercraftVisuals>();
    // Add placeholder particle systems for thrusters
    CreateParticleChild(visuals.transform, "Thruster_Main_L", new Vector3(-0.6f, 0f, -1.5f));
    CreateParticleChild(visuals.transform, "Thruster_Main_R", new Vector3(0.6f, 0f, -1.5f));
    CreateParticleChild(visuals.transform, "Boost_Effect", new Vector3(0f, 0f, -2f));
    CreateParticleChild(visuals.transform, "Sparks", Vector3.zero);
    CreateParticleChild(visuals.transform, "Smoke", new Vector3(0f, 0.5f, 0f));

    // Audio child
    var audio = new GameObject("Audio");
    audio.transform.SetParent(root.transform);
    audio.AddComponent<HovercraftAudio>();
    var engineSrc = audio.AddComponent<AudioSource>();
    engineSrc.loop = true;
    engineSrc.playOnAwake = true;
    engineSrc.spatialBlend = 0f; // 2D for player vehicle

    // Customization child
    var customization = new GameObject("Customization");
    customization.transform.SetParent(root.transform);
    customization.AddComponent<HovercraftCustomization>();

    // Save as prefab
    string prefabPath = "Assets/Prefabs/Hovercraft/Hovercraft.prefab";
    EnsureDirectoryExists(prefabPath);
    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
    Object.DestroyImmediate(root);

    return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
}
```

### 5. Create _Persistent Scene

Bootstrap scene with DontDestroyOnLoad managers:
```csharp
private static void CreatePersistentScene()
{
    var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

    // GameManager object
    var gm = new GameObject("GameManager");
    gm.AddComponent<GameManager>();
    gm.AddComponent<GameStateManager>();
    gm.AddComponent<SceneLoader>();
    gm.AddComponent<AudioManager>();
    // Wire GameConfigSO reference if possible

    // ProgressionManager
    var pm = new GameObject("ProgressionManager");
    pm.AddComponent<ProgressionManager>();

    EditorSceneManager.SaveScene(scene, "Assets/Scenes/_Persistent.unity");
}
```

### 6. Create Main Menu Scene

```csharp
private static void CreateMainMenuScene()
{
    var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

    // Canvas
    var canvas = CreateCanvas("MainMenuCanvas");

    // Background (dark panel)
    var bg = CreateUIImage(canvas.transform, "Background", Color.black,
        Vector2.zero, new Vector2(1920, 1080));

    // Title text
    var title = CreateUIText(canvas.transform, "Title", "METAL POD",
        72, new Color(1f, 0.53f, 0f), new Vector2(0, 200));

    // Subtitle
    CreateUIText(canvas.transform, "Subtitle", "A Crocobyte Game",
        24, Color.gray, new Vector2(0, 140));

    // Buttons
    var btnGroup = CreateVerticalGroup(canvas.transform, "ButtonGroup",
        new Vector2(0, -50), spacing: 20);
    CreateButton(btnGroup.transform, "ContinueButton", "CONTINUE");
    CreateButton(btnGroup.transform, "NewGameButton", "NEW GAME");
    CreateButton(btnGroup.transform, "SettingsButton", "SETTINGS");

    // Attach MainMenuUI script
    var menuObj = new GameObject("MainMenuUI");
    menuObj.AddComponent<MainMenuUI>();

    // Event System
    CreateEventSystem();

    EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
}
```

### 7. Create Workshop Scene

Similar to Main Menu but with:
- 3D environment: floor plane, walls (cubes), hovercraft display platform
- Hovercraft prefab instance on platform
- Camera positioned for workshop view
- Canvas with top bar (currency, medals), bottom nav (3 buttons), side panels
- WorkshopManager component

### 8. Create Test Course Scene

```csharp
private static void CreateTestCourseScene()
{
    var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

    // Ground plane
    var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
    ground.name = "Ground";
    ground.transform.localScale = new Vector3(10, 1, 50); // 100m x 500m
    ground.layer = LayerMask.NameToLayer("Ground");

    // Walls (box colliders along edges)
    CreateWall("Wall_Left", new Vector3(-50, 2.5f, 0), new Vector3(1, 5, 500));
    CreateWall("Wall_Right", new Vector3(50, 2.5f, 0), new Vector3(1, 5, 500));

    // Ramp
    CreateRamp("Ramp_01", new Vector3(0, 0, 50), new Vector3(10, 0.1f, 20), 15f);

    // Narrow corridor walls
    CreateWall("Narrow_L", new Vector3(-3, 2.5f, 120), new Vector3(1, 5, 30));
    CreateWall("Narrow_R", new Vector3(3, 2.5f, 120), new Vector3(1, 5, 30));

    // Start line
    var start = new GameObject("StartLine");
    start.transform.position = new Vector3(0, 1, 0);
    var startCollider = start.AddComponent<BoxCollider>();
    startCollider.size = new Vector3(15, 3, 1);
    startCollider.isTrigger = true;

    // Checkpoints (3)
    CreateCheckpoint("Checkpoint_1", new Vector3(0, 1, 60));
    CreateCheckpoint("Checkpoint_2", new Vector3(0, 1, 140));
    CreateCheckpoint("Checkpoint_3", new Vector3(0, 1, 220));

    // Finish line
    var finish = new GameObject("FinishLine");
    finish.transform.position = new Vector3(0, 1, 280);
    finish.tag = "FinishLine";
    var finishCol = finish.AddComponent<BoxCollider>();
    finishCol.size = new Vector3(15, 3, 1);
    finishCol.isTrigger = true;
    finish.AddComponent<FinishLine>();

    // Course manager
    var courseObj = new GameObject("CourseManager");
    courseObj.AddComponent<CourseManager>();
    courseObj.AddComponent<CourseTimer>();

    // Hovercraft spawn
    var hovercraft = PrefabUtility.InstantiatePrefab(
        AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Hovercraft/Hovercraft.prefab")
    ) as GameObject;
    if (hovercraft != null)
        hovercraft.transform.position = new Vector3(0, 2, -5);

    // Collectibles (5)
    CreateCollectible("Collectible_01", new Vector3(3, 1.5f, 30));
    CreateCollectible("Collectible_02", new Vector3(-3, 1.5f, 80));
    CreateCollectible("Collectible_03", new Vector3(0, 3f, 55));  // on ramp
    CreateCollectible("Collectible_04", new Vector3(2, 1.5f, 130)); // in narrow
    CreateCollectible("Collectible_05", new Vector3(-5, 1.5f, 200));

    // HUD Canvas
    var hudCanvas = CreateCanvas("HUDCanvas");
    hudCanvas.AddComponent<HUD>();

    // Directional light + skybox
    // (DefaultGameObjects already includes camera + light)

    EditorSceneManager.SaveScene(scene, "Assets/Scenes/TestCourse.unity");
}
```

### 9. Create All 9 Course Scene Stubs

For each course, create a basic scene with:
- Ground plane with environment-appropriate color
- StartLine, FinishLine, CourseManager, Checkpoints (count per course spec)
- Hovercraft spawn point
- HUD canvas
- Appropriate number of empty hazard spawn points as markers
- Scene named correctly: `Lava_Course_01.unity`, etc.

These are stub scenes — geometry will be refined in Unity, but the structure is correct.

### 10. Build Scenes List

After creating all scenes, add them to `EditorBuildSettings.scenes`:
```csharp
private static void UpdateBuildScenes()
{
    var scenes = new EditorBuildSettingsScene[]
    {
        new EditorBuildSettingsScene("Assets/Scenes/_Persistent.unity", true),
        new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
        new EditorBuildSettingsScene("Assets/Scenes/Workshop.unity", true),
        new EditorBuildSettingsScene("Assets/Scenes/TestCourse.unity", true),
        new EditorBuildSettingsScene("Assets/Scenes/Lava_Course_01.unity", true),
        new EditorBuildSettingsScene("Assets/Scenes/Lava_Course_02.unity", true),
        new EditorBuildSettingsScene("Assets/Scenes/Lava_Course_03.unity", true),
        new EditorBuildSettingsScene("Assets/Scenes/Ice_Course_01.unity", true),
        new EditorBuildSettingsScene("Assets/Scenes/Ice_Course_02.unity", true),
        new EditorBuildSettingsScene("Assets/Scenes/Ice_Course_03.unity", true),
        new EditorBuildSettingsScene("Assets/Scenes/Toxic_Course_01.unity", true),
        new EditorBuildSettingsScene("Assets/Scenes/Toxic_Course_02.unity", true),
        new EditorBuildSettingsScene("Assets/Scenes/Toxic_Course_03.unity", true),
    };
    EditorBuildSettings.scenes = scenes;
}
```

---

## Helper Methods Needed

```csharp
private static void EnsureDirectoryExists(string assetPath) { /* create folder if missing */ }
private static GameObject CreateCanvas(string name) { /* Canvas + CanvasScaler + GraphicRaycaster */ }
private static GameObject CreateUIText(Transform parent, string name, string text, int size, Color color, Vector2 pos) { }
private static GameObject CreateUIImage(Transform parent, string name, Color color, Vector2 pos, Vector2 size) { }
private static GameObject CreateButton(Transform parent, string name, string label) { }
private static GameObject CreateVerticalGroup(Transform parent, string name, Vector2 pos, float spacing) { }
private static void CreateEventSystem() { }
private static void CreateWall(string name, Vector3 pos, Vector3 scale) { }
private static void CreateRamp(string name, Vector3 pos, Vector3 scale, float angle) { }
private static void CreateCheckpoint(string name, Vector3 pos) { }
private static void CreateCollectible(string name, Vector3 pos) { }
private static void CreateParticleChild(Transform parent, string name, Vector3 localPos) { }
```

---

## Important Notes

- ALL ScriptableObject classes are in `Assets/ScriptableObjects/` — read them to understand the exact fields
- Use `AssetDatabase.CreateAsset()` for .asset files, `PrefabUtility.SaveAsPrefabAsset()` for prefabs
- Use `EditorSceneManager` for scene creation
- Call `AssetDatabase.SaveAssets()` and `AssetDatabase.Refresh()` at the end
- Add progress bar via `EditorUtility.DisplayProgressBar()` during setup
- Wrap everything in `try/finally` with `EditorUtility.ClearProgressBar()` in finally
- Check for existing assets before creating (don't overwrite if already exists)
- Use `#if UNITY_EDITOR` or keep in Editor folder so it doesn't compile in builds
- The existing `Agent4DataAssetGenerator.cs` in `Assets/Scripts/Progression/Editor/` may partially overlap — review it first and either integrate or replace

---

## Acceptance Criteria

- [ ] Single menu item "Metal Pod/Full Project Setup" runs everything
- [ ] Individual menu items for each step
- [ ] All tags and layers configured correctly
- [ ] All ~39 .asset files generated with correct values
- [ ] Course asset prerequisites wired correctly
- [ ] Hovercraft prefab created with full hierarchy (model, physics, hover points, input, health, visuals, audio, customization)
- [ ] _Persistent scene with all managers
- [ ] MainMenu scene with canvas + buttons
- [ ] Workshop scene with 3D environment + UI
- [ ] TestCourse scene with geometry, checkpoints, collectibles, hovercraft
- [ ] 9 course scene stubs created
- [ ] Build settings list populated
- [ ] Progress bar during setup
- [ ] Idempotent (safe to run multiple times)
