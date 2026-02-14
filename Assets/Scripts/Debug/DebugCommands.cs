#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MetalPod.Core;
using MetalPod.Course;
using MetalPod.Hovercraft;
using MetalPod.Progression;
using MetalPod.Shared;
using MetalPod.Tutorial;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetalPod.Debugging
{
    /// <summary>
    /// Registers built-in debug commands for development playtesting.
    /// </summary>
    public static class DebugCommands
    {
        private static bool _godModeEnabled;

        public static void RegisterAll(DebugConsole console)
        {
            if (console == null)
            {
                return;
            }

            RegisterHelp(console);
            RegisterUtility(console);
            RegisterProgression(console);
            RegisterScene(console);
            RegisterGameplay(console);
            RegisterToolToggles(console);
        }

        private static void RegisterHelp(DebugConsole console)
        {
            console.RegisterCommand(new DebugCommand(
                "help",
                "List all available debug commands",
                "help",
                _ =>
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine("=== DEBUG COMMANDS ===");

                    foreach (DebugCommand command in console.GetAllCommands().OrderBy(c => c.Name))
                    {
                        builder.AppendLine($"{command.Name,-12} {command.Description}");
                    }

                    return builder.ToString().TrimEnd();
                }));

            console.RegisterCommand(new DebugCommand(
                "clear",
                "Clear console output",
                "clear",
                _ =>
                {
                    console.ClearLog();
                    return string.Empty;
                }));
        }

        private static void RegisterUtility(DebugConsole console)
        {
            console.RegisterCommand(new DebugCommand(
                "overlay",
                "Toggle FPS and memory overlay",
                "overlay",
                _ =>
                {
                    DebugOverlay overlay = GetOrCreateTool<DebugOverlay>();
                    if (overlay == null)
                    {
                        return "Unable to create/find DebugOverlay.";
                    }

                    overlay.Toggle();
                    return $"Overlay {(overlay.IsVisible ? "enabled" : "disabled")}.";
                }));

            console.RegisterCommand(new DebugCommand(
                "timescale",
                "Get or set Time.timeScale",
                "timescale [0..10]",
                args =>
                {
                    if (args.Length == 0)
                    {
                        return $"timeScale={Time.timeScale:F2}";
                    }

                    if (!float.TryParse(args[0], out float scale))
                    {
                        return "Invalid value. Example: timescale 0.5";
                    }

                    scale = Mathf.Clamp(scale, 0f, 10f);
                    Time.timeScale = scale;
                    Time.fixedDeltaTime = 0.02f * Mathf.Max(0.01f, scale);
                    return $"timeScale set to {scale:F2}";
                }));
        }

        private static void RegisterProgression(DebugConsole console)
        {
            console.RegisterCommand(new DebugCommand(
                "currency",
                "Get, set, or add currency",
                "currency [set|add] [amount]",
                args =>
                {
                    ProgressionManager progression = GetProgressionManager();
                    SaveSystem save = GetSaveSystem(progression);
                    if (save == null || save.CurrentData == null)
                    {
                        return "SaveSystem unavailable.";
                    }

                    if (args.Length == 0)
                    {
                        return $"Currency: {save.CurrentData.currency}";
                    }

                    if (args.Length < 2)
                    {
                        return "Usage: currency [set|add] [amount]";
                    }

                    if (!int.TryParse(args[1], out int amount))
                    {
                        return "Invalid amount.";
                    }

                    string mode = args[0].ToLowerInvariant();
                    if (mode == "set")
                    {
                        save.CurrentData.currency = Mathf.Max(0, amount);
                    }
                    else if (mode == "add")
                    {
                        save.CurrentData.currency = Mathf.Max(0, save.CurrentData.currency + amount);
                    }
                    else
                    {
                        return "Usage: currency [set|add] [amount]";
                    }

                    save.MarkDirty();
                    save.Save();

                    EventBus.RaiseCurrencyChanged(save.CurrentData.currency);
                    if (mode == "add" && amount > 0)
                    {
                        EventBus.RaiseCurrencyEarned(amount);
                    }

                    return $"Currency: {save.CurrentData.currency}";
                }));

            console.RegisterCommand(new DebugCommand(
                "unlockall",
                "Unlock all known courses",
                "unlockall",
                _ =>
                {
                    ProgressionManager progression = GetProgressionManager();
                    SaveSystem save = GetSaveSystem(progression);
                    if (progression == null || save == null || save.CurrentData == null)
                    {
                        return "Progression system unavailable.";
                    }

                    if (progression.AllCourses == null || progression.AllCourses.Length == 0)
                    {
                        return "No course data available on ProgressionManager.";
                    }

                    int unlocked = 0;
                    int alreadyUnlocked = 0;
                    for (int i = 0; i < progression.AllCourses.Length; i++)
                    {
                        MetalPod.ScriptableObjects.CourseDataSO course = progression.AllCourses[i];
                        if (course == null || string.IsNullOrWhiteSpace(course.courseId))
                        {
                            continue;
                        }

                        bool wasUnlocked = save.CurrentData.unlockedCourses.GetValueOrDefault(course.courseId, false);
                        save.CurrentData.unlockedCourses.Set(course.courseId, true);
                        if (wasUnlocked)
                        {
                            alreadyUnlocked++;
                        }
                        else
                        {
                            unlocked++;
                            EventBus.RaiseCourseUnlocked(course.courseId);
                        }
                    }

                    save.MarkDirty();
                    save.Save();
                    return $"Unlock complete. Newly unlocked: {unlocked}, already unlocked: {alreadyUnlocked}.";
                }));

            console.RegisterCommand(new DebugCommand(
                "upgrade",
                "Set a specific upgrade level",
                "upgrade [speed|handling|shield|boost|armor] [level]",
                args =>
                {
                    if (args.Length < 2)
                    {
                        return "Usage: upgrade [speed|handling|shield|boost|armor] [level]";
                    }

                    string upgradeId = NormalizeUpgradeId(args[0]);
                    if (!int.TryParse(args[1], out int requestedLevel))
                    {
                        return "Invalid level.";
                    }

                    ProgressionManager progression = GetProgressionManager();
                    SaveSystem save = GetSaveSystem(progression);
                    if (save == null || save.CurrentData == null)
                    {
                        return "SaveSystem unavailable.";
                    }

                    int maxLevel = GetUpgradeMaxLevel(progression, upgradeId);
                    int clampedLevel = Mathf.Clamp(requestedLevel, 0, maxLevel);
                    save.CurrentData.upgradeLevels.Set(upgradeId, clampedLevel);
                    save.MarkDirty();
                    save.Save();

                    EventBus.RaiseUpgradePurchased(upgradeId, clampedLevel);
                    return $"Upgrade '{upgradeId}' set to {clampedLevel}/{maxLevel}.";
                }));

            console.RegisterCommand(new DebugCommand(
                "maxupgrades",
                "Set all core upgrades to max level",
                "maxupgrades",
                _ =>
                {
                    ProgressionManager progression = GetProgressionManager();
                    SaveSystem save = GetSaveSystem(progression);
                    if (save == null || save.CurrentData == null)
                    {
                        return "SaveSystem unavailable.";
                    }

                    HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        "speed",
                        "handling",
                        "shield",
                        "boost"
                    };

                    foreach (KeyValuePair<string, int> pair in save.CurrentData.upgradeLevels)
                    {
                        if (!string.IsNullOrWhiteSpace(pair.Key))
                        {
                            ids.Add(NormalizeUpgradeId(pair.Key));
                        }
                    }

                    foreach (string id in ids)
                    {
                        int maxLevel = GetUpgradeMaxLevel(progression, id);
                        save.CurrentData.upgradeLevels.Set(id, maxLevel);
                        EventBus.RaiseUpgradePurchased(id, maxLevel);
                    }

                    save.MarkDirty();
                    save.Save();
                    return $"Maxed {ids.Count} upgrade categories.";
                }));

            console.RegisterCommand(new DebugCommand(
                "resetsave",
                "Reset all progression save data",
                "resetsave",
                _ =>
                {
                    SaveSystem save = GetSaveSystem(GetProgressionManager()) ?? UnityEngine.Object.FindObjectOfType<SaveSystem>();
                    if (save == null)
                    {
                        return "SaveSystem not found.";
                    }

                    save.ResetSave();

                    TryDeleteFile(save.BackupSavePath);
                    EventBus.RaiseCurrencyChanged(save.CurrentData != null ? save.CurrentData.currency : 0);

                    return $"Save data reset at '{save.SavePath}'.";
                }));
        }

        private static void RegisterScene(DebugConsole console)
        {
            console.RegisterCommand(new DebugCommand(
                "scene",
                "Load a scene by name",
                "scene [sceneName]",
                args =>
                {
                    if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
                    {
                        return "Usage: scene [sceneName]";
                    }

                    string sceneName = args[0];
                    if (!Application.CanStreamedLevelBeLoaded(sceneName))
                    {
                        return $"Scene '{sceneName}' is not in Build Settings.";
                    }

                    LoadScene(sceneName);
                    return $"Loading scene '{sceneName}'...";
                }));

            console.RegisterCommand(new DebugCommand(
                "scenes",
                "List scenes available in Build Settings",
                "scenes",
                _ =>
                {
                    int count = SceneManager.sceneCountInBuildSettings;
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"Scenes in build ({count}):");

                    for (int i = 0; i < count; i++)
                    {
                        string path = SceneUtility.GetScenePathByBuildIndex(i);
                        string name = Path.GetFileNameWithoutExtension(path);
                        builder.AppendLine($"[{i}] {name}");
                    }

                    builder.AppendLine($"Current: {SceneManager.GetActiveScene().name}");
                    return builder.ToString().TrimEnd();
                }));
        }

        private static void RegisterGameplay(DebugConsole console)
        {
            console.RegisterCommand(new DebugCommand(
                "god",
                "Toggle invincibility",
                "god",
                _ =>
                {
                    HovercraftHealth health = GetHovercraftHealth();
                    if (health == null)
                    {
                        return "HovercraftHealth not found.";
                    }

                    _godModeEnabled = !_godModeEnabled;
                    if (!TrySetGodMode(health, _godModeEnabled))
                    {
                        _godModeEnabled = false;
                        return "Unable to toggle invincibility on this build.";
                    }

                    if (_godModeEnabled)
                    {
                        health.RestoreToFull();
                    }

                    return $"God mode {(_godModeEnabled ? "ON" : "OFF")}.";
                }));

            console.RegisterCommand(new DebugCommand(
                "heal",
                "Restore hovercraft health and shield",
                "heal",
                _ =>
                {
                    HovercraftHealth health = GetHovercraftHealth();
                    if (health == null)
                    {
                        return "HovercraftHealth not found.";
                    }

                    health.RestoreToFull();
                    return "Hovercraft restored to full health and shield.";
                }));

            console.RegisterCommand(new DebugCommand(
                "damage",
                "Deal damage to hovercraft",
                "damage [amount] [damageType]",
                args =>
                {
                    HovercraftHealth health = GetHovercraftHealth();
                    if (health == null)
                    {
                        return "HovercraftHealth not found.";
                    }

                    float amount = 10f;
                    if (args.Length >= 1 && !float.TryParse(args[0], out amount))
                    {
                        return "Invalid amount.";
                    }

                    DamageType type = DamageType.Physical;
                    if (args.Length >= 2 && !Enum.TryParse(args[1], true, out type))
                    {
                        return "Invalid damage type.";
                    }

                    health.TakeDamage(Mathf.Max(0f, amount), type);
                    return $"Damage applied: {amount:F1} ({type}). " +
                           $"HP {health.CurrentHealth:F1}/{health.MaxHealth:F1}, " +
                           $"Shield {health.CurrentShield:F1}/{health.MaxShield:F1}.";
                }));

            console.RegisterCommand(new DebugCommand(
                "speed",
                "Show hovercraft speed information",
                "speed",
                _ =>
                {
                    HovercraftController controller = GetHovercraftController();
                    if (controller == null)
                    {
                        return "HovercraftController not found.";
                    }

                    float speed = controller.CurrentSpeed;
                    Rigidbody rb = controller.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        speed = rb.velocity.magnitude;
                    }

                    return $"Speed {speed:F1} m/s ({speed * 3.6f:F1} km/h), state={controller.CurrentState}.";
                }));

            console.RegisterCommand(new DebugCommand(
                "skiptutorial",
                "Mark tutorials complete and disable active tutorial",
                "skiptutorial",
                _ =>
                {
                    TutorialSaveData.SetTutorialCompleted("first_play");
                    TutorialSaveData.SetTutorialCompleted("workshop_intro");

                    TutorialManager manager = UnityEngine.Object.FindObjectOfType<TutorialManager>();
                    if (manager != null)
                    {
                        manager.enabled = false;
                    }

                    Time.timeScale = 1f;
                    return "Tutorials marked complete.";
                }));

            console.RegisterCommand(new DebugCommand(
                "medal",
                "Award medal for current course",
                "medal [gold|silver|bronze|0-3] [timeSeconds]",
                args =>
                {
                    if (args.Length < 1)
                    {
                        return "Usage: medal [gold|silver|bronze|0-3] [timeSeconds]";
                    }

                    if (!TryParseMedal(args[0], out int medal))
                    {
                        return "Invalid medal. Use gold/silver/bronze or 0-3.";
                    }

                    float completionTime = 30f;
                    if (args.Length >= 2 && !float.TryParse(args[1], out completionTime))
                    {
                        return "Invalid completion time.";
                    }

                    completionTime = Mathf.Max(0.01f, completionTime);
                    string courseId = ResolveCurrentCourseId();
                    EventBus.RaiseCourseCompleted(courseId, completionTime, medal);
                    return $"Awarded medal={medal} for '{courseId}' at {completionTime:F2}s.";
                }));

            console.RegisterCommand(new DebugCommand(
                "finish",
                "Instantly finish current course with gold medal",
                "finish [timeSeconds]",
                args =>
                {
                    float completionTime = 30f;
                    if (args.Length >= 1 && !float.TryParse(args[0], out completionTime))
                    {
                        return "Invalid completion time.";
                    }

                    completionTime = Mathf.Max(0.01f, completionTime);
                    string courseId = ResolveCurrentCourseId();
                    EventBus.RaiseCourseCompleted(courseId, completionTime, 3);

                    CourseManager manager = UnityEngine.Object.FindObjectOfType<CourseManager>();
                    if (manager != null)
                    {
                        return $"Finish event sent for '{courseId}' ({completionTime:F2}s, gold).";
                    }

                    return $"Course completion event raised for '{courseId}' ({completionTime:F2}s, gold).";
                }));
        }

        private static void RegisterToolToggles(DebugConsole console)
        {
            console.RegisterCommand(new DebugCommand(
                "coursemenu",
                "Toggle quick course skip window",
                "coursemenu",
                _ =>
                {
                    DebugCourseSkip skip = GetOrCreateTool<DebugCourseSkip>();
                    if (skip == null)
                    {
                        return "Unable to create/find DebugCourseSkip.";
                    }

                    skip.Toggle();
                    return $"Course menu {(skip.IsVisible ? "enabled" : "disabled")}.";
                }));

            console.RegisterCommand(new DebugCommand(
                "hazards",
                "Toggle hazard tester window",
                "hazards",
                _ =>
                {
                    DebugHazardTester tester = GetOrCreateTool<DebugHazardTester>();
                    if (tester == null)
                    {
                        return "Unable to create/find DebugHazardTester.";
                    }

                    tester.Toggle();
                    return $"Hazard tester {(tester.IsVisible ? "enabled" : "disabled")}.";
                }));
        }

        private static ProgressionManager GetProgressionManager()
        {
            return ProgressionManager.Instance ?? UnityEngine.Object.FindObjectOfType<ProgressionManager>();
        }

        private static SaveSystem GetSaveSystem(ProgressionManager progression)
        {
            SaveSystem save = null;

            if (progression != null)
            {
                save = progression.GetComponent<SaveSystem>();
            }

            if (save == null)
            {
                save = UnityEngine.Object.FindObjectOfType<SaveSystem>();
            }

            if (save != null)
            {
                save.Initialize();
            }

            return save;
        }

        private static HovercraftController GetHovercraftController()
        {
            if (GameManager.Instance != null && GameManager.Instance.ActiveHovercraft != null)
            {
                return GameManager.Instance.ActiveHovercraft;
            }

            return UnityEngine.Object.FindObjectOfType<HovercraftController>();
        }

        private static HovercraftHealth GetHovercraftHealth()
        {
            HovercraftController controller = GetHovercraftController();
            if (controller != null)
            {
                return controller.GetComponent<HovercraftHealth>();
            }

            return UnityEngine.Object.FindObjectOfType<HovercraftHealth>();
        }

        private static T GetOrCreateTool<T>() where T : Component
        {
            T existing = UnityEngine.Object.FindObjectOfType<T>();
            if (existing != null)
            {
                return existing;
            }

            if (DebugConsole.Instance != null)
            {
                return DebugConsole.Instance.gameObject.AddComponent<T>();
            }

            GameObject host = new GameObject(typeof(T).Name);
            UnityEngine.Object.DontDestroyOnLoad(host);
            return host.AddComponent<T>();
        }

        private static void LoadScene(string sceneName)
        {
            if (GameManager.Instance != null && GameManager.Instance.SceneLoader != null)
            {
                GameManager.Instance.SceneLoader.LoadSceneAsync(sceneName);
                return;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        private static string NormalizeUpgradeId(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            string normalized = raw.Trim().ToLowerInvariant();
            return normalized == "armor" ? "shield" : normalized;
        }

        private static int GetUpgradeMaxLevel(ProgressionManager progression, string upgradeId)
        {
            int maxLevel = progression != null && progression.Upgrades != null
                ? progression.Upgrades.GetMaxLevel(upgradeId)
                : 0;

            if (maxLevel <= 0)
            {
                return 5;
            }

            return maxLevel;
        }

        private static bool TryParseMedal(string value, out int medal)
        {
            medal = 0;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string token = value.Trim().ToLowerInvariant();
            switch (token)
            {
                case "gold":
                    medal = 3;
                    return true;
                case "silver":
                    medal = 2;
                    return true;
                case "bronze":
                    medal = 1;
                    return true;
            }

            if (int.TryParse(token, out int parsed))
            {
                medal = Mathf.Clamp(parsed, 0, 3);
                return true;
            }

            return false;
        }

        private static string ResolveCurrentCourseId()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentCourse != null)
            {
                string courseId = GameManager.Instance.CurrentCourse.courseId;
                if (!string.IsNullOrWhiteSpace(courseId))
                {
                    return courseId;
                }
            }

            string sceneName = SceneManager.GetActiveScene().name;

            ProgressionManager progression = GetProgressionManager();
            if (progression != null && progression.AllCourses != null)
            {
                for (int i = 0; i < progression.AllCourses.Length; i++)
                {
                    MetalPod.ScriptableObjects.CourseDataSO course = progression.AllCourses[i];
                    if (course == null || string.IsNullOrWhiteSpace(course.sceneName))
                    {
                        continue;
                    }

                    if (!string.Equals(course.sceneName, sceneName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(course.courseId))
                    {
                        return course.courseId;
                    }
                }
            }

            return sceneName;
        }

        private static bool TrySetGodMode(HovercraftHealth health, bool enabled)
        {
            if (health == null)
            {
                return false;
            }

            health.SetInvincible(enabled);
            return true;
        }

        private static void TryDeleteFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                File.Delete(path);
            }
            catch
            {
                // Best-effort cleanup for reset command.
            }
        }
    }
}
#endif
