using System.Collections.Generic;
using NUnit.Framework;
using MetalPod.Shared;

namespace MetalPod.Tests.EditMode
{
    [TestFixture]
    public class GameConstantsTests
    {
        [Test]
        public void AllTagConstants_NotEmpty()
        {
            Assert.IsNotEmpty(GameConstants.TAG_PLAYER);
            Assert.IsNotEmpty(GameConstants.TAG_CHECKPOINT);
            Assert.IsNotEmpty(GameConstants.TAG_HAZARD);
            Assert.IsNotEmpty(GameConstants.TAG_COLLECTIBLE);
            Assert.IsNotEmpty(GameConstants.TAG_FINISH);
        }

        [Test]
        public void AllLayerConstants_NotEmpty()
        {
            Assert.IsNotEmpty(GameConstants.LAYER_HOVERCRAFT);
            Assert.IsNotEmpty(GameConstants.LAYER_GROUND);
            Assert.IsNotEmpty(GameConstants.LAYER_HAZARD);
            Assert.IsNotEmpty(GameConstants.LAYER_COLLECTIBLE);
        }

        [Test]
        public void SceneConstants_NotEmpty()
        {
            Assert.IsNotEmpty(GameConstants.SCENE_MAIN_MENU);
            Assert.IsNotEmpty(GameConstants.SCENE_WORKSHOP);
            Assert.IsNotEmpty(GameConstants.SCENE_PERSISTENT);
        }

        [Test]
        public void MedalBonuses_Increasing()
        {
            Assert.Greater(GameConstants.MEDAL_BONUS_SILVER, GameConstants.MEDAL_BONUS_BRONZE);
            Assert.Greater(GameConstants.MEDAL_BONUS_GOLD, GameConstants.MEDAL_BONUS_SILVER);
        }

        [Test]
        public void ReplayMultiplier_LessThanOne_AndGreaterThanZero()
        {
            Assert.Less(GameConstants.REPLAY_REWARD_MULTIPLIER, 1f);
            Assert.Greater(GameConstants.REPLAY_REWARD_MULTIPLIER, 0f);
        }

        [Test]
        public void HealthThresholds_Ordered()
        {
            Assert.Less(GameConstants.HEALTH_SPEED_THRESHOLD_25, GameConstants.HEALTH_SPEED_THRESHOLD_50);
        }

        [Test]
        public void PrefKeys_AreNonEmptyAndUnique()
        {
            var keys = new List<string>
            {
                GameConstants.PREF_TILT_SENSITIVITY,
                GameConstants.PREF_INVERT_TILT,
                GameConstants.PREF_MASTER_VOLUME,
                GameConstants.PREF_MUSIC_VOLUME,
                GameConstants.PREF_SFX_VOLUME,
                GameConstants.PREF_HAPTICS_ENABLED,
                GameConstants.PREF_QUALITY_LEVEL
            };

            foreach (string key in keys)
            {
                Assert.IsNotEmpty(key);
            }

            HashSet<string> unique = new HashSet<string>(keys);
            Assert.AreEqual(keys.Count, unique.Count);
        }
    }
}
