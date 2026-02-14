using System;
using System.Reflection;
using MetalPod.Course;
using MetalPod.Hazards;
using MetalPod.Hovercraft;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Tests.PlayMode
{
    public static class PlayModeTestFactory
    {
        public static HovercraftStatsSO CreateHovercraftStats(
            float maxSpeed = 40f,
            float maxHealth = 100f,
            float maxShield = 50f,
            float boostMultiplier = 1.5f,
            float boostDuration = 2f,
            float boostCooldown = 0f)
        {
            HovercraftStatsSO stats = ScriptableObject.CreateInstance<HovercraftStatsSO>();
            stats.baseSpeed = 20f;
            stats.maxSpeed = maxSpeed;
            stats.maxHealth = maxHealth;
            stats.maxShield = maxShield;
            stats.boostMultiplier = boostMultiplier;
            stats.boostDuration = boostDuration;
            stats.boostCooldown = boostCooldown;
            stats.brakeForce = 15f;
            stats.turnSpeed = 3f;
            stats.hoverHeight = 2f;
            stats.hoverForce = 65f;
            stats.hoverDamping = 5f;
            stats.shieldRegenRate = 0f;
            stats.shieldRegenDelay = 1000f;
            stats.driftFactor = 0.95f;
            stats.stabilizationForce = 10f;
            return stats;
        }

        public static CourseDataSO CreateCourseData(
            string courseId = "test_course",
            float goldTime = 30f,
            float silverTime = 45f,
            float bronzeTime = 60f,
            DifficultyLevel difficulty = DifficultyLevel.Easy,
            EnvironmentType environment = EnvironmentType.Ice,
            int courseIndex = 1)
        {
            CourseDataSO data = ScriptableObject.CreateInstance<CourseDataSO>();
            data.courseId = courseId;
            data.courseName = courseId;
            data.sceneName = $"Scene_{courseId}";
            data.goldTime = goldTime;
            data.silverTime = silverTime;
            data.bronzeTime = bronzeTime;
            data.difficulty = difficulty;
            data.environmentType = environment;
            data.courseIndex = courseIndex;
            data.requiredMedals = 0;
            return data;
        }

        public static HazardDataSO CreateHazardData(
            string hazardId = "test_hazard",
            float damagePerSecond = 0f,
            float damagePerHit = 10f)
        {
            HazardDataSO data = ScriptableObject.CreateInstance<HazardDataSO>();
            data.hazardId = hazardId;
            data.hazardName = hazardId;
            data.damagePerSecond = damagePerSecond;
            data.damagePerHit = damagePerHit;
            data.warningRadius = 25f;
            data.threatLevel = 2;
            return data;
        }

        public static UpgradeDataSO CreateUpgradeData(string upgradeId, UpgradeCategory category, params int[] levelCosts)
        {
            if (levelCosts == null || levelCosts.Length == 0)
            {
                levelCosts = new[] { 100 };
            }

            UpgradeDataSO data = ScriptableObject.CreateInstance<UpgradeDataSO>();
            data.upgradeId = upgradeId;
            data.upgradeName = upgradeId;
            data.category = category;
            data.levels = new UpgradeLevel[levelCosts.Length];
            for (int i = 0; i < levelCosts.Length; i++)
            {
                data.levels[i] = new UpgradeLevel
                {
                    cost = levelCosts[i],
                    statMultiplier = 1f + (i + 1) * 0.1f,
                    description = $"{upgradeId}_level_{i + 1}"
                };
            }

            return data;
        }

        public static CosmeticDataSO CreateCosmeticData(
            string cosmeticId,
            CosmeticType cosmeticType,
            int cost,
            int requiredMedals = 0)
        {
            CosmeticDataSO data = ScriptableObject.CreateInstance<CosmeticDataSO>();
            data.cosmeticId = cosmeticId;
            data.cosmeticName = cosmeticId;
            data.cosmeticType = cosmeticType;
            data.cost = cost;
            data.requiredMedals = requiredMedals;
            data.primaryColor = Color.white;
            data.secondaryColor = Color.gray;
            data.accentColor = Color.black;
            return data;
        }

        public static GameObject CreateHovercraft(HovercraftStatsSO stats = null)
        {
            if (stats == null)
            {
                stats = CreateHovercraftStats();
            }

            GameObject gameObject = new GameObject("TestHovercraft");
            gameObject.tag = GameConstants.TAG_PLAYER;
            gameObject.layer = LayerMask.NameToLayer("Default");

            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.mass = 10f;

            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(1f, 1f, 2f);

            HovercraftPhysics physics = gameObject.AddComponent<HovercraftPhysics>();
            HovercraftHealth health = gameObject.AddComponent<HovercraftHealth>();
            gameObject.AddComponent<HovercraftInput>();
            HovercraftController controller = gameObject.AddComponent<HovercraftController>();
            HovercraftStats runtimeStats = gameObject.AddComponent<HovercraftStats>();

            SetPrivateField(physics, "stats", stats);
            SetPrivateField(health, "stats", stats);
            SetPrivateField(health, "respawnInvincibilitySeconds", 0f);
            SetPrivateField(controller, "stats", stats);
            SetPrivateField(runtimeStats, "baseStats", stats);

            health.RestoreToFull();
            return gameObject;
        }

        public static GameObject CreateCheckpoint(Vector3 position, int index)
        {
            GameObject gameObject = new GameObject($"Checkpoint_{index}");
            gameObject.tag = GameConstants.TAG_CHECKPOINT;
            gameObject.transform.position = position;

            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(6f, 6f, 6f);

            Checkpoint checkpoint = gameObject.AddComponent<Checkpoint>();
            SetPrivateField(checkpoint, "checkpointIndex", index);
            return gameObject;
        }

        public static GameObject CreateCollectible(Vector3 position, CollectibleType type = CollectibleType.Currency)
        {
            GameObject gameObject = new GameObject($"Collectible_{type}");
            gameObject.tag = GameConstants.TAG_COLLECTIBLE;
            gameObject.transform.position = position;

            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 1f;

            Collectible collectible = gameObject.AddComponent<Collectible>();
            SetPrivateField(collectible, "type", type);
            return gameObject;
        }

        public static GameObject CreateFinishLine(Vector3 position)
        {
            GameObject gameObject = new GameObject("FinishLine");
            gameObject.tag = GameConstants.TAG_FINISH;
            gameObject.transform.position = position;

            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(6f, 6f, 2f);

            gameObject.AddComponent<FinishLine>();
            return gameObject;
        }

        public static GameObject CreateDamageZone(
            Vector3 position,
            Vector3 size,
            float damagePerHit,
            float damagePerSecond,
            DamageType damageType = DamageType.Physical,
            HazardDataSO hazardData = null)
        {
            GameObject gameObject = new GameObject("DamageZone");
            gameObject.tag = GameConstants.TAG_HAZARD;
            gameObject.transform.position = position;

            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;

            DamageZone zone = gameObject.AddComponent<DamageZone>();
            SetPrivateField(zone, "hazardData", hazardData);
            SetPrivateField(zone, "damagePerHit", damagePerHit);
            SetPrivateField(zone, "damagePerSecond", damagePerSecond);
            SetPrivateField(zone, "damageType", damageType);
            SetPrivateField(zone, "isActive", true);
            return gameObject;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(target.GetType().FullName, fieldName);
            }

            field.SetValue(target, value);
        }
    }
}
