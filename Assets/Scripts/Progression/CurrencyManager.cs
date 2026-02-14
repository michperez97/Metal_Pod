using System;
using MetalPod.ScriptableObjects;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Progression
{
    public class CurrencyManager
    {
        private SaveSystem _saveSystem;

        public event Action<int> OnCurrencyChanged;

        public int Currency => _saveSystem != null && _saveSystem.CurrentData != null
            ? _saveSystem.CurrentData.currency
            : 0;

        public void Initialize(SaveSystem saveSystem)
        {
            _saveSystem = saveSystem;
            EventBus.RaiseCurrencyChanged(Currency);
            OnCurrencyChanged?.Invoke(Currency);
        }

        public int AwardCourseCompletion(
            CourseDataSO course,
            float time,
            int medal,
            int collectiblesFound,
            bool isFirstCompletion)
        {
            int baseReward = GetBaseReward(course);
            float completionMultiplier = isFirstCompletion ? 1.0f : GameConstants.REPLAY_REWARD_MULTIPLIER;

            float medalBonus = medal switch
            {
                3 => GameConstants.MEDAL_BONUS_GOLD,
                2 => GameConstants.MEDAL_BONUS_SILVER,
                1 => GameConstants.MEDAL_BONUS_BRONZE,
                _ => 0f
            };

            int collectibleBonus = Mathf.Max(0, collectiblesFound) * 10;

            int total = Mathf.RoundToInt(baseReward * completionMultiplier * (1f + medalBonus)) + collectibleBonus;
            AddCurrency(total);
            EventBus.RaiseCurrencyEarned(total);
            return total;
        }

        public bool CanAfford(int cost)
        {
            return cost <= Currency;
        }

        public bool SpendCurrency(int amount)
        {
            if (_saveSystem == null || _saveSystem.CurrentData == null || amount <= 0)
            {
                return false;
            }

            if (!CanAfford(amount))
            {
                return false;
            }

            _saveSystem.CurrentData.currency -= amount;
            _saveSystem.MarkDirty();
            _saveSystem.Save();
            NotifyCurrencyChanged();
            return true;
        }

        public void AddCurrency(int amount)
        {
            if (_saveSystem == null || _saveSystem.CurrentData == null || amount <= 0)
            {
                return;
            }

            _saveSystem.CurrentData.currency += amount;
            _saveSystem.MarkDirty();
            _saveSystem.Save();
            NotifyCurrencyChanged();
        }

        private int GetBaseReward(CourseDataSO course)
        {
            if (course == null)
            {
                return 100;
            }

            return course.difficulty switch
            {
                DifficultyLevel.Easy => 100,
                DifficultyLevel.Medium => 150,
                DifficultyLevel.Hard => 200,
                DifficultyLevel.Extreme => 300,
                _ => 100
            };
        }

        private void NotifyCurrencyChanged()
        {
            int total = Currency;
            EventBus.RaiseCurrencyChanged(total);
            OnCurrencyChanged?.Invoke(total);
        }
    }
}
