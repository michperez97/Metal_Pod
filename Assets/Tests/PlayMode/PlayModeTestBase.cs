using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MetalPod.Shared;
using NUnit.Framework;
using UnityEngine;

namespace MetalPod.Tests.PlayMode
{
    public abstract class PlayModeTestBase
    {
        private readonly List<GameObject> _createdObjects = new List<GameObject>();
        private readonly List<UnityEngine.Object> _createdAssets = new List<UnityEngine.Object>();

        [SetUp]
        public void BaseSetUp()
        {
            EventBus.Initialize();
        }

        [TearDown]
        public void BaseTearDown()
        {
            for (int i = _createdObjects.Count - 1; i >= 0; i--)
            {
                if (_createdObjects[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(_createdObjects[i]);
                }
            }

            for (int i = _createdAssets.Count - 1; i >= 0; i--)
            {
                if (_createdAssets[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(_createdAssets[i]);
                }
            }

            _createdObjects.Clear();
            _createdAssets.Clear();
            EventBus.Shutdown();
        }

        protected GameObject CreateTestObject(string name = "TestObject")
        {
            GameObject gameObject = new GameObject(name);
            _createdObjects.Add(gameObject);
            return gameObject;
        }

        protected T TrackObject<T>(T gameObject) where T : GameObject
        {
            if (gameObject != null)
            {
                _createdObjects.Add(gameObject);
            }

            return gameObject;
        }

        protected T TrackAsset<T>(T asset) where T : UnityEngine.Object
        {
            if (asset != null)
            {
                _createdAssets.Add(asset);
            }

            return asset;
        }

        protected IEnumerator WaitFrames(int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }
        }

        protected IEnumerator WaitFixedFrames(int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        protected IEnumerator WaitSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }

        protected static void SetPrivateField(object target, string fieldName, object value)
        {
            Assert.IsNotNull(target, "Target cannot be null.");
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing private field '{fieldName}' on {target.GetType().FullName}.");
            field.SetValue(target, value);
        }

        protected static T GetPrivateField<T>(object target, string fieldName)
        {
            Assert.IsNotNull(target, "Target cannot be null.");
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing private field '{fieldName}' on {target.GetType().FullName}.");
            return (T)field.GetValue(target);
        }

        protected static void SetAutoProperty<T>(object target, string propertyName, T value)
        {
            Assert.IsNotNull(target, "Target cannot be null.");
            string backingFieldName = $"<{propertyName}>k__BackingField";
            FieldInfo field = target.GetType().GetField(backingFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing auto-property backing field '{backingFieldName}' on {target.GetType().FullName}.");
            field.SetValue(target, value);
        }

        protected static object InvokeNonPublicMethod(object target, string methodName, params object[] args)
        {
            Assert.IsNotNull(target, "Target cannot be null.");
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Missing private method '{methodName}' on {target.GetType().FullName}.");
            return method.Invoke(target, args);
        }

        protected sealed class SaveFileIsolationScope : IDisposable
        {
            private readonly string _savePath;
            private readonly string _backupPath;
            private readonly bool _hadSave;
            private readonly bool _hadBackup;
            private readonly string _saveContent;
            private readonly string _backupContent;

            private bool _disposed;

            public SaveFileIsolationScope()
            {
                _savePath = Path.Combine(Application.persistentDataPath, "metalpod_save.json");
                _backupPath = Path.Combine(Application.persistentDataPath, "metalpod_save_backup.json");

                _hadSave = File.Exists(_savePath);
                _hadBackup = File.Exists(_backupPath);
                _saveContent = _hadSave ? File.ReadAllText(_savePath) : string.Empty;
                _backupContent = _hadBackup ? File.ReadAllText(_backupPath) : string.Empty;

                DeleteIfExists(_savePath);
                DeleteIfExists(_backupPath);
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                DeleteIfExists(_savePath);
                DeleteIfExists(_backupPath);

                if (_hadSave)
                {
                    File.WriteAllText(_savePath, _saveContent);
                }

                if (_hadBackup)
                {
                    File.WriteAllText(_backupPath, _backupContent);
                }

                _disposed = true;
            }

            private static void DeleteIfExists(string path)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}
