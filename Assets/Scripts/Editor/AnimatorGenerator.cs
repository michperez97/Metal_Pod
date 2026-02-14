#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MetalPod.Editor
{
    public static class AnimatorGenerator
    {
        private const string ControllersRoot = "Assets/Animations/Controllers";
        private const string ClipsRoot = "Assets/Animations/Clips";
        private const string HovercraftClipsRoot = ClipsRoot + "/Hovercraft";
        private const string ProtagonistClipsRoot = ClipsRoot + "/Protagonist";
        private const string UiClipsRoot = ClipsRoot + "/UI";

        private const string HovercraftAnimatorPath = ControllersRoot + "/HovercraftAnimator.controller";
        private const string ProtagonistAnimatorPath = ControllersRoot + "/ProtagonistAnimator.controller";
        private const string UiCountdownAnimatorPath = ControllersRoot + "/UICountdownAnimator.controller";
        private const string UiMedalAnimatorPath = ControllersRoot + "/UIMedalAnimator.controller";
        private const string UiPanelAnimatorPath = ControllersRoot + "/UIPanelAnimator.controller";

        [MenuItem("Metal Pod/Animations/Generate All Animator Controllers")]
        public static void GenerateAll()
        {
            EditorUtility.DisplayProgressBar("Metal Pod Animations", "Generating clips...", 0.1f);
            try
            {
                EnsureDirectory(ControllersRoot);
                EnsureDirectory(HovercraftClipsRoot);
                EnsureDirectory(ProtagonistClipsRoot);
                EnsureDirectory(UiClipsRoot);

                GenerateAnimationClips();

                EditorUtility.DisplayProgressBar("Metal Pod Animations", "Generating hovercraft animator...", 0.4f);
                GenerateHovercraftAnimator();

                EditorUtility.DisplayProgressBar("Metal Pod Animations", "Generating protagonist animator...", 0.6f);
                GenerateProtagonistAnimator();

                EditorUtility.DisplayProgressBar("Metal Pod Animations", "Generating UI animators...", 0.8f);
                GenerateUIAnimators();

                EditorUtility.DisplayProgressBar("Metal Pod Animations", "Saving assets...", 1f);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        [MenuItem("Metal Pod/Animations/Generate Hovercraft Animator")]
        public static void GenerateHovercraftAnimator()
        {
            EnsureDirectory(ControllersRoot);
            EnsureDirectory(HovercraftClipsRoot);

            AnimatorController controller = CreateController(HovercraftAnimatorPath);
            controller.AddParameter("IsBoosting", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsDamaged", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsDestroyed", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsRespawning", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("HealthNormalized", AnimatorControllerParameterType.Float);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            sm.entryPosition = new Vector3(20f, 120f, 0f);
            sm.anyStatePosition = new Vector3(20f, 300f, 0f);

            AnimatorState idle = sm.AddState("Idle", new Vector3(280f, 120f, 0f));
            AnimatorState boosting = sm.AddState("Boosting", new Vector3(520f, 40f, 0f));
            AnimatorState damaged = sm.AddState("Damaged", new Vector3(520f, 200f, 0f));
            AnimatorState destroyed = sm.AddState("Destroyed", new Vector3(760f, 120f, 0f));
            AnimatorState respawning = sm.AddState("Respawning", new Vector3(1000f, 120f, 0f));

            sm.defaultState = idle;

            idle.motion = LoadClip(HovercraftClipsRoot + "/Hover_Idle.anim");
            boosting.motion = LoadClip(HovercraftClipsRoot + "/Hover_Boost.anim");
            damaged.motion = LoadClip(HovercraftClipsRoot + "/Hover_Damaged.anim");
            destroyed.motion = LoadClip(HovercraftClipsRoot + "/Hover_Destroyed.anim");
            respawning.motion = LoadClip(HovercraftClipsRoot + "/Hover_Respawn.anim");

            AddBoolTransition(idle, boosting, "IsBoosting", true, 0.15f);
            AddBoolTransition(boosting, idle, "IsBoosting", false, 0.15f);
            AddBoolTransition(damaged, idle, "IsDamaged", false, 0.15f);
            AddBoolTransition(destroyed, respawning, "IsRespawning", true, 0.15f);
            AddBoolTransition(respawning, idle, "IsRespawning", false, 0.5f);

            AddAnyBoolTransition(sm, damaged, "IsDamaged", true, 0.15f);
            AddAnyBoolTransition(sm, destroyed, "IsDestroyed", true, 0.15f);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Metal Pod/Animations/Generate Protagonist Animator")]
        public static void GenerateProtagonistAnimator()
        {
            EnsureDirectory(ControllersRoot);
            EnsureDirectory(ProtagonistClipsRoot);

            AnimatorController controller = CreateController(ProtagonistAnimatorPath);
            controller.AddParameter("Work", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Celebrate", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("IsWorking", AnimatorControllerParameterType.Bool);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            sm.entryPosition = new Vector3(20f, 130f, 0f);

            AnimatorState idle = sm.AddState("Idle", new Vector3(260f, 130f, 0f));
            AnimatorState working = sm.AddState("Working", new Vector3(500f, 60f, 0f));
            AnimatorState celebrating = sm.AddState("Celebrating", new Vector3(500f, 220f, 0f));
            sm.defaultState = idle;

            idle.motion = LoadClip(ProtagonistClipsRoot + "/Proto_Idle.anim");
            working.motion = LoadClip(ProtagonistClipsRoot + "/Proto_Working.anim");
            celebrating.motion = LoadClip(ProtagonistClipsRoot + "/Proto_Celebrating.anim");

            AddBoolTransition(idle, working, "IsWorking", true, 0.15f);
            AddBoolTransition(working, idle, "IsWorking", false, 0.15f);
            AddTriggerTransition(idle, working, "Work", 0.15f);
            AddTriggerTransition(idle, celebrating, "Celebrate", 0.1f);
            AddTriggerTransition(working, celebrating, "Celebrate", 0.1f);
            AddExitTimeTransition(celebrating, idle, 0.15f, 0.95f);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Metal Pod/Animations/Generate UI Animators")]
        public static void GenerateUIAnimators()
        {
            EnsureDirectory(ControllersRoot);
            EnsureDirectory(UiClipsRoot);

            GenerateUiCountdownAnimator();
            GenerateUiMedalAnimator();
            GenerateUiPanelAnimator();

            AssetDatabase.SaveAssets();
        }

        [MenuItem("Metal Pod/Animations/Generate Animation Clips")]
        public static void GenerateAnimationClips()
        {
            EnsureDirectory(HovercraftClipsRoot);
            EnsureDirectory(ProtagonistClipsRoot);
            EnsureDirectory(UiClipsRoot);

            SaveClip(CreateHoverIdleClip(), HovercraftClipsRoot + "/Hover_Idle.anim");
            SaveClip(CreateHoverBoostClip(), HovercraftClipsRoot + "/Hover_Boost.anim");
            SaveClip(CreateHoverDamagedClip(), HovercraftClipsRoot + "/Hover_Damaged.anim");
            SaveClip(CreateHoverDestroyedClip(), HovercraftClipsRoot + "/Hover_Destroyed.anim");
            SaveClip(CreateHoverRespawnClip(), HovercraftClipsRoot + "/Hover_Respawn.anim");

            SaveClip(CreateProtoIdleClip(), ProtagonistClipsRoot + "/Proto_Idle.anim");
            SaveClip(CreateProtoWorkingClip(), ProtagonistClipsRoot + "/Proto_Working.anim");
            SaveClip(CreateProtoCelebratingClip(), ProtagonistClipsRoot + "/Proto_Celebrating.anim");

            SaveClip(CreateUiSlideInRightClip(), UiClipsRoot + "/UI_SlideInRight.anim");
            SaveClip(CreateUiSlideOutRightClip(), UiClipsRoot + "/UI_SlideOutRight.anim");
            SaveClip(CreateUiFadeInClip(), UiClipsRoot + "/UI_FadeIn.anim");
            SaveClip(CreateUiFadeOutClip(), UiClipsRoot + "/UI_FadeOut.anim");
            SaveClip(CreateUiScalePopClip(), UiClipsRoot + "/UI_ScalePop.anim");
            SaveClip(CreateUiMedalRevealClip(), UiClipsRoot + "/UI_MedalReveal.anim");
            SaveClip(CreateUiCountdownPopClip(), UiClipsRoot + "/UI_CountdownPop.anim");
            SaveClip(CreateUiNumberCountUpClip(), UiClipsRoot + "/UI_NumberCountUp.anim");

            AssetDatabase.SaveAssets();
        }

        private static void GenerateUiCountdownAnimator()
        {
            AnimatorController controller = CreateController(UiCountdownAnimatorPath);
            controller.AddParameter("Pop", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            AnimatorState hidden = sm.AddState("Hidden", new Vector3(250f, 130f, 0f));
            AnimatorState pop = sm.AddState("Pop", new Vector3(490f, 130f, 0f));
            sm.defaultState = hidden;

            hidden.motion = LoadClip(UiClipsRoot + "/UI_FadeOut.anim");
            pop.motion = LoadClip(UiClipsRoot + "/UI_CountdownPop.anim");

            AddTriggerTransition(hidden, pop, "Pop", 0.05f);
            AddExitTimeTransition(pop, hidden, 0.05f, 0.95f);

            EditorUtility.SetDirty(controller);
        }

        private static void GenerateUiMedalAnimator()
        {
            AnimatorController controller = CreateController(UiMedalAnimatorPath);
            controller.AddParameter("Reveal", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            AnimatorState hidden = sm.AddState("Hidden", new Vector3(250f, 130f, 0f));
            AnimatorState reveal = sm.AddState("Reveal", new Vector3(490f, 130f, 0f));
            sm.defaultState = hidden;

            hidden.motion = LoadClip(UiClipsRoot + "/UI_FadeOut.anim");
            reveal.motion = LoadClip(UiClipsRoot + "/UI_MedalReveal.anim");

            AddTriggerTransition(hidden, reveal, "Reveal", 0.05f);

            EditorUtility.SetDirty(controller);
        }

        private static void GenerateUiPanelAnimator()
        {
            AnimatorController controller = CreateController(UiPanelAnimatorPath);
            controller.AddParameter("IsVisible", AnimatorControllerParameterType.Bool);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            AnimatorState hidden = sm.AddState("Hidden", new Vector3(250f, 130f, 0f));
            AnimatorState visible = sm.AddState("Visible", new Vector3(490f, 130f, 0f));
            sm.defaultState = hidden;

            hidden.motion = LoadClip(UiClipsRoot + "/UI_SlideOutRight.anim");
            visible.motion = LoadClip(UiClipsRoot + "/UI_SlideInRight.anim");

            AddBoolTransition(hidden, visible, "IsVisible", true, 0.3f);
            AddBoolTransition(visible, hidden, "IsVisible", false, 0.2f);

            EditorUtility.SetDirty(controller);
        }

        private static AnimatorController CreateController(string path)
        {
            EnsureDirectoryForAsset(path);
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            return AnimatorController.CreateAnimatorControllerAtPath(path);
        }

        private static AnimationClip LoadClip(string path)
        {
            EnsureDirectoryForAsset(path);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null)
            {
                return clip;
            }

            AnimationClip created = new AnimationClip();
            AssetDatabase.CreateAsset(created, path);
            return created;
        }

        private static void SaveClip(AnimationClip clip, string path)
        {
            EnsureDirectoryForAsset(path);
            AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            AssetDatabase.CreateAsset(clip, path);
            EditorUtility.SetDirty(clip);
        }

        private static void EnsureDirectoryForAsset(string assetPath)
        {
            string directory = Path.GetDirectoryName(assetPath);
            EnsureDirectory(string.IsNullOrEmpty(directory) ? "Assets" : directory.Replace("\\", "/"));
        }

        private static void EnsureDirectory(string assetDirectory)
        {
            if (AssetDatabase.IsValidFolder(assetDirectory))
            {
                return;
            }

            string[] parts = assetDirectory.Split('/');
            if (parts.Length == 0)
            {
                return;
            }

            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static AnimatorStateTransition AddBoolTransition(
            AnimatorState from,
            AnimatorState to,
            string parameter,
            bool value,
            float duration)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = duration;
            transition.canTransitionToSelf = false;
            transition.interruptionSource = TransitionInterruptionSource.None;
            transition.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, parameter);
            return transition;
        }

        private static AnimatorStateTransition AddTriggerTransition(
            AnimatorState from,
            AnimatorState to,
            string triggerParameter,
            float duration)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = duration;
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.If, 0f, triggerParameter);
            return transition;
        }

        private static AnimatorStateTransition AddExitTimeTransition(
            AnimatorState from,
            AnimatorState to,
            float duration,
            float exitTime)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = true;
            transition.exitTime = exitTime;
            transition.hasFixedDuration = true;
            transition.duration = duration;
            transition.canTransitionToSelf = false;
            return transition;
        }

        private static AnimatorStateTransition AddAnyBoolTransition(
            AnimatorStateMachine stateMachine,
            AnimatorState to,
            string parameter,
            bool value,
            float duration)
        {
            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(to);
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = duration;
            transition.canTransitionToSelf = false;
            transition.interruptionSource = TransitionInterruptionSource.None;
            transition.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, parameter);
            return transition;
        }

        private static AnimationClip CreateHoverIdleClip()
        {
            AnimationClip clip = CreateClip("Hover_Idle", true);
            clip.SetCurve("", typeof(Transform), "localPosition.y", Curve(
                Key(0f, 0f),
                Key(0.5f, 0.05f),
                Key(1.0f, 0f),
                Key(1.5f, -0.03f),
                Key(2.0f, 0f)));
            clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.z", Curve(
                Key(0f, 0f),
                Key(1.0f, 0.5f),
                Key(2.0f, 0f),
                Key(3.0f, -0.3f),
                Key(4.0f, 0f)));
            return clip;
        }

        private static AnimationClip CreateHoverBoostClip()
        {
            AnimationClip clip = CreateClip("Hover_Boost", true);
            clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.x", Curve(
                Key(0f, 0f),
                Key(0.2f, -5f),
                Key(0.5f, -4.2f),
                Key(0.8f, -5f)));
            clip.SetCurve("", typeof(Transform), "localPosition.y", Curve(
                Key(0f, 0f),
                Key(0.1f, 0.06f),
                Key(0.2f, -0.04f),
                Key(0.3f, 0f)));
            return clip;
        }

        private static AnimationClip CreateHoverDamagedClip()
        {
            AnimationClip clip = CreateClip("Hover_Damaged", true);
            clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.z", Curve(
                Key(0f, 0f),
                Key(0.05f, 2f),
                Key(0.1f, -1.5f),
                Key(0.15f, 1f),
                Key(0.2f, -0.5f),
                Key(0.25f, 0f)));
            return clip;
        }

        private static AnimationClip CreateHoverDestroyedClip()
        {
            AnimationClip clip = CreateClip("Hover_Destroyed", false);
            AnimationCurve scale = Curve(Key(0f, 1f), Key(0.1f, 0f));
            clip.SetCurve("", typeof(Transform), "localScale.x", scale);
            clip.SetCurve("", typeof(Transform), "localScale.y", scale);
            clip.SetCurve("", typeof(Transform), "localScale.z", scale);
            return clip;
        }

        private static AnimationClip CreateHoverRespawnClip()
        {
            AnimationClip clip = CreateClip("Hover_Respawn", false);
            clip.SetCurve("", typeof(Transform), "localScale.x", Curve(Key(0f, 0f), Key(0.3f, 1.1f), Key(0.5f, 1f)));
            clip.SetCurve("", typeof(Transform), "localScale.y", Curve(Key(0f, 0f), Key(0.3f, 1.1f), Key(0.5f, 1f)));
            clip.SetCurve("", typeof(Transform), "localScale.z", Curve(Key(0f, 0f), Key(0.3f, 1.1f), Key(0.5f, 1f)));
            return clip;
        }

        private static AnimationClip CreateProtoIdleClip()
        {
            AnimationClip clip = CreateClip("Proto_Idle", true);
            clip.SetCurve("", typeof(Transform), "localScale.y", Curve(
                Key(0f, 1.0f),
                Key(1.5f, 1.02f),
                Key(3f, 1.0f)));
            clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.y", Curve(
                Key(0f, 0f),
                Key(2f, 5f),
                Key(4f, 0f),
                Key(6f, -3f),
                Key(8f, 0f)));
            clip.SetCurve("", typeof(Transform), "localPosition.x", Curve(
                Key(0f, 0f),
                Key(2.5f, 0.02f),
                Key(5f, 0f)));
            return clip;
        }

        private static AnimationClip CreateProtoWorkingClip()
        {
            AnimationClip clip = CreateClip("Proto_Working", true);
            clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.x", Curve(
                Key(0f, 0f),
                Key(0.2f, 8f),
                Key(0.5f, 10f),
                Key(0.8f, 8f),
                Key(1f, 0f)));
            clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.z", Curve(
                Key(0f, -8f),
                Key(0.25f, 12f),
                Key(0.5f, -14f),
                Key(0.75f, 10f),
                Key(1f, -8f)));
            return clip;
        }

        private static AnimationClip CreateProtoCelebratingClip()
        {
            AnimationClip clip = CreateClip("Proto_Celebrating", false);
            clip.SetCurve("", typeof(Transform), "localPosition.y", Curve(
                Key(0f, 0f),
                Key(0.3f, 0f),
                Key(0.5f, 0.2f),
                Key(0.7f, 0f),
                Key(1.5f, 0f)));
            clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.z", Curve(
                Key(0f, 0f),
                Key(0.3f, -150f),
                Key(1f, 0f),
                Key(1.5f, 0f)));
            return clip;
        }

        private static AnimationClip CreateUiSlideInRightClip()
        {
            AnimationClip clip = CreateClip("UI_SlideInRight", false);
            clip.SetCurve("", typeof(RectTransform), "m_AnchoredPosition.x", Curve(
                Key(0f, 600f),
                Key(0.15f, -20f),
                Key(0.25f, 5f),
                Key(0.3f, 0f)));
            return clip;
        }

        private static AnimationClip CreateUiSlideOutRightClip()
        {
            AnimationClip clip = CreateClip("UI_SlideOutRight", false);
            clip.SetCurve("", typeof(RectTransform), "m_AnchoredPosition.x", Curve(
                Key(0f, 0f),
                Key(0.05f, -15f),
                Key(0.2f, 600f)));
            return clip;
        }

        private static AnimationClip CreateUiFadeInClip()
        {
            AnimationClip clip = CreateClip("UI_FadeIn", false);
            clip.SetCurve("", typeof(CanvasGroup), "m_Alpha", Curve(Key(0f, 0f), Key(0.3f, 1f)));
            return clip;
        }

        private static AnimationClip CreateUiFadeOutClip()
        {
            AnimationClip clip = CreateClip("UI_FadeOut", false);
            clip.SetCurve("", typeof(CanvasGroup), "m_Alpha", Curve(Key(0f, 1f), Key(0.2f, 0f)));
            return clip;
        }

        private static AnimationClip CreateUiScalePopClip()
        {
            AnimationClip clip = CreateClip("UI_ScalePop", false);
            AnimationCurve scale = Curve(Key(0f, 0f), Key(0.15f, 1.2f), Key(0.25f, 0.95f), Key(0.3f, 1f));
            clip.SetCurve("", typeof(RectTransform), "m_LocalScale.x", scale);
            clip.SetCurve("", typeof(RectTransform), "m_LocalScale.y", scale);
            clip.SetCurve("", typeof(RectTransform), "m_LocalScale.z", Curve(Key(0f, 1f), Key(0.3f, 1f)));
            return clip;
        }

        private static AnimationClip CreateUiMedalRevealClip()
        {
            AnimationClip clip = CreateClip("UI_MedalReveal", false);
            clip.SetCurve("", typeof(RectTransform), "m_LocalScale.x", Curve(
                Key(0f, 0f),
                Key(0.2f, 1.3f),
                Key(0.35f, 0.9f),
                Key(0.45f, 1f)));
            clip.SetCurve("", typeof(RectTransform), "m_LocalScale.y", Curve(
                Key(0f, 0f),
                Key(0.2f, 1.3f),
                Key(0.35f, 0.9f),
                Key(0.45f, 1f)));
            clip.SetCurve("", typeof(RectTransform), "localEulerAnglesRaw.y", Curve(Key(0f, 0f), Key(0.45f, 360f)));
            return clip;
        }

        private static AnimationClip CreateUiCountdownPopClip()
        {
            AnimationClip clip = CreateClip("UI_CountdownPop", false);
            clip.SetCurve("", typeof(RectTransform), "m_LocalScale.x", Curve(
                Key(0f, 2f),
                Key(0.1f, 1f),
                Key(0.4f, 1f),
                Key(0.5f, 0.5f)));
            clip.SetCurve("", typeof(RectTransform), "m_LocalScale.y", Curve(
                Key(0f, 2f),
                Key(0.1f, 1f),
                Key(0.4f, 1f),
                Key(0.5f, 0.5f)));
            clip.SetCurve("", typeof(CanvasGroup), "m_Alpha", Curve(
                Key(0f, 0f),
                Key(0.05f, 1f),
                Key(0.4f, 1f),
                Key(0.5f, 0f)));
            return clip;
        }

        private static AnimationClip CreateUiNumberCountUpClip()
        {
            AnimationClip clip = CreateClip("UI_NumberCountUp", false);
            clip.SetCurve("", typeof(CanvasGroup), "m_Alpha", Curve(
                Key(0f, 1f),
                Key(0.2f, 1f)));
            return clip;
        }

        private static AnimationClip CreateClip(string name, bool loop)
        {
            AnimationClip clip = new AnimationClip { name = name, frameRate = 60f };
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            settings.loopBlend = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            return clip;
        }

        private static Keyframe Key(float time, float value)
        {
            return new Keyframe(time, value);
        }

        private static AnimationCurve Curve(params Keyframe[] keys)
        {
            return new AnimationCurve(keys);
        }
    }
}
#endif
