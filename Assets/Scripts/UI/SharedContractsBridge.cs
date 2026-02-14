using System;
using System.Reflection;
using UnityEngine;

namespace MetalPod.UI
{
    internal static class SharedContractsBridge
    {
        private const string EventBusTypeName = "MetalPod.Shared.EventBus";
        private const string GameConstantsTypeName = "MetalPod.Shared.GameConstants";

        private static Type _eventBusType;
        private static Type _gameConstantsType;

        public static string SceneMainMenu => GetGameConstant("SCENE_MAIN_MENU", "MainMenu");
        public static string SceneWorkshop => GetGameConstant("SCENE_WORKSHOP", "Workshop");

        public static string PrefTiltSensitivity => GetGameConstant("PREF_TILT_SENSITIVITY", "TiltSensitivity");
        public static string PrefInvertTilt => GetGameConstant("PREF_INVERT_TILT", "InvertTilt");
        public static string PrefMasterVolume => GetGameConstant("PREF_MASTER_VOLUME", "MasterVolume");
        public static string PrefMusicVolume => GetGameConstant("PREF_MUSIC_VOLUME", "MusicVolume");
        public static string PrefSfxVolume => GetGameConstant("PREF_SFX_VOLUME", "SFXVolume");
        public static string PrefHapticsEnabled => GetGameConstant("PREF_HAPTICS_ENABLED", "HapticsEnabled");
        public static string PrefQualityLevel => GetGameConstant("PREF_QUALITY_LEVEL", "QualityLevel");

        private static Type EventBusType
        {
            get
            {
                if (_eventBusType == null)
                {
                    _eventBusType = Type.GetType(EventBusTypeName);
                }

                return _eventBusType;
            }
        }

        private static Type GameConstantsType
        {
            get
            {
                if (_gameConstantsType == null)
                {
                    _gameConstantsType = Type.GetType(GameConstantsTypeName);
                }

                return _gameConstantsType;
            }
        }

        public static bool SubscribeEvent(string eventName, Action handler)
        {
            return ModifyEventHandler(eventName, handler, true);
        }

        public static bool UnsubscribeEvent(string eventName, Action handler)
        {
            return ModifyEventHandler(eventName, handler, false);
        }

        public static bool SubscribeEvent<T>(string eventName, Action<T> handler)
        {
            return ModifyEventHandler(eventName, handler, true);
        }

        public static bool UnsubscribeEvent<T>(string eventName, Action<T> handler)
        {
            return ModifyEventHandler(eventName, handler, false);
        }

        public static bool SubscribeEvent<T1, T2>(string eventName, Action<T1, T2> handler)
        {
            return ModifyEventHandler(eventName, handler, true);
        }

        public static bool UnsubscribeEvent<T1, T2>(string eventName, Action<T1, T2> handler)
        {
            return ModifyEventHandler(eventName, handler, false);
        }

        public static bool SubscribeEvent<T1, T2, T3>(string eventName, Action<T1, T2, T3> handler)
        {
            return ModifyEventHandler(eventName, handler, true);
        }

        public static bool UnsubscribeEvent<T1, T2, T3>(string eventName, Action<T1, T2, T3> handler)
        {
            return ModifyEventHandler(eventName, handler, false);
        }

        public static void Raise(string methodName, params object[] args)
        {
            Type eventBusType = EventBusType;
            if (eventBusType == null || string.IsNullOrEmpty(methodName))
            {
                return;
            }

            MethodInfo method = eventBusType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                return;
            }

            try
            {
                method.Invoke(null, args);
            }
            catch
            {
                // Ignore invocation mismatch errors to keep UI runtime-safe.
            }
        }

        public static string GetGameConstant(string fieldName, string fallback)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return fallback;
            }

            Type gameConstantsType = GameConstantsType;
            if (gameConstantsType == null)
            {
                return fallback;
            }

            FieldInfo field = gameConstantsType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
            if (field == null || field.FieldType != typeof(string))
            {
                return fallback;
            }

            string value = field.GetValue(null) as string;
            return string.IsNullOrEmpty(value) ? fallback : value;
        }

        private static bool ModifyEventHandler(string eventName, Delegate handler, bool subscribe)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null)
            {
                return false;
            }

            Type eventBusType = EventBusType;
            if (eventBusType == null)
            {
                return false;
            }

            EventInfo eventInfo = eventBusType.GetEvent(eventName, BindingFlags.Public | BindingFlags.Static);
            if (eventInfo == null)
            {
                return false;
            }

            try
            {
                if (subscribe)
                {
                    eventInfo.AddEventHandler(null, handler);
                }
                else
                {
                    eventInfo.RemoveEventHandler(null, handler);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    internal static class ReflectionValueReader
    {
        public static float GetFloat(object target, string memberName, float fallback = 0f)
        {
            object raw = GetValue(target, memberName);
            if (raw == null)
            {
                return fallback;
            }

            if (raw is float floatValue)
            {
                return floatValue;
            }

            try
            {
                return Convert.ToSingle(raw);
            }
            catch
            {
                return fallback;
            }
        }

        public static int GetInt(object target, string memberName, int fallback = 0)
        {
            object raw = GetValue(target, memberName);
            if (raw == null)
            {
                return fallback;
            }

            if (raw is int intValue)
            {
                return intValue;
            }

            try
            {
                return Convert.ToInt32(raw);
            }
            catch
            {
                return fallback;
            }
        }

        public static bool GetBool(object target, string memberName, bool fallback = false)
        {
            object raw = GetValue(target, memberName);
            if (raw == null)
            {
                return fallback;
            }

            if (raw is bool boolValue)
            {
                return boolValue;
            }

            try
            {
                return Convert.ToBoolean(raw);
            }
            catch
            {
                return fallback;
            }
        }

        public static string GetString(object target, string memberName, string fallback = "")
        {
            object raw = GetValue(target, memberName);
            if (raw == null)
            {
                return fallback;
            }

            if (raw is string stringValue)
            {
                return stringValue;
            }

            return raw.ToString();
        }

        public static object Invoke(object target, string methodName, params object[] args)
        {
            if (target == null || string.IsNullOrEmpty(methodName))
            {
                return null;
            }

            Type type = target.GetType();
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
            {
                return null;
            }

            try
            {
                return method.Invoke(target, args);
            }
            catch
            {
                return null;
            }
        }

        public static bool HasMember(object target, string memberName)
        {
            if (target == null || string.IsNullOrEmpty(memberName))
            {
                return false;
            }

            Type type = target.GetType();
            return type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance) != null ||
                   type.GetField(memberName, BindingFlags.Public | BindingFlags.Instance) != null ||
                   type.GetMethod(memberName, BindingFlags.Public | BindingFlags.Instance) != null;
        }

        public static bool HasEvent(object target, string eventName)
        {
            if (target == null || string.IsNullOrEmpty(eventName))
            {
                return false;
            }

            return target.GetType().GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance) != null;
        }

        public static bool SubscribeEvent(object target, string eventName, Delegate handler)
        {
            if (target == null || string.IsNullOrEmpty(eventName) || handler == null)
            {
                return false;
            }

            EventInfo eventInfo = target.GetType().GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);
            if (eventInfo == null)
            {
                return false;
            }

            try
            {
                eventInfo.AddEventHandler(target, handler);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool UnsubscribeEvent(object target, string eventName, Delegate handler)
        {
            if (target == null || string.IsNullOrEmpty(eventName) || handler == null)
            {
                return false;
            }

            EventInfo eventInfo = target.GetType().GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);
            if (eventInfo == null)
            {
                return false;
            }

            try
            {
                eventInfo.RemoveEventHandler(target, handler);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static object GetValue(object target, string memberName)
        {
            if (target == null || string.IsNullOrEmpty(memberName))
            {
                return null;
            }

            Type type = target.GetType();
            PropertyInfo property = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
            {
                try
                {
                    return property.GetValue(target);
                }
                catch
                {
                    return null;
                }
            }

            FieldInfo field = type.GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                try
                {
                    return field.GetValue(target);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }
    }
}
