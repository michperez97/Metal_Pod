using System;
using System.Collections.Generic;
using System.Reflection;

namespace MetalPod.Progression
{
    internal static class SharedBridge
    {
        private const string EventBusTypeName = "MetalPod.Shared.EventBus";
        private const string GameConstantsTypeName = "MetalPod.Shared.GameConstants";

        private static readonly Dictionary<string, float> FloatCache = new Dictionary<string, float>();
        private static readonly Dictionary<Action<string, float, int>, Delegate> CourseCompletedDelegates =
            new Dictionary<Action<string, float, int>, Delegate>();

        public static float MedalBonusBronze => GetFloatConstant("MEDAL_BONUS_BRONZE", 0.25f);
        public static float MedalBonusSilver => GetFloatConstant("MEDAL_BONUS_SILVER", 0.50f);
        public static float MedalBonusGold => GetFloatConstant("MEDAL_BONUS_GOLD", 1.00f);
        public static float ReplayRewardMultiplier => GetFloatConstant("REPLAY_REWARD_MULTIPLIER", 0.5f);

        public static void RaiseCurrencyChanged(int total)
        {
            InvokeEventBusMethod("RaiseCurrencyChanged", total);
        }

        public static void RaiseCurrencyEarned(int amount)
        {
            InvokeEventBusMethod("RaiseCurrencyEarned", amount);
        }

        public static void RaiseCourseUnlocked(string courseId)
        {
            InvokeEventBusMethod("RaiseCourseUnlocked", courseId);
        }

        public static void RaiseUpgradePurchased(string upgradeId, int level)
        {
            InvokeEventBusMethod("RaiseUpgradePurchased", upgradeId, level);
        }

        public static void RaiseCosmeticEquipped(string cosmeticId)
        {
            InvokeEventBusMethod("RaiseCosmeticEquipped", cosmeticId);
        }

        public static bool SubscribeCourseCompleted(Action<string, float, int> callback)
        {
            if (callback == null)
            {
                return false;
            }

            Type eventBusType = ResolveType(EventBusTypeName);
            EventInfo eventInfo = eventBusType?.GetEvent("OnCourseCompleted", BindingFlags.Public | BindingFlags.Static);
            if (eventInfo == null)
            {
                return false;
            }

            Delegate del = Delegate.CreateDelegate(eventInfo.EventHandlerType, callback.Target, callback.Method, false);
            if (del == null)
            {
                return false;
            }

            eventInfo.AddEventHandler(null, del);
            CourseCompletedDelegates[callback] = del;
            return true;
        }

        public static void UnsubscribeCourseCompleted(Action<string, float, int> callback)
        {
            if (callback == null)
            {
                return;
            }

            if (!CourseCompletedDelegates.TryGetValue(callback, out Delegate del))
            {
                return;
            }

            Type eventBusType = ResolveType(EventBusTypeName);
            EventInfo eventInfo = eventBusType?.GetEvent("OnCourseCompleted", BindingFlags.Public | BindingFlags.Static);
            if (eventInfo != null)
            {
                eventInfo.RemoveEventHandler(null, del);
            }

            CourseCompletedDelegates.Remove(callback);
        }

        private static void InvokeEventBusMethod(string methodName, params object[] args)
        {
            Type eventBusType = ResolveType(EventBusTypeName);
            if (eventBusType == null)
            {
                return;
            }

            MethodInfo[] methods = eventBusType.GetMethods(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.Name == methodName && method.GetParameters().Length == args.Length)
                {
                    method.Invoke(null, args);
                    return;
                }
            }
        }

        private static float GetFloatConstant(string fieldName, float fallback)
        {
            if (FloatCache.TryGetValue(fieldName, out float cached))
            {
                return cached;
            }

            float resolved = fallback;
            Type constantsType = ResolveType(GameConstantsTypeName);
            FieldInfo field = constantsType?.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
            if (field != null && field.FieldType == typeof(float))
            {
                resolved = (float)field.GetValue(null);
            }

            FloatCache[fieldName] = resolved;
            return resolved;
        }

        private static Type ResolveType(string typeName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type type = assemblies[i].GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
