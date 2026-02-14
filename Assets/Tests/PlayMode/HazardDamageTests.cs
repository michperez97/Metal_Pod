using System.Collections;
using MetalPod.Hazards;
using MetalPod.Hovercraft;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MetalPod.Tests.PlayMode
{
    public class HazardDamageTests : PlayModeTestBase
    {
        [UnityTest]
        public IEnumerator HazardBase_DealsDamageOnContact()
        {
            HovercraftHealth health = CreatePlayer(out Collider playerCollider);
            DamageZone hazard = CreateHazardZone(20f, 0f, DamageType.Physical);
            float shieldBefore = health.CurrentShield;

            hazard.gameObject.SendMessage(
                "OnTriggerEnter",
                playerCollider,
                SendMessageOptions.DontRequireReceiver);
            yield return null;

            Assert.Less(health.CurrentShield, shieldBefore);
        }

        [UnityTest]
        public IEnumerator DamageType_AppliedCorrectly()
        {
            HovercraftHealth health = CreatePlayer(out Collider playerCollider);
            DamageZone hazard = CreateHazardZone(10f, 0f, DamageType.Fire);

            bool fired = false;
            DamageType receivedType = DamageType.Physical;
            health.OnDamageTyped += (_, _, type) =>
            {
                fired = true;
                receivedType = type;
            };

            hazard.gameObject.SendMessage(
                "OnTriggerEnter",
                playerCollider,
                SendMessageOptions.DontRequireReceiver);
            yield return null;

            Assert.IsTrue(fired);
            Assert.AreEqual(DamageType.Fire, receivedType);
        }

        [UnityTest]
        public IEnumerator HazardWarning_ActivatesBeforeDamage()
        {
            HovercraftHealth health = CreatePlayer(out Collider playerCollider);
            DamageZone hazard = CreateHazardZone(10f, 0f, DamageType.Physical, new Vector3(0f, 0f, 5f));

            HazardWarning warning = CreateTestObject("HazardWarning").AddComponent<HazardWarning>();
            HovercraftController player = health.GetComponent<HovercraftController>();
            SetPrivateField(warning, "player", player);
            SetPrivateField(warning, "detectionRadius", 50f);
            SetPrivateField(warning, "refreshInterval", 0.01f);

            int sequence = 0;
            int warningOrder = 0;
            int damageOrder = 0;

            warning.OnWarningUpdated += (_, threatLevel) =>
            {
                if (threatLevel > 0 && warningOrder == 0)
                {
                    warningOrder = ++sequence;
                }
            };

            health.OnDamageTyped += (_, _, _) => damageOrder = ++sequence;

            yield return WaitSeconds(0.05f);
            hazard.gameObject.SendMessage(
                "OnTriggerEnter",
                playerCollider,
                SendMessageOptions.DontRequireReceiver);
            yield return null;

            Assert.Greater(warningOrder, 0);
            Assert.Greater(damageOrder, warningOrder);
        }

        [UnityTest]
        public IEnumerator MultipleHazards_StackDamage()
        {
            HovercraftHealth health = CreatePlayer(out Collider playerCollider);
            DamageZone first = CreateHazardZone(10f, 0f, DamageType.Physical, new Vector3(-1f, 0f, 0f));
            DamageZone second = CreateHazardZone(10f, 0f, DamageType.Physical, new Vector3(1f, 0f, 0f));

            float shieldBefore = health.CurrentShield;

            first.gameObject.SendMessage("OnTriggerEnter", playerCollider, SendMessageOptions.DontRequireReceiver);
            second.gameObject.SendMessage("OnTriggerEnter", playerCollider, SendMessageOptions.DontRequireReceiver);
            yield return null;

            Assert.AreEqual(shieldBefore - 20f, health.CurrentShield, 0.1f);
        }

        [UnityTest]
        public IEnumerator Invincibility_BlocksDamage()
        {
            HovercraftHealth health = CreatePlayer(out Collider playerCollider);
            DamageZone hazard = CreateHazardZone(200f, 0f, DamageType.Explosive);

            SetAutoProperty(health, "IsInvincible", true);
            float healthBefore = health.CurrentHealth;
            float shieldBefore = health.CurrentShield;

            hazard.gameObject.SendMessage(
                "OnTriggerEnter",
                playerCollider,
                SendMessageOptions.DontRequireReceiver);
            yield return null;

            Assert.AreEqual(healthBefore, health.CurrentHealth, 0.001f);
            Assert.AreEqual(shieldBefore, health.CurrentShield, 0.001f);
        }

        private HovercraftHealth CreatePlayer(out Collider playerCollider)
        {
            HovercraftStatsSO stats = TrackAsset(PlayModeTestFactory.CreateHovercraftStats());
            GameObject hovercraftObject = TrackObject(PlayModeTestFactory.CreateHovercraft(stats));
            HovercraftHealth health = hovercraftObject.GetComponent<HovercraftHealth>();
            playerCollider = hovercraftObject.GetComponent<Collider>();
            return health;
        }

        private DamageZone CreateHazardZone(
            float damagePerHit,
            float damagePerSecond,
            DamageType damageType,
            Vector3? position = null)
        {
            HazardDataSO hazardData = TrackAsset(PlayModeTestFactory.CreateHazardData(
                "hazard_runtime",
                damagePerSecond,
                damagePerHit));
            GameObject hazardObject = TrackObject(PlayModeTestFactory.CreateDamageZone(
                position ?? Vector3.zero,
                new Vector3(4f, 4f, 4f),
                damagePerHit,
                damagePerSecond,
                damageType,
                hazardData));
            return hazardObject.GetComponent<DamageZone>();
        }
    }
}
