# Codex Task 3: Procedural Course Builder

> **Goal**: Write Editor scripts that programmatically generate all 10 course levels (1 test + 9 game courses) with geometry, hazard placement, checkpoints, and collectibles. When run in Unity, instant playable levels.

---

## Context

Metal Pod has 9 game courses across 3 environments plus 1 greybox test course. All course layouts are defined in `Docs/AGENT_3_DELIVERABLES.md` and `AGENT_3_COURSES_AND_HAZARDS.md`. Instead of building these by hand in Unity, we generate them programmatically.

**Read these files first**:
- `AGENT_3_COURSES_AND_HAZARDS.md` — All 10 course layouts with hazard details
- `Docs/AGENT_3_DELIVERABLES.md` — Environment art direction
- `Assets/Scripts/Hazards/` — All hazard scripts (to know what components to add)
- `Assets/Scripts/Course/` — Checkpoint, FinishLine, Collectible, CourseManager
- `Assets/Scripts/Shared/GameConstants.cs` — Tags, layers

---

## Files to Create

```
Assets/Scripts/Editor/CourseBuilder/
├── CourseBuilder.cs                    # Main builder with menu items
├── CourseDefinition.cs                 # Data class defining a course layout
├── CourseSegment.cs                    # Individual course segment (straight, turn, ramp, etc.)
├── HazardPlacer.cs                    # Places hazards based on course definition
├── EnvironmentDecorator.cs            # Adds environment-specific decorations
├── TestCourseDefinition.cs            # Greybox test course data
├── LavaCourseDefinitions.cs           # Lava 1-3 layouts
├── IceCourseDefinitions.cs            # Ice 1-3 layouts
└── ToxicCourseDefinitions.cs          # Toxic 1-3 layouts
```

---

## Architecture

### CourseDefinition.cs — Data Structure

```csharp
public enum SegmentType
{
    Straight,
    TurnLeft,
    TurnRight,
    SCurve,
    Ramp,
    Gap,              // missing ground (must jump)
    NarrowCorridor,
    WideOpen,
    Bridge,           // narrow path over hazard
    Tunnel
}

public enum HazardPlacement
{
    Left, Center, Right, Random, BothSides, Overhead, Behind
}

[System.Serializable]
public class CourseSegmentData
{
    public SegmentType type;
    public float length = 30f;         // meters
    public float width = 15f;          // meters (default track width)
    public float elevation = 0f;       // height change over segment
    public bool hasCheckpoint;
    public int checkpointIndex;

    // Hazards in this segment
    public HazardEntry[] hazards;

    // Collectibles in this segment
    public CollectibleEntry[] collectibles;

    // Walls
    public bool hasLeftWall = true;
    public bool hasRightWall = true;
    public float wallHeight = 5f;
}

[System.Serializable]
public class HazardEntry
{
    public string hazardType;           // e.g. "LavaFlow", "FallingIcicle"
    public HazardPlacement placement;
    public Vector3 localOffset;         // fine-tune position within segment
    public float scale = 1f;
    public float customParam1;          // hazard-specific (e.g., interval for timed hazards)
}

[System.Serializable]
public class CollectibleEntry
{
    public Vector3 localOffset;         // position within segment
    public CollectibleType type;        // Currency, Health, Shield
}

[System.Serializable]
public class CourseDefinitionData
{
    public string courseName;
    public string sceneName;
    public EnvironmentType environmentType;
    public float trackWidth = 15f;      // default width
    public CourseSegmentData[] segments;
    public Vector3 startPosition = Vector3.zero;
    public float startRotation = 0f;
}
```

### CourseBuilder.cs — Main Generator

```csharp
[MenuItem("Metal Pod/Courses/Build All Courses")]
public static void BuildAllCourses()
{
    BuildTestCourse();
    BuildLavaCourses();
    BuildIceCourses();
    BuildToxicCourses();
}

[MenuItem("Metal Pod/Courses/Build Test Course")]
public static void BuildTestCourse() { BuildCourse(TestCourseDefinition.Get()); }

[MenuItem("Metal Pod/Courses/Build Lava Courses")]
public static void BuildLavaCourses()
{
    BuildCourse(LavaCourseDefinitions.GetCourse1());
    BuildCourse(LavaCourseDefinitions.GetCourse2());
    BuildCourse(LavaCourseDefinitions.GetCourse3());
}
// ... etc

private static void BuildCourse(CourseDefinitionData def)
{
    var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);

    // 1. Generate track geometry
    Vector3 currentPos = def.startPosition;
    float currentAngle = def.startRotation;
    var allSegments = new List<GameObject>();

    foreach (var segment in def.segments)
    {
        var segObj = GenerateSegment(segment, currentPos, currentAngle, def.environmentType);
        allSegments.Add(segObj);

        // Advance position based on segment type
        (currentPos, currentAngle) = AdvanceCursor(segment, currentPos, currentAngle);
    }

    // 2. Place start line
    PlaceStartLine(def.startPosition, def.startRotation);

    // 3. Place finish line at end
    PlaceFinishLine(currentPos, currentAngle);

    // 4. Place checkpoints
    PlaceCheckpoints(def, allSegments);

    // 5. Place hazards
    HazardPlacer.PlaceAllHazards(def, allSegments);

    // 6. Place collectibles
    PlaceCollectibles(def, allSegments);

    // 7. Add course manager
    var courseObj = new GameObject("CourseManager");
    courseObj.AddComponent<CourseManager>();
    courseObj.AddComponent<CourseTimer>();

    // 8. Place hovercraft at start
    PlaceHovercraft(def.startPosition + Vector3.up * 2);

    // 9. Add HUD
    AddHUD();

    // 10. Apply environment lighting/skybox
    EnvironmentDecorator.ApplyEnvironment(def.environmentType);

    // 11. Save scene
    EditorSceneManager.SaveScene(scene, $"Assets/Scenes/{def.sceneName}.unity");
}
```

### Segment Generation

```csharp
private static GameObject GenerateSegment(CourseSegmentData seg, Vector3 pos, float angle, EnvironmentType env)
{
    var parent = new GameObject($"Segment_{seg.type}");
    parent.transform.position = pos;
    parent.transform.rotation = Quaternion.Euler(0, angle, 0);

    switch (seg.type)
    {
        case SegmentType.Straight:
            // Flat ground plane + optional walls
            CreateGroundQuad(parent.transform, seg.length, seg.width, env);
            if (seg.hasLeftWall) CreateWall(parent.transform, true, seg.length, seg.wallHeight);
            if (seg.hasRightWall) CreateWall(parent.transform, false, seg.length, seg.wallHeight);
            break;

        case SegmentType.TurnLeft:
        case SegmentType.TurnRight:
            // Arc-shaped ground (approximate with 5-8 straight subsections)
            float turnAngle = (seg.type == SegmentType.TurnLeft) ? -90f : 90f;
            CreateTurnGeometry(parent.transform, seg.length, seg.width, turnAngle, env);
            break;

        case SegmentType.Ramp:
            // Angled surface going up/down
            CreateRampGeometry(parent.transform, seg.length, seg.width, seg.elevation, env);
            break;

        case SegmentType.Gap:
            // No ground - just walls showing the gap
            // Ground before and after but missing in the middle
            float gapLength = seg.length * 0.4f; // 40% is the gap
            CreateGroundQuad(parent.transform, seg.length * 0.3f, seg.width, env); // before
            // gap in middle
            var afterGap = CreateGroundQuad(parent.transform, seg.length * 0.3f, seg.width, env);
            afterGap.transform.localPosition = new Vector3(0, 0, seg.length * 0.7f);
            break;

        case SegmentType.NarrowCorridor:
            float narrowWidth = 6f;
            CreateGroundQuad(parent.transform, seg.length, narrowWidth, env);
            CreateWall(parent.transform, true, seg.length, seg.wallHeight, narrowWidth);
            CreateWall(parent.transform, false, seg.length, seg.wallHeight, narrowWidth);
            break;

        case SegmentType.Bridge:
            // Narrow path with hazard (lava/acid) on both sides
            float bridgeWidth = 4f;
            CreateGroundQuad(parent.transform, seg.length, bridgeWidth, env);
            // No walls - danger of falling off
            break;

        case SegmentType.WideOpen:
            CreateGroundQuad(parent.transform, seg.length, seg.width * 1.5f, env);
            // Wider area, no walls
            break;

        case SegmentType.SCurve:
            CreateSCurveGeometry(parent.transform, seg.length, seg.width, env);
            break;

        case SegmentType.Tunnel:
            CreateGroundQuad(parent.transform, seg.length, seg.width, env);
            CreateWall(parent.transform, true, seg.length, seg.wallHeight);
            CreateWall(parent.transform, false, seg.length, seg.wallHeight);
            CreateCeiling(parent.transform, seg.length, seg.width, seg.wallHeight); // adds roof
            break;
    }

    return parent;
}
```

### HazardPlacer.cs

```csharp
public static class HazardPlacer
{
    public static void PlaceAllHazards(CourseDefinitionData def, List<GameObject> segments)
    {
        for (int i = 0; i < def.segments.Length; i++)
        {
            var segData = def.segments[i];
            if (segData.hazards == null) continue;

            foreach (var hazard in segData.hazards)
            {
                PlaceHazard(hazard, segments[i].transform, segData);
            }
        }
    }

    private static void PlaceHazard(HazardEntry entry, Transform segmentRoot, CourseSegmentData seg)
    {
        var hazardObj = new GameObject($"Hazard_{entry.hazardType}");
        hazardObj.transform.SetParent(segmentRoot);
        hazardObj.tag = GameConstants.TAG_HAZARD;
        hazardObj.layer = LayerMask.NameToLayer(GameConstants.LAYER_HAZARD);

        // Position based on placement
        Vector3 pos = CalculateHazardPosition(entry.placement, seg.width, seg.length);
        pos += entry.localOffset;
        hazardObj.transform.localPosition = pos;

        // Add collider (trigger for damage zones)
        var col = hazardObj.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(entry.scale * 3f, entry.scale * 2f, entry.scale * 3f);

        // Add the hazard script component based on type
        AddHazardComponent(hazardObj, entry);
    }

    private static void AddHazardComponent(GameObject obj, HazardEntry entry)
    {
        switch (entry.hazardType)
        {
            case "LavaFlow":
                var lf = obj.AddComponent<MetalPod.Hazards.Lava.LavaFlow>();
                // Configure from entry.customParam1 if needed
                break;
            case "VolcanicEruption":
                obj.AddComponent<MetalPod.Hazards.Lava.VolcanicEruption>();
                break;
            case "LavaGeyser":
                obj.AddComponent<MetalPod.Hazards.Lava.LavaGeyser>();
                break;
            case "HeatZone":
                obj.AddComponent<MetalPod.Hazards.Lava.HeatZone>();
                break;
            case "FallingDebris":
                obj.AddComponent<MetalPod.Hazards.FallingDebris>();
                break;
            case "IcePatch":
                obj.AddComponent<MetalPod.Hazards.Ice.IcePatch>();
                break;
            case "FallingIcicle":
                obj.AddComponent<MetalPod.Hazards.Ice.FallingIcicle>();
                break;
            case "BlizzardZone":
                obj.AddComponent<MetalPod.Hazards.Ice.BlizzardZone>();
                break;
            case "IceWall":
                obj.AddComponent<MetalPod.Hazards.Ice.IceWall>();
                break;
            case "Avalanche":
                obj.AddComponent<MetalPod.Hazards.Ice.Avalanche>();
                break;
            case "ToxicGas":
                obj.AddComponent<MetalPod.Hazards.Toxic.ToxicGas>();
                break;
            case "AcidPool":
                obj.AddComponent<MetalPod.Hazards.Toxic.AcidPool>();
                break;
            case "IndustrialPress":
                obj.AddComponent<MetalPod.Hazards.Toxic.IndustrialPress>();
                break;
            case "ElectricFence":
                obj.AddComponent<MetalPod.Hazards.Toxic.ElectricFence>();
                break;
            case "BarrelExplosion":
                obj.AddComponent<MetalPod.Hazards.Toxic.BarrelExplosion>();
                break;
        }
    }
}
```

### EnvironmentDecorator.cs

```csharp
public static class EnvironmentDecorator
{
    public static void ApplyEnvironment(EnvironmentType type)
    {
        switch (type)
        {
            case EnvironmentType.Lava:
                ApplyLavaEnvironment();
                break;
            case EnvironmentType.Ice:
                ApplyIceEnvironment();
                break;
            case EnvironmentType.Toxic:
                ApplyToxicEnvironment();
                break;
        }
    }

    private static void ApplyLavaEnvironment()
    {
        // Set directional light color: warm orange
        var light = Object.FindFirstObjectByType<Light>();
        if (light != null)
        {
            light.color = new Color(1f, 0.6f, 0.3f);
            light.intensity = 1.2f;
        }

        // Set ambient color
        RenderSettings.ambientLight = new Color(0.3f, 0.1f, 0.05f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.2f, 0.08f, 0.02f);
        RenderSettings.fogDensity = 0.01f;

        // Ground material color: dark volcanic rock
        // Applied via material on ground objects
    }

    private static void ApplyIceEnvironment()
    {
        var light = Object.FindFirstObjectByType<Light>();
        if (light != null)
        {
            light.color = new Color(0.7f, 0.8f, 1f);
            light.intensity = 0.8f;
        }

        RenderSettings.ambientLight = new Color(0.15f, 0.2f, 0.3f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.6f, 0.7f, 0.8f);
        RenderSettings.fogDensity = 0.015f;
    }

    private static void ApplyToxicEnvironment()
    {
        var light = Object.FindFirstObjectByType<Light>();
        if (light != null)
        {
            light.color = new Color(0.8f, 0.9f, 0.7f);
            light.intensity = 0.6f; // dim, industrial
        }

        RenderSettings.ambientLight = new Color(0.1f, 0.15f, 0.05f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.15f, 0.2f, 0.05f);
        RenderSettings.fogDensity = 0.02f;
    }
}
```

---

## Course Layout Definitions

Implement these EXACT layouts. Reference `AGENT_3_COURSES_AND_HAZARDS.md` for full details.

### TestCourse (Greybox)
```
Segments:
1. Straight(50m, 15w)
2. TurnLeft(30m, 15w)           + checkpoint
3. Straight(40m, 15w) + Ramp
4. Gap(20m)                     + checkpoint
5. NarrowCorridor(30m, 6w)
6. SCurve(40m, 12w)             + checkpoint
7. Straight(40m, 15w) + walls

Hazards: NONE (greybox test only)
Collectibles: 5 (2 on path, 3 on edges/ramp)
```

### Lava Course 1 — "Inferno Gate"
```
Segments:
1. Straight(60m, 15w)                  — intro, no hazards
2. Straight(50m, 12w)                  — LavaFlow(left edge), LavaFlow(right edge) + checkpoint
3. TurnRight(40m, 12w)                 — static lava pool on inside of turn
4. Straight(60m, 15w)                  — FallingDebris(2 spots, spaced out) + checkpoint
5. WideOpen(50m, 20w)                  — lava pools scattered, clear path through middle
6. Straight(40m, 12w)                  — FallingDebris(1 spot) + checkpoint
7. Straight(50m, 15w)                  — clear run to finish

Collectibles: 5
```

### Lava Course 2 — "Magma Run"
```
Segments:
1. Straight(40m, 15w)                  — intro
2. Straight(60m, 12w)                  — 3 timed LavaFlows crossing path + checkpoint
3. TurnLeft(30m, 12w)                  — LavaGeyser on inside
4. Ramp(30m, up 5m)                    — ramp over lava river
5. Bridge(40m, 4w)                     — narrow bridge, lava below + checkpoint
6. Straight(50m, 12w)                  — LavaGeysers(4, staggered pattern)
7. SCurve(40m, 10w)                    — FallingDebris zones + checkpoint
8. Straight(30m, 12w)                  — LavaFlow(timed, fast) + checkpoint
9. Straight(40m, 15w)                  — final stretch

Collectibles: 8 (some require altitude control on ramps)
```

### Lava Course 3 — "Eruption"
```
Segments:
1. NarrowCorridor(40m, 8w)             — tight start, lava walls glowing
2. Straight(50m, 10w)                  — HeatZone + LavaGeysers + checkpoint
3. Bridge(30m, 4w)                     — bridge over lava lake, geysers below
4. TurnRight(30m, 10w)                 — VolcanicEruption trigger at midpoint
5. Straight(60m, 10w)                  — ERUPTION SEQUENCE: debris rain everywhere + checkpoint
6. NarrowCorridor(30m, 6w)             — tight escape from eruption area + checkpoint
7. SCurve(50m, 8w)                     — LavaFlows on both sides + FallingDebris
8. Bridge(20m, 3w)                     — extremely narrow over lava + checkpoint
9. Ramp(30m, up 8m)                    — jump over final lava river + checkpoint
10. Straight(30m, 15w)                 — finish

Collectibles: 10 (very risky placements - on bridges, in hazard zones)
```

### Ice Course 1 — "Frozen Lake"
```
Segments:
1. Straight(50m, 15w)                  — intro on solid ground
2. WideOpen(80m, 25w)                  — frozen lake with IcePatches(large areas) + checkpoint
3. Straight(40m, 12w)                  — FallingIcicle zones(2)
4. TurnLeft(30m, 12w)                  — IcePatch on entire turn surface
5. Straight(50m, 12w)                  — gentle BlizzardZone + FallingIcicles + checkpoint
6. WideOpen(60m, 20w)                  — more IcePatches, collectibles on risky paths
7. Straight(40m, 15w)                  — clear finish + checkpoint

Collectibles: 6
```

### Ice Course 2 — "Crystal Caverns"
```
Segments:
1. Tunnel(40m, 10w)                    — cave entrance
2. NarrowCorridor(30m, 6w)             — IcePatches + FallingIcicles + checkpoint
3. WideOpen(40m, 15w)                  — ice chamber with IceWalls(2, blocking shortcuts)
4. Tunnel(50m, 8w)                     — BlizzardZone(strong wind) + checkpoint
5. SCurve(40m, 8w)                     — IcePatches on every curve
6. NarrowCorridor(30m, 6w)             — FallingIcicles(dense) + IceWall(1) + checkpoint
7. Straight(50m, 12w)                  — BlizzardZone + IcePatches + checkpoint
8. Straight(30m, 15w)                  — exit to finish

Collectibles: 8 (behind IceWalls = shortcut rewards)
```

### Ice Course 3 — "Avalanche Pass"
```
Segments:
1. Straight(40m, 15w)                  — mountain pass entrance, wind starting
2. WideOpen(60m, 20w)                  — full BlizzardZone + IcePatches + checkpoint
3. TurnRight(30m, 10w)                 — FallingIcicles + IcePatch on turn
4. NarrowCorridor(40m, 8w)             — IceWalls(3, must boost through) + checkpoint
5. Straight(50m, 12w)                  — relative calm before storm + checkpoint
6. Straight(80m, 15w)                  — AVALANCHE TRIGGER! Chase sequence begins + checkpoint
7. SCurve(50m, 10w)                    — Avalanche chasing, must maintain speed
8. NarrowCorridor(40m, 8w)             — Avalanche still coming, tight passage + checkpoint
9. Ramp(20m, up 5m)                    — jump to safety
10. Straight(30m, 15w)                 — avalanche stops, finish

Collectibles: 10
```

### Toxic Course 1 — "Waste Disposal"
```
Segments:
1. Straight(40m, 12w)                  — industrial entrance
2. Straight(50m, 12w)                  — ToxicGas vents(periodic, 3) + checkpoint
3. WideOpen(40m, 18w)                  — AcidPools(static, scattered)
4. TurnLeft(30m, 10w)                  — BarrelExplosions(4, along edge) + checkpoint
5. NarrowCorridor(40m, 8w)             — ToxicGas(constant cloud)
6. Straight(50m, 12w)                  — AcidPools + BarrelExplosions(chain) + checkpoint
7. Straight(30m, 15w)                  — finish

Collectibles: 6
```

### Toxic Course 2 — "The Foundry"
```
Segments:
1. Straight(40m, 12w)                  — factory entrance
2. Straight(40m, 10w)                  — IndustrialPress(3, timed) + checkpoint
3. NarrowCorridor(30m, 8w)             — ElectricFence(intermittent, 2)
4. Straight(50m, 12w)                  — AcidPool(rising level) + ToxicGas + checkpoint
5. TurnRight(30m, 10w)                 — IndustrialPress at turn + checkpoint
6. Bridge(30m, 5w)                     — over rising acid, ElectricFence(1)
7. Straight(50m, 10w)                  — ALL toxic hazards combined + checkpoint
8. Straight(30m, 15w)                  — finish

Collectibles: 8
```

### Toxic Course 3 — "Meltdown"
```
Segments:
1. NarrowCorridor(30m, 8w)             — warning lights, alarms
2. Straight(40m, 10w)                  — BarrelExplosion chain(6) + ToxicGas + checkpoint
3. Bridge(20m, 4w)                     — over acid, AcidPool(rising fast)
4. Straight(50m, 10w)                  — IndustrialPress(5, tight timing) + checkpoint
5. WideOpen(40m, 15w)                  — CHAIN EXPLOSION sequence: barrels everywhere + checkpoint
6. Tunnel(40m, 8w)                     — ElectricFence(3) + ToxicGas + checkpoint
7. SCurve(40m, 8w)                     — AcidPool(rising) on both sides
8. NarrowCorridor(30m, 6w)             — everything: press + fence + gas + checkpoint
9. Ramp(20m, up 5m)                    — escape jump
10. Straight(30m, 15w)                 — finish

Collectibles: 10
```

---

## Ground Material Colors (Applied Programmatically)

```csharp
private static Color GetGroundColor(EnvironmentType type)
{
    return type switch
    {
        EnvironmentType.Lava => new Color(0.1f, 0.08f, 0.06f),  // dark volcanic
        EnvironmentType.Ice => new Color(0.7f, 0.8f, 0.9f),     // ice blue-white
        EnvironmentType.Toxic => new Color(0.3f, 0.25f, 0.2f),  // rusted brown
        _ => new Color(0.5f, 0.5f, 0.5f)                         // greybox gray
    };
}
```

---

## Acceptance Criteria

- [ ] All 10 courses generate as Unity scenes via menu items
- [ ] Each course has: ground geometry, walls, checkpoints, finish line, start line, hovercraft spawn
- [ ] Hazards placed correctly per definitions (right type, right position, right segment)
- [ ] Collectibles placed per definitions
- [ ] Environment-specific lighting, fog, and ground colors applied
- [ ] CourseManager + CourseTimer components on each scene
- [ ] HUD canvas in each scene
- [ ] Turns and S-curves have proper geometry (not just straight pieces)
- [ ] Ramps have correct angles
- [ ] Gaps have missing ground sections
- [ ] Bridges are narrow with no walls
- [ ] Tunnels have ceilings
- [ ] All objects properly tagged and layered (GameConstants)
- [ ] Menu item "Metal Pod/Courses/Build All Courses" generates all 10
