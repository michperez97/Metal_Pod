#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace MetalPod.Editor
{
    /// <summary>
    /// Pre-build iOS settings configuration.
    /// </summary>
    public sealed class BuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS)
            {
                return;
            }

            Debug.Log("[BuildPreprocessor] Applying iOS build configuration...");

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.crocobyte.metalpod");
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.iOS.buildNumber = "1";

            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.statusBarHidden = true;

            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.iOS, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { GraphicsDeviceType.Metal });
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.stripEngineCode = true;

            TrySetAccelerometerFrequency(60);
            TrySetArchitectureArm64Only();
            TrySetScriptingBackendIL2CPP();
            TrySetScriptCallOptimizationFastNoExceptions();

            Debug.Log("[BuildPreprocessor] iOS build configuration complete.");
        }

        private static void TrySetAccelerometerFrequency(int hz)
        {
            PropertyInfo prop = typeof(PlayerSettings).GetProperty("accelerometerFrequency", BindingFlags.Public | BindingFlags.Static);
            if (prop == null || !prop.CanWrite)
            {
                return;
            }

            prop.SetValue(null, hz, null);
        }

        private static void TrySetArchitectureArm64Only()
        {
            MethodInfo byBuildTargetGroup = typeof(PlayerSettings).GetMethod(
                "SetArchitecture",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(BuildTargetGroup), typeof(int) },
                null);

            if (byBuildTargetGroup != null)
            {
                byBuildTargetGroup.Invoke(null, new object[] { BuildTargetGroup.iOS, 1 });
                return;
            }

            Type namedBuildTargetType = typeof(PlayerSettings).Assembly.GetType("UnityEditor.Build.NamedBuildTarget");
            if (namedBuildTargetType == null)
            {
                return;
            }

            MethodInfo byNamedBuildTarget = typeof(PlayerSettings).GetMethod(
                "SetArchitecture",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { namedBuildTargetType, typeof(int) },
                null);

            if (byNamedBuildTarget == null)
            {
                return;
            }

            object iosNamedBuildTarget = namedBuildTargetType.GetProperty("iOS", BindingFlags.Public | BindingFlags.Static)?.GetValue(null, null);
            if (iosNamedBuildTarget != null)
            {
                byNamedBuildTarget.Invoke(null, new[] { iosNamedBuildTarget, (object)1 });
            }
        }

        private static void TrySetScriptingBackendIL2CPP()
        {
            Type scriptingImplType = typeof(PlayerSettings).Assembly.GetType("UnityEditor.ScriptingImplementation");
            if (scriptingImplType == null)
            {
                return;
            }

            object il2CppValue = Enum.Parse(scriptingImplType, "IL2CPP");

            MethodInfo byBuildTargetGroup = typeof(PlayerSettings).GetMethod(
                "SetScriptingBackend",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(BuildTargetGroup), scriptingImplType },
                null);

            if (byBuildTargetGroup != null)
            {
                byBuildTargetGroup.Invoke(null, new[] { (object)BuildTargetGroup.iOS, il2CppValue });
                return;
            }

            Type namedBuildTargetType = typeof(PlayerSettings).Assembly.GetType("UnityEditor.Build.NamedBuildTarget");
            if (namedBuildTargetType == null)
            {
                return;
            }

            MethodInfo byNamedBuildTarget = typeof(PlayerSettings).GetMethod(
                "SetScriptingBackend",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { namedBuildTargetType, scriptingImplType },
                null);

            object iosNamedBuildTarget = namedBuildTargetType.GetProperty("iOS", BindingFlags.Public | BindingFlags.Static)?.GetValue(null, null);
            if (byNamedBuildTarget != null && iosNamedBuildTarget != null)
            {
                byNamedBuildTarget.Invoke(null, new[] { iosNamedBuildTarget, il2CppValue });
            }
        }

        private static void TrySetScriptCallOptimizationFastNoExceptions()
        {
            Type iosType = typeof(PlayerSettings).GetNestedType("iOS", BindingFlags.Public);
            if (iosType == null)
            {
                return;
            }

            PropertyInfo property = iosType.GetProperty("scriptCallOptimization", BindingFlags.Public | BindingFlags.Static);
            if (property == null || !property.CanWrite)
            {
                return;
            }

            object value = Enum.Parse(property.PropertyType, "FastButNoExceptions");
            property.SetValue(null, value, null);
        }
    }

    /// <summary>
    /// Post-process Xcode output to enforce plist keys and include privacy manifest.
    /// </summary>
    public sealed class BuildPostprocessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS)
            {
                return;
            }

#if UNITY_IOS
            string plistPath = Path.Combine(report.summary.outputPath, "Info.plist");
            if (File.Exists(plistPath))
            {
                PlistDocument plist = new PlistDocument();
                plist.ReadFromFile(plistPath);
                PlistElementDict root = plist.root;

                root.SetString("NSMotionUsageDescription", "Metal Pod uses device motion for tilt-based steering controls.");
                root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
                root.SetBoolean("UIRequiresFullScreen", true);

                PlistElementArray orientations = root.CreateArray("UISupportedInterfaceOrientations");
                orientations.values.Clear();
                orientations.AddString("UIInterfaceOrientationLandscapeLeft");
                orientations.AddString("UIInterfaceOrientationLandscapeRight");

                PlistElementArray ipadOrientations = root.CreateArray("UISupportedInterfaceOrientations~ipad");
                ipadOrientations.values.Clear();
                ipadOrientations.AddString("UIInterfaceOrientationLandscapeLeft");
                ipadOrientations.AddString("UIInterfaceOrientationLandscapeRight");

                plist.WriteToFile(plistPath);
                Debug.Log("[BuildPostprocessor] Updated Info.plist keys.");
            }

            string privacySource = Path.Combine(Application.dataPath, "Plugins/iOS/PrivacyInfo.xcprivacy");
            if (File.Exists(privacySource))
            {
                string privacyDest = Path.Combine(report.summary.outputPath, "PrivacyInfo.xcprivacy");
                File.Copy(privacySource, privacyDest, true);
                Debug.Log("[BuildPostprocessor] Copied PrivacyInfo.xcprivacy to Xcode output.");
            }
#endif
        }
    }
}
#endif
