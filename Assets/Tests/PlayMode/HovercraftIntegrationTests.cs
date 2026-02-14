using System.Collections;
using MetalPod.Hovercraft;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MetalPod.Tests.PlayMode
{
    public class HovercraftIntegrationTests : PlayModeTestBase
    {
        [UnityTest]
        public IEnumerator HovercraftSpawn_HasAllRequiredComponents()
        {
            HovercraftController controller = CreateHovercraftController();
            yield return null;

            Assert.IsNotNull(controller.GetComponent<Rigidbody>());
            Assert.IsNotNull(controller.GetComponent<HovercraftPhysics>());
            Assert.IsNotNull(controller.GetComponent<HovercraftHealth>());
            Assert.IsNotNull(controller.GetComponent<HovercraftInput>());
            Assert.IsNotNull(controller.GetComponent<HovercraftStats>());
        }

        [UnityTest]
        public IEnumerator HovercraftDamage_ReducesHealth()
        {
            HovercraftController controller = CreateHovercraftController();
            HovercraftHealth health = controller.GetComponent<HovercraftHealth>();
            float initialHealth = health.CurrentHealth;

            health.TakeDamage(80f, DamageType.Physical);
            yield return null;

            Assert.Less(health.CurrentHealth, initialHealth);
        }

        [UnityTest]
        public IEnumerator HovercraftDamage_FiresOnDamageEvent()
        {
            HovercraftController controller = CreateHovercraftController();
            HovercraftHealth health = controller.GetComponent<HovercraftHealth>();

            bool fired = false;
            float finalDamage = 0f;
            float remainingDamage = 0f;
            health.OnDamage += (total, remaining) =>
            {
                fired = true;
                finalDamage = total;
                remainingDamage = remaining;
            };

            health.TakeDamage(20f, DamageType.Fire);
            yield return null;

            Assert.IsTrue(fired);
            Assert.Greater(finalDamage, 0f);
            Assert.GreaterOrEqual(remainingDamage, 0f);
        }

        [UnityTest]
        public IEnumerator HovercraftDestruction_FiresOnDestroyedEvent()
        {
            HovercraftController controller = CreateHovercraftController();
            HovercraftHealth health = controller.GetComponent<HovercraftHealth>();

            bool destroyed = false;
            health.OnDestroyed += () => destroyed = true;

            health.TakeDamage(200f, DamageType.Explosive);
            yield return null;

            Assert.IsTrue(destroyed);
            Assert.IsTrue(health.IsDestroyed);
            Assert.AreEqual(HovercraftState.Destroyed, controller.CurrentState);
        }

        [UnityTest]
        public IEnumerator HovercraftRestoreToFull_ResetsHealth()
        {
            HovercraftController controller = CreateHovercraftController();
            HovercraftHealth health = controller.GetComponent<HovercraftHealth>();

            health.TakeDamage(75f, DamageType.Physical);
            yield return null;
            health.RestoreToFull();

            Assert.AreEqual(health.MaxHealth, health.CurrentHealth, 0.001f);
            Assert.AreEqual(health.MaxShield, health.CurrentShield, 0.001f);
            Assert.IsFalse(health.IsDestroyed);
        }

        [UnityTest]
        public IEnumerator HovercraftShield_AbsorbsDamageFirst()
        {
            HovercraftController controller = CreateHovercraftController();
            HovercraftHealth health = controller.GetComponent<HovercraftHealth>();

            health.TakeDamage(30f, DamageType.Physical);
            yield return null;

            Assert.AreEqual(20f, health.CurrentShield, 0.001f);
            Assert.AreEqual(100f, health.CurrentHealth, 0.001f);
        }

        [UnityTest]
        public IEnumerator HovercraftState_ChangesOnBoost()
        {
            HovercraftController controller = CreateHovercraftController();
            HovercraftInput input = controller.GetComponent<HovercraftInput>();

            input.enabled = false;
            SetAutoProperty(input, "BoostPressedThisFrame", true);
            yield return null;

            Assert.AreEqual(HovercraftState.Boosting, controller.CurrentState);
        }

        [UnityTest]
        public IEnumerator HovercraftPhysics_RigidbodyExists()
        {
            HovercraftController controller = CreateHovercraftController();
            HovercraftPhysics physics = controller.GetComponent<HovercraftPhysics>();
            yield return null;

            Assert.IsNotNull(physics);
            Assert.IsNotNull(physics.Rigidbody);
        }

        [UnityTest]
        public IEnumerator HovercraftHealth_SpeedMultiplier_AtLowHealth()
        {
            HovercraftController controller = CreateHovercraftController();
            HovercraftHealth health = controller.GetComponent<HovercraftHealth>();

            health.TakeDamage(50f, DamageType.Physical);
            health.TakeDamage(80f, DamageType.Physical);
            yield return null;

            Assert.AreEqual(20f, health.CurrentHealth, 0.001f);
            Assert.AreEqual(0.6f, health.SpeedMultiplier, 0.001f);
        }

        private HovercraftController CreateHovercraftController()
        {
            HovercraftStatsSO stats = TrackAsset(PlayModeTestFactory.CreateHovercraftStats());
            GameObject hovercraft = TrackObject(PlayModeTestFactory.CreateHovercraft(stats));
            return hovercraft.GetComponent<HovercraftController>();
        }
    }
}
