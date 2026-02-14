using NUnit.Framework;
using MetalPod.Hovercraft;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Tests.EditMode
{
    [TestFixture]
    public class DamageCalculationTests
    {
        private GameObject _hovercraftObject;
        private HovercraftHealth _health;
        private HovercraftStatsSO _stats;

        [SetUp]
        public void SetUp()
        {
            _hovercraftObject = new GameObject("Test_HovercraftHealth");
            _health = _hovercraftObject.AddComponent<HovercraftHealth>();

            _stats = ScriptableObject.CreateInstance<HovercraftStatsSO>();
            _stats.maxHealth = 100f;
            _stats.maxShield = 50f;
            _stats.shieldRegenRate = 0f;
            _stats.shieldRegenDelay = 1000f;

            TestDataFactory.SetPrivateField(_health, "stats", _stats);
            TestDataFactory.SetPrivateField(_health, "respawnInvincibilitySeconds", 0f);
            _health.RestoreToFull();
        }

        [TearDown]
        public void TearDown()
        {
            TestDataFactory.DestroyAll(_hovercraftObject, _stats);
        }

        [Test]
        public void DamageTypeModifier_Physical_IsOne()
        {
            _health.TakeDamage(10f, DamageType.Physical);
            Assert.AreEqual(40f, _health.CurrentShield, 0.001f);
            Assert.AreEqual(100f, _health.CurrentHealth, 0.001f);
        }

        [Test]
        public void DamageTypeModifier_Electric_IsHigher()
        {
            _health.TakeDamage(10f, DamageType.Electric);
            Assert.AreEqual(38f, _health.CurrentShield, 0.001f);
        }

        [Test]
        public void DamageTypeModifier_Explosive_IsHighest()
        {
            _health.TakeDamage(10f, DamageType.Explosive);
            Assert.AreEqual(35f, _health.CurrentShield, 0.001f);
        }

        [Test]
        public void ShieldAbsorbsFirst()
        {
            _health.TakeDamage(30f, DamageType.Physical);
            Assert.AreEqual(20f, _health.CurrentShield, 0.001f);
            Assert.AreEqual(100f, _health.CurrentHealth, 0.001f);
        }

        [Test]
        public void DamageOverflowsToHealth()
        {
            _health.TakeDamage(70f, DamageType.Physical);
            Assert.AreEqual(0f, _health.CurrentShield, 0.001f);
            Assert.AreEqual(80f, _health.CurrentHealth, 0.001f);
        }

        [Test]
        public void PerformanceDegradation_Below50()
        {
            _health.TakeDamage(50f, DamageType.Physical);
            _health.TakeDamage(60f, DamageType.Physical);

            Assert.AreEqual(40f, _health.CurrentHealth, 0.001f);
            Assert.AreEqual(0.8f, _health.SpeedMultiplier, 0.001f);
        }

        [Test]
        public void PerformanceDegradation_Below25()
        {
            _health.TakeDamage(50f, DamageType.Physical);
            _health.TakeDamage(80f, DamageType.Physical);

            Assert.AreEqual(20f, _health.CurrentHealth, 0.001f);
            Assert.AreEqual(0.6f, _health.SpeedMultiplier, 0.001f);
            Assert.AreEqual(0.7f, _health.HandlingMultiplier, 0.001f);
        }

        [Test]
        public void PerformanceDegradation_Above50_NoEffect()
        {
            _health.TakeDamage(50f, DamageType.Physical);
            _health.TakeDamage(20f, DamageType.Physical);

            Assert.AreEqual(80f, _health.CurrentHealth, 0.001f);
            Assert.AreEqual(1f, _health.SpeedMultiplier, 0.001f);
            Assert.AreEqual(1f, _health.HandlingMultiplier, 0.001f);
        }

        [Test]
        public void RestoreHealth_DoesNotExceedMax()
        {
            _health.TakeDamage(50f, DamageType.Physical);
            _health.TakeDamage(40f, DamageType.Physical);
            _health.RestoreHealth(1000f);

            Assert.AreEqual(100f, _health.CurrentHealth, 0.001f);
        }

        [Test]
        public void RestoreShield_DoesNotExceedMax()
        {
            _health.TakeDamage(10f, DamageType.Physical);
            _health.RestoreShield(1000f);

            Assert.AreEqual(50f, _health.CurrentShield, 0.001f);
        }

        [Test]
        public void SetDurabilityMultiplier_ScalesMaxValues()
        {
            _health.SetDurabilityMultiplier(1.5f);
            _health.RestoreToFull();

            Assert.AreEqual(150f, _health.MaxHealth, 0.001f);
            Assert.AreEqual(75f, _health.MaxShield, 0.001f);
            Assert.AreEqual(150f, _health.CurrentHealth, 0.001f);
            Assert.AreEqual(75f, _health.CurrentShield, 0.001f);
        }
    }
}
