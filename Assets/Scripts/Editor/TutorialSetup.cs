#if UNITY_EDITOR
using MetalPod.Tutorial;
using MetalPod.ScriptableObjects;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MetalPod.Editor
{
    public static class TutorialSetup
    {
        private const string TutorialSystemName = "TutorialSystem";
        private const string TutorialCanvasName = "TutorialCanvas";
        private const string TutorialAssetFolder = "Assets/ScriptableObjects/Data/Tutorial";

        [MenuItem("Metal Pod/Tutorial/Add Tutorial Triggers to Test Course")]
        public static void AddTutorialTriggers()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                Debug.LogWarning("[TutorialSetup] No active scene.");
                return;
            }

            TutorialManager manager = EnsureTutorialManager();
            TutorialUI tutorialUI = EnsureTutorialUI();

            if (manager != null && tutorialUI != null)
            {
                SerializedObject managerSerialized = new SerializedObject(manager);
                SerializedProperty uiProperty = managerSerialized.FindProperty("tutorialUI");
                if (uiProperty != null)
                {
                    uiProperty.objectReferenceValue = tutorialUI;
                    managerSerialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            Transform spawnPoint = FindSpawnPoint();
            Vector3 basePosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            Vector3 forward = spawnPoint != null ? spawnPoint.forward : Vector3.forward;

            AddOrUpdateTrigger(
                "TutorialTrigger_DamageWarning",
                "damage_warning",
                basePosition + (forward * 95f) + Vector3.up,
                new Vector3(14f, 4f, 8f));

            AddOrUpdateTrigger(
                "TutorialTrigger_Collectible",
                "collectible",
                basePosition + (forward * 150f) + Vector3.up,
                new Vector3(12f, 4f, 8f));

            EditorSceneManager.MarkSceneDirty(activeScene);
            Debug.Log("[TutorialSetup] Tutorial manager/UI/triggers added to active scene.");
        }

        [MenuItem("Metal Pod/Tutorial/Reset Tutorial Progress")]
        public static void ResetTutorialProgress()
        {
            TutorialSaveData.ResetAllTutorials();
            Debug.Log("[TutorialSetup] Tutorial progress reset.");
        }

        [MenuItem("Metal Pod/Tutorial/Create Default Tutorial Assets")]
        public static void CreateDefaultTutorialAssets()
        {
            EnsureFolderPath(TutorialAssetFolder);

            CreateSequenceAssets(TutorialSequence.CreateFirstPlaySequence());
            CreateSequenceAssets(TutorialSequence.CreateWorkshopIntroSequence());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[TutorialSetup] Default tutorial step assets generated.");
        }

        private static TutorialManager EnsureTutorialManager()
        {
            TutorialManager manager = Object.FindFirstObjectByType<TutorialManager>(FindObjectsInactive.Include);
            if (manager != null)
            {
                return manager;
            }

            GameObject root = new GameObject(TutorialSystemName);
            return root.AddComponent<TutorialManager>();
        }

        private static TutorialUI EnsureTutorialUI()
        {
            TutorialUI existing = Object.FindFirstObjectByType<TutorialUI>(FindObjectsInactive.Include);
            if (existing != null)
            {
                return existing;
            }

            GameObject canvasObject = new GameObject(TutorialCanvasName);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            canvasObject.AddComponent<GraphicRaycaster>();
            CanvasGroup canvasGroup = canvasObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();

            Image dimOverlay = CreateImage(
                "DimOverlay",
                canvasRect,
                new Color(0f, 0f, 0f, 0.55f),
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                false);

            Image promptPanelImage = CreateImage(
                "PromptPanel",
                canvasRect,
                new Color(0.08f, 0.1f, 0.14f, 0.92f),
                new Vector2(0.15f, 0.35f),
                new Vector2(0.85f, 0.65f),
                Vector2.zero,
                Vector2.zero,
                true);

            RectTransform promptRect = promptPanelImage.rectTransform;

            TextMeshProUGUI promptText = CreateTmpText(
                "PromptText",
                promptRect,
                "WELCOME TO METAL POD",
                54,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                Color.white,
                new Vector2(0.08f, 0.55f),
                new Vector2(0.92f, 0.92f));

            TextMeshProUGUI subtitleText = CreateTmpText(
                "SubtitleText",
                promptRect,
                "Tutorial subtitle.",
                30,
                FontStyles.Normal,
                TextAlignmentOptions.Center,
                new Color(0.85f, 0.9f, 1f),
                new Vector2(0.08f, 0.24f),
                new Vector2(0.92f, 0.56f));

            Image iconImage = CreateImage(
                "IconImage",
                promptRect,
                Color.white,
                new Vector2(0.04f, 0.7f),
                new Vector2(0.14f, 0.9f),
                Vector2.zero,
                Vector2.zero,
                false);

            TextMeshProUGUI tapText = CreateTmpText(
                "TapToContinueText",
                promptRect,
                "Tap to continue",
                24,
                FontStyles.Italic,
                TextAlignmentOptions.Center,
                new Color(1f, 0.85f, 0.4f),
                new Vector2(0.18f, 0.08f),
                new Vector2(0.82f, 0.2f));
            tapText.gameObject.SetActive(false);

            Image highlightRing = CreateImage(
                "HighlightRing",
                canvasRect,
                new Color(1f, 0.82f, 0.25f, 0.35f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(220f, 110f),
                false);
            highlightRing.raycastTarget = false;

            Image arrowPointer = CreateImage(
                "ArrowPointer",
                canvasRect,
                new Color(1f, 0.85f, 0.4f, 1f),
                new Vector2(0.8f, 0.2f),
                new Vector2(0.8f, 0.2f),
                Vector2.zero,
                new Vector2(52f, 52f),
                false);
            arrowPointer.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);

            TutorialUI tutorialUI = canvasObject.AddComponent<TutorialUI>();
            SerializedObject serializedUI = new SerializedObject(tutorialUI);
            SetProperty(serializedUI, "promptPanel", promptPanelImage.gameObject);
            SetProperty(serializedUI, "promptText", promptText);
            SetProperty(serializedUI, "subtitleText", subtitleText);
            SetProperty(serializedUI, "iconImage", iconImage);
            SetProperty(serializedUI, "tapToContinueText", tapText);
            SetProperty(serializedUI, "canvasGroup", canvasGroup);
            SetProperty(serializedUI, "promptRect", promptRect);
            SetProperty(serializedUI, "dimOverlay", dimOverlay);
            SetProperty(serializedUI, "highlightRing", highlightRing);
            SetProperty(serializedUI, "arrowPointer", arrowPointer);
            serializedUI.ApplyModifiedPropertiesWithoutUndo();

            return tutorialUI;
        }

        private static void AddOrUpdateTrigger(string objectName, string stepId, Vector3 worldPosition, Vector3 colliderSize)
        {
            GameObject triggerObject = GameObject.Find(objectName);
            if (triggerObject == null)
            {
                triggerObject = new GameObject(objectName);
            }

            triggerObject.transform.position = worldPosition;

            BoxCollider triggerCollider = triggerObject.GetComponent<BoxCollider>();
            if (triggerCollider == null)
            {
                triggerCollider = triggerObject.AddComponent<BoxCollider>();
            }

            triggerCollider.isTrigger = true;
            triggerCollider.size = colliderSize;

            TutorialTrigger trigger = triggerObject.GetComponent<TutorialTrigger>();
            if (trigger == null)
            {
                trigger = triggerObject.AddComponent<TutorialTrigger>();
            }

            SerializedObject serializedTrigger = new SerializedObject(trigger);
            SerializedProperty stepIdProperty = serializedTrigger.FindProperty("stepId");
            if (stepIdProperty != null)
            {
                stepIdProperty.stringValue = stepId;
            }

            SerializedProperty triggerOnceProperty = serializedTrigger.FindProperty("triggerOnce");
            if (triggerOnceProperty != null)
            {
                triggerOnceProperty.boolValue = true;
            }

            serializedTrigger.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform FindSpawnPoint()
        {
            GameObject namedSpawn = GameObject.Find("SpawnPoint");
            if (namedSpawn != null)
            {
                return namedSpawn.transform;
            }

            GameObject courseSpawn = GameObject.Find("Course/SpawnPoint");
            if (courseSpawn != null)
            {
                return courseSpawn.transform;
            }

            MetalPod.Hovercraft.HovercraftController player =
                Object.FindFirstObjectByType<MetalPod.Hovercraft.HovercraftController>(FindObjectsInactive.Include);
            return player != null ? player.transform : null;
        }

        private static Image CreateImage(
            string name,
            RectTransform parent,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            bool raycastTarget)
        {
            GameObject imageObject = new GameObject(name);
            imageObject.transform.SetParent(parent, false);

            RectTransform rect = imageObject.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        private static TextMeshProUGUI CreateTmpText(
            string name,
            RectTransform parent,
            string text,
            float fontSize,
            FontStyles style,
            TextAlignmentOptions alignment,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.enableWordWrapping = true;
            return tmp;
        }

        private static void SetProperty(SerializedObject serializedObject, string propertyName, Object value)
        {
            if (serializedObject == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return;
            }

            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static void CreateSequenceAssets(TutorialSequence sequence)
        {
            if (sequence?.steps == null)
            {
                return;
            }

            for (int i = 0; i < sequence.steps.Length; i++)
            {
                TutorialStep step = sequence.steps[i];
                if (step == null || string.IsNullOrWhiteSpace(step.stepId))
                {
                    continue;
                }

                string safeSequence = MakeSafeAssetName(sequence.sequenceId);
                string safeStep = MakeSafeAssetName(step.stepId);
                string assetPath = $"{TutorialAssetFolder}/{safeSequence}_{i + 1:00}_{safeStep}.asset";

                TutorialStepSO asset = AssetDatabase.LoadAssetAtPath<TutorialStepSO>(assetPath);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<TutorialStepSO>();
                    AssetDatabase.CreateAsset(asset, assetPath);
                }

                asset.step = CloneStep(step);
                EditorUtility.SetDirty(asset);
            }
        }

        private static TutorialStep CloneStep(TutorialStep source)
        {
            return new TutorialStep
            {
                stepId = source.stepId,
                promptText = source.promptText,
                subtitleText = source.subtitleText,
                iconSprite = source.iconSprite,
                completionCondition = source.completionCondition,
                conditionValue = source.conditionValue,
                autoAdvanceDelay = source.autoAdvanceDelay,
                pauseGame = source.pauseGame,
                slowMotion = source.slowMotion,
                requireTapToContinue = source.requireTapToContinue,
                highlightUIElement = source.highlightUIElement,
                promptPosition = source.promptPosition,
                showArrowPointing = source.showArrowPointing,
                arrowTarget = source.arrowTarget,
                dimBackground = source.dimBackground
            };
        }

        private static string MakeSafeAssetName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "tutorial";
            }

            foreach (char invalid in System.IO.Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '_');
            }

            return value.Replace(' ', '_');
        }

        private static void EnsureFolderPath(string fullPath)
        {
            string[] parts = fullPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
#endif
