# Codex Task 22: Critical Bug Fixes from Round 5 Code Review

> **Goal**: Fix 3 critical issues found during code review of Round 5 agent output. Two are quick compilation fixes in PlayMode tests, one is an architectural fix in CloudSave.

---

## Context

A code review of Tasks 16–21 identified 3 critical bugs. C1 and C2 prevent the PlayMode test suite from compiling at all. C3 causes silent data-loss on iCloud restore.

**Read these files before making changes:**
- `Assets/Tests/PlayMode/PlayModeTestBase.cs` — Fix C1
- `Assets/Tests/PlayMode/PlayModeTestFactory.cs` — Fix C2
- `Assets/Scripts/CloudSave/CloudSaveManager.cs` — Fix C3
- `Assets/Scripts/Progression/SaveSystem.cs` — Fix C3 (add Reload method)
- `Assets/Scripts/Hazards/HazardBase.cs` — Reference for C2 (do NOT modify)

---

## Fix C1: Sealed Class Generic Constraint (PlayModeTestBase.cs)

### Problem
Line 54 declares:
```csharp
protected T TrackObject<T>(T gameObject) where T : GameObject
```
`GameObject` is a **sealed class** in Unity. C# does not allow sealed classes as generic type constraints. This causes a **compilation error** that cascades to every test file calling `TrackObject()` (`HovercraftIntegrationTests.cs`, `CourseFlowTests.cs`, `HazardDamageTests.cs`).

### Fix
Remove the generic constraint. Change the method to accept `GameObject` directly.

### Exact Changes

In `Assets/Tests/PlayMode/PlayModeTestBase.cs`, **replace** lines 54–62:

```csharp
        protected T TrackObject<T>(T gameObject) where T : GameObject
        {
            if (gameObject != null)
            {
                _createdObjects.Add(gameObject);
            }

            return gameObject;
        }
```

**With:**

```csharp
        protected GameObject TrackObject(GameObject gameObject)
        {
            if (gameObject != null)
            {
                _createdObjects.Add(gameObject);
            }

            return gameObject;
        }
```

This is a drop-in replacement — all call sites already pass `GameObject` and assign the result to `GameObject` variables.

---

## Fix C2: Reflection on Inherited Protected Fields (PlayModeTestFactory.cs)

### Problem
`SetPrivateField` at line 226–235 uses:
```csharp
FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
```

When called on a `DamageZone` instance, `target.GetType()` returns `typeof(DamageZone)`. But the fields `hazardData`, `damagePerHit`, `damagePerSecond`, `damageType`, and `isActive` are declared as `protected` on the **base class** `HazardBase` (at `Assets/Scripts/Hazards/HazardBase.cs` lines 10–14). `Type.GetField()` does **NOT** search non-public fields on base types — it only finds fields declared on the exact type queried.

This causes all 5 `SetPrivateField` calls in `CreateDamageZone()` (lines 218–222) to throw `MissingFieldException`, breaking all 5 hazard damage tests.

### Fix
Walk the type hierarchy in `SetPrivateField` to find fields declared on any ancestor class.

### Exact Changes

In `Assets/Tests/PlayMode/PlayModeTestFactory.cs`, **replace** lines 226–235:

```csharp
        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(target.GetType().FullName, fieldName);
            }

            field.SetValue(target, value);
        }
```

**With:**

```csharp
        private static void SetPrivateField(object target, string fieldName, object value)
        {
            Type type = target.GetType();
            FieldInfo field = null;

            // Walk the type hierarchy to find fields on base classes
            while (type != null)
            {
                field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field != null) break;
                type = type.BaseType;
            }

            if (field == null)
            {
                throw new MissingFieldException(target.GetType().FullName, fieldName);
            }

            field.SetValue(target, value);
        }
```

Also check `PlayModeTestBase.cs` — if it has a similar `SetPrivateField` or `GetPrivateField` helper, apply the same hierarchy-walking fix there too.

---

## Fix C3: Cloud Restore Reflection Hack (CloudSaveManager.cs + SaveSystem.cs)

### Problem
`ApplyRestoredDataToSaveSystem` at line 457–468 of `CloudSaveManager.cs` uses reflection to find `<CurrentData>k__BackingField` — a C# compiler implementation detail. Under IL2CPP (which Metal Pod uses for iOS builds), code stripping may remove this backing field, causing the reflection lookup to return `null`. When this happens:

1. Cloud restore writes the correct JSON to disk ✓
2. The running game's `SaveSystem.CurrentData` is **NOT updated** ✗
3. The user thinks the restore succeeded, but the game keeps running on old data
4. The next autosave **overwrites** the restored file with the old in-memory data ✗✗

This is a silent data-loss scenario.

### Fix
Add a public `ReloadFromDisk()` method to `SaveSystem.cs` that re-reads the save file into `CurrentData`. Then replace the reflection hack in `CloudSaveManager` with a direct call.

### Exact Changes

**Step 1:** In `Assets/Scripts/Progression/SaveSystem.cs`, add the following method **after** the `ResetSave()` method (after line 130):

```csharp
        /// <summary>
        /// Reloads save data from disk, replacing the in-memory CurrentData.
        /// Used by CloudSaveManager after writing a restored file to disk.
        /// </summary>
        public void ReloadFromDisk()
        {
            SaveData loaded = Load();
            if (loaded != null)
            {
                CurrentData = loaded;
            }
            else
            {
                CurrentData = SaveData.CreateDefault();
            }

            MigrateIfNeeded(CurrentData);
            _dirty = false;
            _autoSaveTimer = 0f;
        }
```

**Step 2:** In `Assets/Scripts/CloudSave/CloudSaveManager.cs`, **replace** the `ApplyRestoredDataToSaveSystem` method (lines 457–468):

```csharp
        private void ApplyRestoredDataToSaveSystem(SaveData cloudData)
        {
            // SaveSystem doesn't expose a setter/reload API; update CurrentData via backing field.
            FieldInfo currentDataField = typeof(SaveSystem).GetField("<CurrentData>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (currentDataField == null)
            {
                Debug.LogWarning("[CloudSave] Could not update SaveSystem.CurrentData at runtime. Data will apply on next launch.");
                return;
            }

            currentDataField.SetValue(_saveSystem, cloudData);
        }
```

**With:**

```csharp
        private void ApplyRestoredDataToSaveSystem(SaveData cloudData)
        {
            _saveSystem.ReloadFromDisk();
        }
```

**Step 3:** Also remove the `using System.Reflection;` import from `CloudSaveManager.cs` (line 3) if no other code in the file uses reflection. Check first — if nothing else references `System.Reflection`, remove it.

---

## Files Modified

```
Assets/Tests/PlayMode/PlayModeTestBase.cs          ← Fix C1 (remove sealed generic constraint)
Assets/Tests/PlayMode/PlayModeTestFactory.cs        ← Fix C2 (hierarchy-walking reflection)
Assets/Scripts/Progression/SaveSystem.cs            ← Fix C3 (add ReloadFromDisk method)
Assets/Scripts/CloudSave/CloudSaveManager.cs        ← Fix C3 (replace reflection with direct call)
```

---

## Acceptance Criteria

- [ ] `TrackObject` is a non-generic method accepting `GameObject` — no `where T : GameObject` constraint
- [ ] All test files that call `TrackObject()` compile without error
- [ ] `SetPrivateField` walks the type hierarchy (`type = type.BaseType`) to find inherited fields
- [ ] `CreateDamageZone()` successfully sets `hazardData`, `damagePerHit`, `damagePerSecond`, `damageType`, `isActive` on `DamageZone` (fields declared on `HazardBase`)
- [ ] `SaveSystem.ReloadFromDisk()` exists and re-reads the save file into `CurrentData`
- [ ] `CloudSaveManager.ApplyRestoredDataToSaveSystem` calls `_saveSystem.ReloadFromDisk()` — no reflection
- [ ] `System.Reflection` import removed from `CloudSaveManager.cs` if unused
- [ ] No other files modified beyond the 4 listed
- [ ] All changes compile without errors
