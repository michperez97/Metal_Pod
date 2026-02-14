# Codex Task 10: App Store Preparation

> **Goal**: Create all text-based assets, configuration files, and documentation needed for iOS App Store submission. This is a DOCUMENTATION + CODE task — generates metadata files, privacy manifest, accessibility config, and submission checklist.

---

## Context

Metal Pod targets **iOS 15+** via the App Store. Apple requires specific metadata, privacy declarations, app icons specs, screenshot specs, and review guidelines compliance. This task creates everything that can be prepared without Unity open — text metadata, the privacy manifest, accessibility helpers, a complete submission checklist, and editor scripts to generate required image assets.

**Read these files for requirements**:
- `IMPLEMENTATION_PLAN.md` — Task 5.7 (iOS Submission Preparation)
- `AGENT_1_CORE_INFRASTRUCTURE.md` — GameConstants (bundle ID patterns)
- `AGENT_4_PROGRESSION_AND_DATA.md` — SaveSystem (data storage details)
- `AGENT_5_UI_AND_WORKSHOP.md` — UI layout (for screenshot planning)
- `Assets/Scripts/Progression/SaveSystem.cs` — Verify data storage method
- `Assets/Scripts/Progression/SaveData.cs` — Verify what data is stored
- `Assets/Scripts/Core/GameManager.cs` — App lifecycle

---

## Files to Create

```
Docs/
├── APP_STORE_METADATA.md              # All App Store Connect text fields
├── APP_STORE_SUBMISSION_CHECKLIST.md   # Step-by-step submission guide
├── PRIVACY_POLICY.md                  # Privacy policy (hosted on web too)
├── SCREENSHOT_PLAN.md                 # Exact screenshot compositions per device
└── APP_REVIEW_NOTES.md               # Notes for Apple review team

Assets/
├── Plugins/iOS/
│   └── PrivacyInfo.xcprivacy          # Apple privacy manifest (XML)
│
├── Scripts/Accessibility/
│   ├── AccessibilityManager.cs        # VoiceOver + Dynamic Type support
│   ├── AccessibilityLabels.cs         # Static label strings for UI elements
│   └── HapticFeedbackManager.cs       # iOS Taptic Engine integration
│
├── Scripts/Editor/
│   ├── AppIconGenerator.cs            # Editor script to generate icon variants
│   └── BuildPreprocessor.cs           # Pre-build iOS config (capabilities, plist)
│
└── Scripts/Core/
    └── iOSNativePlugin.cs             # Native iOS bridge (haptics, review prompt, ATT)
```

---

## SECTION 1: APP STORE METADATA

Create `Docs/APP_STORE_METADATA.md` with ALL text fields needed for App Store Connect:

```markdown
# Metal Pod — App Store Metadata

## App Information
- **App Name**: Metal Pod
- **Subtitle**: Heavy Metal Hovercraft Racing
- **Bundle ID**: com.crocobyte.metalpod
- **SKU**: METALPOD001
- **Primary Language**: English (U.S.)
- **Category**: Games > Racing
- **Secondary Category**: Games > Action
- **Content Rating**: 9+ (Infrequent/Mild Cartoon or Fantasy Violence)

## Version Information
- **Version**: 1.0.0
- **Build**: 1
- **Copyright**: © 2026 Crocobyte. All rights reserved.

## App Store Description

### Promotional Text (170 chars max — can be updated without new build)
Race your customizable hovercraft through lava, ice, and toxic wastelands. Upgrade, customize, and earn gold medals in this heavy metal racing experience!

### Description (4000 chars max)
**METAL POD** — Strap into your hovercraft and blast through the most dangerous courses ever built.

TILT TO STEER. TAP TO BOOST. SURVIVE TO WIN.

Navigate through 9 brutal obstacle courses across three deadly environments:

🔥 LAVA ZONE — Dodge erupting geysers, flowing lava streams, and searing heat zones
❄️ ICE ZONE — Slide through blizzards, shatter ice walls, and outrun avalanches
☠️ TOXIC ZONE — Survive acid pools, electric fences, and exploding barrels

UPGRADE YOUR POD
Visit the Workshop to soup up your hovercraft with 4 upgrade categories:
• Speed — Push your top speed to the limit
• Armor — Take more hits before you're toast
• Handling — Tighter turns, better control
• Boost — Longer, more powerful bursts

CUSTOMIZE YOUR RIDE
Unlock paint jobs, decals, and parts to make your Metal Pod uniquely yours. Earn rewards by completing courses and chasing gold medals.

MEDAL CHASE
Every course has Gold, Silver, and Bronze time targets. Beat them all to prove you're the ultimate pilot. Unlock new courses as you progress through increasingly dangerous terrain.

FEATURES:
• Physics-based hovercraft controls with tilt steering
• 9 unique courses across 3 environments
• 15 different hazard types
• Full upgrade system with visible stat changes
• Cosmetic customization (colors, decals, parts)
• Medal-based progression system
• Heavy metal aesthetic throughout
• Optimized for iPhone and iPad

No ads. No in-app purchases. Just pure racing.

### Keywords (100 chars max, comma-separated)
hovercraft,racing,obstacle,course,metal,upgrade,customize,physics,tilt,dodge

### What's New (for v1.0.0)
Initial release! Race through 9 courses, upgrade your hovercraft, and chase gold medals.

### Support URL
https://crocobyte.com/metalpod/support

### Marketing URL
https://crocobyte.com/metalpod

### Privacy Policy URL
https://crocobyte.com/metalpod/privacy
```

**IMPORTANT**: The agent should fill in placeholder URLs but note they need to be replaced with real URLs before submission.

---

## SECTION 2: PRIVACY MANIFEST (PrivacyInfo.xcprivacy)

Create `Assets/Plugins/iOS/PrivacyInfo.xcprivacy` — Apple's required privacy manifest.

Metal Pod does NOT:
- Collect any user data
- Use tracking/advertising frameworks
- Access contacts, location, camera, microphone, photos
- Use any third-party analytics SDKs
- Have in-app purchases
- Have user accounts

Metal Pod DOES:
- Save game progress to `Application.persistentDataPath` (local JSON file)
- Use `UserDefaults` for settings (volume, sensitivity) via PlayerPrefs
- Access accelerometer for tilt controls (CoreMotion)
- Use Taptic Engine for haptic feedback

The privacy manifest should declare:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>NSPrivacyTracking</key>
    <false/>
    <key>NSPrivacyTrackingDomains</key>
    <array/>
    <key>NSPrivacyCollectedDataTypes</key>
    <array/>
    <key>NSPrivacyAccessedAPITypes</key>
    <array>
        <dict>
            <key>NSPrivacyAccessedAPIType</key>
            <string>NSPrivacyAccessedAPICategoryUserDefaults</string>
            <key>NSPrivacyAccessedAPITypeReasons</key>
            <array>
                <string>CA92.1</string>
            </array>
        </dict>
        <dict>
            <key>NSPrivacyAccessedAPIType</key>
            <string>NSPrivacyAccessedAPICategoryFileTimestamp</string>
            <key>NSPrivacyAccessedAPITypeReasons</key>
            <array>
                <string>C617.1</string>
            </array>
        </dict>
    </array>
</dict>
</plist>
```

The reason codes:
- `CA92.1` — UserDefaults access for app functionality (settings storage)
- `C617.1` — File timestamp access for save file management

---

## SECTION 3: PRIVACY POLICY

Create `Docs/PRIVACY_POLICY.md`:

```markdown
# Privacy Policy — Metal Pod

**Last updated**: [INSERT DATE]
**Developer**: Crocobyte

## Overview
Metal Pod is a single-player offline racing game. We do not collect, store, or transmit any personal data.

## Data We Do NOT Collect
- Personal information (name, email, phone)
- Location data
- Device identifiers for advertising
- Analytics or usage data
- Health or fitness data
- Financial information
- Contact information
- Browsing history

## Local Data Storage
Metal Pod stores the following data ONLY on your device:
- **Game progress**: Course completion, medals earned, currency balance, upgrades purchased, cosmetics unlocked
- **Settings**: Volume levels, control sensitivity, graphics preferences

This data is stored locally using standard iOS file storage and is never transmitted to any server.

## Third-Party Services
Metal Pod does not integrate any third-party services, SDKs, or analytics frameworks.

## Children's Privacy
Metal Pod does not collect any data from any user, including children under 13.

## Changes to This Policy
If we update this privacy policy, the new version will be posted at [PRIVACY_URL] and the "Last updated" date will be revised.

## Contact
For questions about this privacy policy, contact: [SUPPORT_EMAIL]
```

---

## SECTION 4: APP REVIEW NOTES

Create `Docs/APP_REVIEW_NOTES.md`:

```markdown
# Notes for App Store Review Team

## App Description
Metal Pod is a single-player offline hovercraft racing game. Players navigate obstacle courses using tilt controls and touch input.

## How to Test
1. Launch the app — you'll see the Main Menu
2. Tap "Play" to enter the Workshop
3. Select "Inferno Gate" (first course, unlocked by default)
4. Tap "Launch" to start racing
5. Tilt device to steer, tap right side to boost, left side to brake
6. Complete the course to earn currency and medals
7. Return to Workshop to upgrade your hovercraft or customize appearance

## Key Points
- **No login required** — game is fully offline, no account creation
- **No in-app purchases** — all content is earned through gameplay
- **No ads** — completely ad-free experience
- **No network access required** — works fully offline
- **Accelerometer used** — tilt controls require physical device (not Simulator)

## Demo Account
Not applicable — no accounts or login system.

## Hardware Requirements
- Accelerometer required for tilt steering (recommend testing on physical device)
- Haptic feedback uses Taptic Engine (iPhone 7+)

## Content Rating Justification
- Rated 9+ for Infrequent/Mild Cartoon or Fantasy Violence
- Violence consists of: hovercraft taking damage from environmental hazards (lava, ice, explosions)
- No violence against characters/people
- No blood, gore, or realistic violence
- Protagonist character in Workshop is never harmed
```

---

## SECTION 5: SCREENSHOT PLAN

Create `Docs/SCREENSHOT_PLAN.md`:

```markdown
# Metal Pod — Screenshot Plan

## Required Device Sizes (App Store Connect)

### iPhone
1. **6.9" Display** (iPhone 16 Pro Max) — 1320 x 2868 px
2. **6.5" Display** (iPhone 15 Plus/14 Pro Max) — 1290 x 2796 px
3. **5.5" Display** (iPhone 8 Plus) — 1242 x 2208 px

### iPad
4. **13" Display** (iPad Pro 13") — 2064 x 2752 px
5. **12.9" Display** (iPad Pro 12.9" 2nd gen) — 2048 x 2732 px

## Screenshot Compositions (up to 10 per device size)

### Screenshot 1 — Hero Shot
- **Scene**: Hovercraft racing through Lava environment
- **Content**: Hovercraft mid-boost with thruster flames, lava geyser erupting nearby
- **Text Overlay**: "METAL POD" (large, top), "Heavy Metal Hovercraft Racing" (subtitle)
- **Captures**: Core gameplay, visual style, heavy metal aesthetic

### Screenshot 2 — Tilt Controls
- **Scene**: Ice environment, hovercraft banking through a turn
- **Content**: Show the tilt-to-steer mechanic in action, blizzard particles
- **Text Overlay**: "TILT TO STEER" (top), "Intuitive physics-based controls" (bottom)
- **Captures**: Control scheme, ice environment

### Screenshot 3 — Hazard Variety
- **Scene**: Toxic environment with multiple hazards visible
- **Content**: Acid pools, electric fences, exploding barrels — show danger
- **Text Overlay**: "SURVIVE 15 HAZARD TYPES" (top), "Across 3 deadly environments" (bottom)
- **Captures**: Hazard variety, challenge factor

### Screenshot 4 — Workshop Upgrades
- **Scene**: Workshop with Upgrade panel open
- **Content**: Hovercraft on display, upgrade categories visible, stat bars
- **Text Overlay**: "UPGRADE YOUR POD" (top), "Speed, Armor, Handling & Boost" (bottom)
- **Captures**: Progression depth, upgrade system

### Screenshot 5 — Customization
- **Scene**: Workshop with Customization panel, color picker or decal grid
- **Content**: Hovercraft with custom paint job, decal options visible
- **Text Overlay**: "MAKE IT YOURS" (top), "Colors, Decals & Parts" (bottom)
- **Captures**: Personalization, cosmetic variety

### Screenshot 6 — Medal Results
- **Scene**: Results screen showing Gold medal earned
- **Content**: Medal animation, time display, currency earned, star rating
- **Text Overlay**: "CHASE GOLD" (top), "Earn medals across 9 courses" (bottom)
- **Captures**: Progression system, replayability hook

### Screenshot 7 — Course Selection
- **Scene**: Course Selection panel in Workshop
- **Content**: Grid of courses with Lava/Ice/Toxic environments, lock icons, medal icons
- **Text Overlay**: "9 BRUTAL COURSES" (top), "3 deadly environments" (bottom)
- **Captures**: Content breadth, progression

### Screenshot 8 — Lava Environment
- **Scene**: Gameplay in Lava Course 3 ("Magma Canyon")
- **Content**: Volcanic eruptions, lava flows, heat distortion shader visible
- **Text Overlay**: "LAVA ZONE" (centered, large)
- **Captures**: Environment showcase

### Screenshot 9 — Ice Environment
- **Scene**: Gameplay in Ice Course 2 ("Glacial Ravine")
- **Content**: Avalanche in background, ice patches, frost overlay
- **Text Overlay**: "ICE ZONE" (centered, large)
- **Captures**: Environment showcase

### Screenshot 10 — Toxic Environment
- **Scene**: Gameplay in Toxic Course 3 ("Biohazard Core")
- **Content**: Toxic gas clouds, electric arcs, industrial presses
- **Text Overlay**: "TOXIC ZONE" (centered, large)
- **Captures**: Environment showcase

## App Preview Video (30 seconds)
- 0-3s: Metal Pod logo slam with heavy metal music
- 3-8s: Lava course gameplay (boost through geyser field)
- 8-13s: Ice course gameplay (drift past avalanche)
- 13-18s: Toxic course gameplay (dodge electric fences and barrels)
- 18-22s: Workshop upgrades (quick stat upgrade montage)
- 22-26s: Customization (paint job change + decal apply)
- 26-28s: Gold medal results screen
- 28-30s: "Download Now" + App Store badge

## App Icon Notes
- 1024x1024 PNG, no alpha channel
- Design: Front-view of Metal Pod hovercraft with thruster glow
- Background: Dark gradient (charcoal to black) with subtle metal texture
- No text on icon (Apple guideline)
- No rounded corners in source (iOS adds them automatically)
```

---

## SECTION 6: SUBMISSION CHECKLIST

Create `Docs/APP_STORE_SUBMISSION_CHECKLIST.md`:

```markdown
# Metal Pod — App Store Submission Checklist

## Phase 1: Pre-Build Preparation

### Xcode Project Settings
- [ ] Bundle Identifier: `com.crocobyte.metalpod`
- [ ] Version: `1.0.0`
- [ ] Build Number: `1`
- [ ] Deployment Target: iOS 15.0
- [ ] Device Family: iPhone + iPad (Universal)
- [ ] Supported Orientations: Landscape Left, Landscape Right
- [ ] Requires Full Screen: YES
- [ ] Status Bar Style: Hidden
- [ ] App Uses Non-Exempt Encryption: NO (add ITSAppUsesNonExemptEncryption = NO to Info.plist)

### Capabilities
- [ ] Game Center: OFF (not used in v1.0)
- [ ] In-App Purchase: OFF (not used)
- [ ] Push Notifications: OFF (not used)

### Info.plist Required Keys
- [ ] `NSMotionUsageDescription`: "Metal Pod uses device motion for tilt-based steering controls."
- [ ] `ITSAppUsesNonExemptEncryption`: `false`
- [ ] `UIRequiresFullScreen`: `true`
- [ ] `UISupportedInterfaceOrientations`: `UIInterfaceOrientationLandscapeLeft`, `UIInterfaceOrientationLandscapeRight`

### Privacy
- [ ] PrivacyInfo.xcprivacy included in Xcode project
- [ ] Privacy policy hosted at public URL
- [ ] App Store Connect privacy questions answered (all "No")

## Phase 2: Unity Build Configuration

### Player Settings (Unity)
- [ ] Company Name: Crocobyte
- [ ] Product Name: Metal Pod
- [ ] Default Orientation: Landscape Left
- [ ] Auto Graphics API: NO → Metal only
- [ ] Color Space: Linear
- [ ] Scripting Backend: IL2CPP
- [ ] Architecture: ARM64
- [ ] Target minimum iOS Version: 15.0
- [ ] Accelerometer Frequency: 60 Hz
- [ ] Requires ARKit: NO
- [ ] Requires Persistent WiFi: NO
- [ ] Allow downloads over HTTP: NO

### Build Optimization
- [ ] Strip Engine Code: YES
- [ ] Managed Stripping Level: Medium (test thoroughly)
- [ ] Script Call Optimization: Fast but no exceptions
- [ ] Texture Compression: ASTC (4x4 for quality, 6x6 for size)
- [ ] Audio: Vorbis compression, quality 70%
- [ ] Disable unused Unity modules (Analytics, Ads, AR, VR, XR)

### Scenes in Build
- [ ] 0: _Persistent
- [ ] 1: MainMenu
- [ ] 2: Workshop
- [ ] 3: TestCourse
- [ ] 4-6: Lava courses (InfernoGate, MoltenRidge, MagmaCanyon)
- [ ] 7-9: Ice courses (FrozenPass, GlacialRavine, ArcticStorm)
- [ ] 10-12: Toxic courses (RustValley, ChemicalPlant, BiohazardCore)

## Phase 3: Testing

### Device Testing
- [ ] Test on physical iPhone (accelerometer/haptics don't work in Simulator)
- [ ] Test on oldest supported device (iPhone 6s / iOS 15)
- [ ] Test on latest device (iPhone 16 Pro Max)
- [ ] Test on iPad (layout, touch zones scale correctly)
- [ ] Verify 60fps on target devices
- [ ] Test all 9 courses complete without crashes
- [ ] Test save/load across app kill and restart
- [ ] Test all upgrade purchases and cosmetic equips
- [ ] Test tutorial plays on first launch, skippable on subsequent
- [ ] Test all 15 hazard types deal damage correctly
- [ ] Test medal thresholds are achievable but challenging

### Edge Cases
- [ ] App backgrounding during race (pause correctly)
- [ ] Low battery / thermal throttling (frame rate degrades gracefully)
- [ ] Interruptions: phone call, notification, Control Center
- [ ] No crash on rapid scene transitions
- [ ] Save data corruption recovery (reset to defaults)
- [ ] First launch with no save data
- [ ] All courses locked except first on fresh install

## Phase 4: App Store Connect

### App Information
- [ ] Create app in App Store Connect
- [ ] Fill in all metadata from APP_STORE_METADATA.md
- [ ] Upload app icon (1024x1024, no alpha, no rounded corners)
- [ ] Upload screenshots for all required device sizes
- [ ] Upload app preview video (30 sec, H.264, AAC audio)
- [ ] Set age rating (questionnaire: Infrequent/Mild Cartoon Violence)
- [ ] Set pricing: Free (or chosen price tier)
- [ ] Set availability: All territories (or specific selection)

### Review Information
- [ ] Add review notes from APP_REVIEW_NOTES.md
- [ ] Contact information for reviewer
- [ ] No demo account needed (offline game)

### Privacy
- [ ] Answer data collection questions: "We do not collect data"
- [ ] Provide privacy policy URL
- [ ] No tracking transparency required (no tracking)

## Phase 5: Build & Submit

- [ ] Archive build in Xcode (Product > Archive)
- [ ] Validate archive (no errors/warnings)
- [ ] Upload to App Store Connect via Xcode Organizer
- [ ] Wait for build processing (~15-30 minutes)
- [ ] Select build in App Store Connect
- [ ] Submit for review
- [ ] Monitor review status (typical: 24-48 hours)

## Phase 6: Post-Submission

- [ ] Prepare marketing materials
- [ ] Set up support email / webpage
- [ ] Plan v1.1 features based on user feedback
- [ ] Monitor crash reports in Xcode Organizer
- [ ] Respond to App Store reviews
```

---

## SECTION 7: C# SCRIPTS

### 7.1 AccessibilityManager.cs

Create `Assets/Scripts/Accessibility/AccessibilityManager.cs`:

```csharp
// Manages VoiceOver labels and Dynamic Type scaling for iOS accessibility.
// Integrates with Unity UI elements to provide accessible descriptions.

using UnityEngine;

namespace MetalPod.Accessibility
{
    /// <summary>
    /// Centralized accessibility manager for iOS VoiceOver and Dynamic Type support.
    /// Attach to a persistent GameObject (e.g., GameManager).
    /// </summary>
    public class AccessibilityManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _enableVoiceOverSupport = true;
        [SerializeField] private bool _enableDynamicType = true;
        [SerializeField] private float _minimumFontScale = 0.8f;
        [SerializeField] private float _maximumFontScale = 1.5f;

        public static AccessibilityManager Instance { get; private set; }

        public bool IsVoiceOverRunning { get; private set; }
        public float FontScale { get; private set; } = 1.0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            UpdateAccessibilityState();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
                UpdateAccessibilityState();
        }

        /// <summary>
        /// Query iOS for current accessibility settings.
        /// </summary>
        private void UpdateAccessibilityState()
        {
#if UNITY_IOS && !UNITY_EDITOR
            IsVoiceOverRunning = iOSNativePlugin.IsVoiceOverRunning();
            if (_enableDynamicType)
            {
                float nativeScale = iOSNativePlugin.GetPreferredFontScale();
                FontScale = Mathf.Clamp(nativeScale, _minimumFontScale, _maximumFontScale);
            }
#else
            IsVoiceOverRunning = false;
            FontScale = 1.0f;
#endif
        }

        /// <summary>
        /// Announce a message via VoiceOver (e.g., "Gold Medal Earned!").
        /// </summary>
        public void Announce(string message)
        {
            if (!_enableVoiceOverSupport || !IsVoiceOverRunning) return;
#if UNITY_IOS && !UNITY_EDITOR
            iOSNativePlugin.PostVoiceOverAnnouncement(message);
#endif
        }

        /// <summary>
        /// Set the accessibility label for a UI element's native view.
        /// </summary>
        public void SetLabel(GameObject uiElement, string label)
        {
            if (!_enableVoiceOverSupport) return;
            // Store label for VoiceOver; in practice this requires a native plugin
            // to map Unity UI elements to iOS accessibility elements.
            // For now, store as component data that native bridge can query.
            var labelComponent = uiElement.GetComponent<AccessibilityLabel>();
            if (labelComponent == null)
                labelComponent = uiElement.AddComponent<AccessibilityLabel>();
            labelComponent.Label = label;
        }
    }

    /// <summary>
    /// Attach to any UI GameObject to provide a VoiceOver label.
    /// </summary>
    public class AccessibilityLabel : MonoBehaviour
    {
        [TextArea] public string Label;
        [TextArea] public string Hint;
        public bool IsButton;
    }
}
```

### 7.2 AccessibilityLabels.cs

Create `Assets/Scripts/Accessibility/AccessibilityLabels.cs`:

```csharp
// Static class containing all VoiceOver label strings for Metal Pod UI elements.
// Centralized here for localization readiness and consistency.

namespace MetalPod.Accessibility
{
    public static class AccessibilityLabels
    {
        // Main Menu
        public const string PlayButton = "Play. Double tap to enter the Workshop.";
        public const string SettingsButton = "Settings. Double tap to open settings.";
        public const string QuitButton = "Quit. Double tap to exit the game.";

        // Workshop
        public const string CoursesTab = "Courses tab. Double tap to browse race courses.";
        public const string UpgradesTab = "Upgrades tab. Double tap to upgrade your hovercraft.";
        public const string CustomizeTab = "Customize tab. Double tap to customize appearance.";
        public const string LaunchButton = "Launch race. Double tap to start the selected course.";
        public const string BackButton = "Back. Double tap to return to the previous screen.";

        // HUD
        public const string SpeedDisplay = "Current speed: {0} km/h";
        public const string HealthBar = "Health: {0} percent";
        public const string ShieldBar = "Shield: {0} percent";
        public const string BoostBar = "Boost: {0} percent";
        public const string TimerDisplay = "Race time: {0}";
        public const string CheckpointDisplay = "Checkpoint {0} of {1}";
        public const string CurrencyDisplay = "Bolts: {0}";

        // Course Selection
        public const string CourseCard = "{0}. {1} environment. Best time: {2}. Medal: {3}.";
        public const string LockedCourse = "{0}. Locked. Requires: {1}.";

        // Upgrades
        public const string UpgradeCategory = "{0}. Level {1} of 5. Cost to upgrade: {2} bolts.";
        public const string UpgradeMaxed = "{0}. Maximum level reached.";
        public const string PurchaseUpgrade = "Purchase {0} upgrade for {1} bolts. Double tap to confirm.";

        // Cosmetics
        public const string CosmeticItem = "{0}. {1}.";
        public const string CosmeticLocked = "{0}. Locked. Price: {1} bolts.";
        public const string CosmeticEquipped = "{0}. Currently equipped.";

        // Results
        public const string RaceComplete = "Race complete! Time: {0}. Medal: {1}. Bolts earned: {2}.";
        public const string NewRecord = "New record! Previous best: {0}.";

        // Medals
        public const string GoldMedal = "Gold medal";
        public const string SilverMedal = "Silver medal";
        public const string BronzeMedal = "Bronze medal";
        public const string NoMedal = "No medal";

        // Announcements (for VoiceOver notifications)
        public const string CountdownAnnounce = "{0}";  // "3", "2", "1", "GO!"
        public const string CheckpointReached = "Checkpoint reached. {0} of {1}.";
        public const string DamageTaken = "Damage taken. Health at {0} percent.";
        public const string MedalEarned = "{0} earned!";
        public const string CourseUnlocked = "New course unlocked: {0}!";
    }
}
```

### 7.3 HapticFeedbackManager.cs

Create `Assets/Scripts/Accessibility/HapticFeedbackManager.cs`:

```csharp
// iOS Taptic Engine integration for Metal Pod.
// Provides haptic feedback on boost, damage, collisions, and UI interactions.
// SRS requirement: "Provide haptic feedback via iOS Taptic Engine on boost, damage, collisions"

using UnityEngine;

namespace MetalPod.Accessibility
{
    /// <summary>
    /// Manages haptic feedback using iOS Taptic Engine.
    /// Wraps native calls with a clean API and user-configurable intensity.
    /// </summary>
    public class HapticFeedbackManager : MonoBehaviour
    {
        public enum HapticType
        {
            Light,      // UI tap, collectible pickup
            Medium,     // Boost activation, checkpoint
            Heavy,      // Collision, taking damage
            Success,    // Medal earned, course complete
            Warning,    // Low health, hazard proximity
            Error       // Destruction, failed action
        }

        [Header("Settings")]
        [SerializeField] private bool _hapticsEnabled = true;
        [SerializeField] [Range(0f, 1f)] private float _hapticIntensity = 1.0f;
        [SerializeField] private float _minTimeBetweenHaptics = 0.05f;

        public static HapticFeedbackManager Instance { get; private set; }

        private float _lastHapticTime;

        public bool HapticsEnabled
        {
            get => _hapticsEnabled;
            set => _hapticsEnabled = value;
        }

        public float HapticIntensity
        {
            get => _hapticIntensity;
            set => _hapticIntensity = Mathf.Clamp01(value);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

#if UNITY_IOS && !UNITY_EDITOR
            iOSNativePlugin.PrepareHapticEngine();
#endif
        }

        /// <summary>
        /// Trigger a haptic feedback event.
        /// </summary>
        public void TriggerHaptic(HapticType type)
        {
            if (!_hapticsEnabled) return;
            if (Time.unscaledTime - _lastHapticTime < _minTimeBetweenHaptics) return;

            _lastHapticTime = Time.unscaledTime;

#if UNITY_IOS && !UNITY_EDITOR
            switch (type)
            {
                case HapticType.Light:
                    iOSNativePlugin.TriggerImpactHaptic(0, _hapticIntensity);
                    break;
                case HapticType.Medium:
                    iOSNativePlugin.TriggerImpactHaptic(1, _hapticIntensity);
                    break;
                case HapticType.Heavy:
                    iOSNativePlugin.TriggerImpactHaptic(2, _hapticIntensity);
                    break;
                case HapticType.Success:
                    iOSNativePlugin.TriggerNotificationHaptic(0);
                    break;
                case HapticType.Warning:
                    iOSNativePlugin.TriggerNotificationHaptic(1);
                    break;
                case HapticType.Error:
                    iOSNativePlugin.TriggerNotificationHaptic(2);
                    break;
            }
#endif
        }

        // Convenience methods matching game events

        public void OnBoostActivated() => TriggerHaptic(HapticType.Medium);
        public void OnDamageTaken() => TriggerHaptic(HapticType.Heavy);
        public void OnCollision() => TriggerHaptic(HapticType.Heavy);
        public void OnCheckpointReached() => TriggerHaptic(HapticType.Medium);
        public void OnCollectiblePickup() => TriggerHaptic(HapticType.Light);
        public void OnMedalEarned() => TriggerHaptic(HapticType.Success);
        public void OnLowHealth() => TriggerHaptic(HapticType.Warning);
        public void OnDestroyed() => TriggerHaptic(HapticType.Error);
        public void OnUIButtonTap() => TriggerHaptic(HapticType.Light);
        public void OnUpgradePurchased() => TriggerHaptic(HapticType.Success);
    }
}
```

### 7.4 iOSNativePlugin.cs

Create `Assets/Scripts/Core/iOSNativePlugin.cs`:

```csharp
// Native iOS bridge for Metal Pod.
// Provides access to Taptic Engine, VoiceOver, SKStoreReviewController, and font scaling.
// Uses [DllImport] to call Objective-C functions from the iOS native plugin.

using UnityEngine;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace MetalPod
{
    /// <summary>
    /// Static bridge to iOS native functionality.
    /// All methods are safe to call on non-iOS platforms (they no-op).
    /// </summary>
    public static class iOSNativePlugin
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void _PrepareHapticEngine();
        [DllImport("__Internal")] private static extern void _TriggerImpactHaptic(int style, float intensity);
        [DllImport("__Internal")] private static extern void _TriggerNotificationHaptic(int type);
        [DllImport("__Internal")] private static extern void _TriggerSelectionHaptic();
        [DllImport("__Internal")] private static extern bool _IsVoiceOverRunning();
        [DllImport("__Internal")] private static extern void _PostVoiceOverAnnouncement(string message);
        [DllImport("__Internal")] private static extern float _GetPreferredFontScale();
        [DllImport("__Internal")] private static extern void _RequestAppReview();
#endif

        public static void PrepareHapticEngine()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _PrepareHapticEngine();
#endif
        }

        /// <summary>
        /// Trigger impact haptic. style: 0=Light, 1=Medium, 2=Heavy.
        /// </summary>
        public static void TriggerImpactHaptic(int style, float intensity)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _TriggerImpactHaptic(style, intensity);
#endif
        }

        /// <summary>
        /// Trigger notification haptic. type: 0=Success, 1=Warning, 2=Error.
        /// </summary>
        public static void TriggerNotificationHaptic(int type)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _TriggerNotificationHaptic(type);
#endif
        }

        public static void TriggerSelectionHaptic()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _TriggerSelectionHaptic();
#endif
        }

        public static bool IsVoiceOverRunning()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _IsVoiceOverRunning();
#else
            return false;
#endif
        }

        public static void PostVoiceOverAnnouncement(string message)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _PostVoiceOverAnnouncement(message);
#endif
        }

        public static float GetPreferredFontScale()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _GetPreferredFontScale();
#else
            return 1.0f;
#endif
        }

        /// <summary>
        /// Request App Store review prompt (SKStoreReviewController).
        /// Call sparingly — Apple limits display frequency.
        /// Good times: after earning first gold medal, after completing 3rd course.
        /// </summary>
        public static void RequestAppReview()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _RequestAppReview();
#endif
        }
    }
}
```

### 7.5 AppIconGenerator.cs

Create `Assets/Scripts/Editor/AppIconGenerator.cs`:

```csharp
// Editor script to generate all required iOS app icon sizes from a 1024x1024 source.
// Menu: Metal Pod > Generate App Icons

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace MetalPod.Editor
{
    public static class AppIconGenerator
    {
        // All icon sizes required by iOS (as of 2025)
        private static readonly (int size, string suffix)[] IconSizes = new[]
        {
            // iPhone Notification
            (40, "iphone-notification@2x"),
            (60, "iphone-notification@3x"),
            // iPhone Settings
            (58, "iphone-settings@2x"),
            (87, "iphone-settings@3x"),
            // iPhone Spotlight
            (80, "iphone-spotlight@2x"),
            (120, "iphone-spotlight@3x"),
            // iPhone App
            (120, "iphone-app@2x"),
            (180, "iphone-app@3x"),
            // iPad Notification
            (20, "ipad-notification@1x"),
            (40, "ipad-notification@2x"),
            // iPad Settings
            (29, "ipad-settings@1x"),
            (58, "ipad-settings@2x"),
            // iPad Spotlight
            (40, "ipad-spotlight@1x"),
            (80, "ipad-spotlight@2x"),
            // iPad App
            (76, "ipad-app@1x"),
            (152, "ipad-app@2x"),
            // iPad Pro App
            (167, "ipad-pro-app@2x"),
            // App Store
            (1024, "appstore"),
        };

        [MenuItem("Metal Pod/Generate App Icons")]
        public static void GenerateIcons()
        {
            string sourcePath = EditorUtility.OpenFilePanel(
                "Select 1024x1024 App Icon Source", "Assets", "png");

            if (string.IsNullOrEmpty(sourcePath)) return;

            byte[] sourceBytes = File.ReadAllBytes(sourcePath);
            Texture2D source = new Texture2D(2, 2);
            source.LoadImage(sourceBytes);

            if (source.width != 1024 || source.height != 1024)
            {
                EditorUtility.DisplayDialog("Error",
                    $"Source image must be 1024x1024. Got {source.width}x{source.height}.",
                    "OK");
                Object.DestroyImmediate(source);
                return;
            }

            string outputDir = "Assets/AppIcons";
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            int completed = 0;
            foreach (var (size, suffix) in IconSizes)
            {
                EditorUtility.DisplayProgressBar("Generating Icons",
                    $"Creating {suffix} ({size}x{size})",
                    (float)completed / IconSizes.Length);

                Texture2D resized = ResizeTexture(source, size, size);
                byte[] pngBytes = resized.EncodeToPNG();
                string outPath = Path.Combine(outputDir, $"icon-{suffix}.png");
                File.WriteAllBytes(outPath, pngBytes);
                Object.DestroyImmediate(resized);
                completed++;
            }

            Object.DestroyImmediate(source);
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("App Icons Generated",
                $"Created {IconSizes.Length} icon variants in {outputDir}/",
                "OK");

            Debug.Log($"[AppIconGenerator] Generated {IconSizes.Length} icons in {outputDir}");
        }

        private static Texture2D ResizeTexture(Texture2D source, int width, int height)
        {
            RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            rt.filterMode = FilterMode.Bilinear;

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            Graphics.Blit(source, rt);

            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            return result;
        }
    }
}
#endif
```

### 7.6 BuildPreprocessor.cs

Create `Assets/Scripts/Editor/BuildPreprocessor.cs`:

```csharp
// Pre-build script that configures iOS-specific settings before Xcode project generation.
// Automatically sets Info.plist entries, capabilities, and build options.
// Runs via IPreprocessBuildWithReport callback.

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;

namespace MetalPod.Editor
{
    /// <summary>
    /// Automatically configures iOS build settings before building.
    /// </summary>
    public class BuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS) return;

            Debug.Log("[BuildPreprocessor] Configuring iOS build settings...");

            // Set Player Settings
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;

            // Landscape only
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

            // Performance
            PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 1); // ARM64 only
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingBackend.IL2CPP);
            PlayerSettings.iOS.scriptCallOptimization = ScriptCallOptimizationLevel.FastButNoExceptions;
            PlayerSettings.stripEngineCode = true;

            // Bundle ID and version
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.crocobyte.metalpod");
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.iOS.buildNumber = "1";

            // Graphics
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.iOS, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { UnityEngine.Rendering.GraphicsDeviceType.Metal });
            PlayerSettings.colorSpace = ColorSpace.Linear;

            // Accelerometer
            PlayerSettings.accelerometerFrequency = 60;

            // Status bar
            PlayerSettings.statusBarHidden = true;

            Debug.Log("[BuildPreprocessor] iOS build settings configured successfully.");
        }
    }

    /// <summary>
    /// Post-process the Xcode project to add Info.plist entries and privacy manifest.
    /// </summary>
    public class BuildPostprocessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS) return;

#if UNITY_IOS
            string plistPath = Path.Combine(report.summary.outputPath, "Info.plist");

            if (File.Exists(plistPath))
            {
                PlistDocument plist = new PlistDocument();
                plist.ReadFromFile(plistPath);
                PlistElementDict root = plist.root;

                // Motion usage (accelerometer for tilt controls)
                root.SetString("NSMotionUsageDescription",
                    "Metal Pod uses device motion for tilt-based steering controls.");

                // No encryption
                root.SetBoolean("ITSAppUsesNonExemptEncryption", false);

                // Full screen required
                root.SetBoolean("UIRequiresFullScreen", true);

                plist.WriteToFile(plistPath);
                Debug.Log("[BuildPostprocessor] Info.plist updated with required keys.");
            }

            // Copy privacy manifest if it exists
            string privacySource = Path.Combine(Application.dataPath, "Plugins/iOS/PrivacyInfo.xcprivacy");
            if (File.Exists(privacySource))
            {
                string privacyDest = Path.Combine(report.summary.outputPath, "PrivacyInfo.xcprivacy");
                File.Copy(privacySource, privacyDest, true);
                Debug.Log("[BuildPostprocessor] PrivacyInfo.xcprivacy copied to Xcode project.");
            }
#endif
        }
    }
}
#endif
```

---

## SECTION 8: iOS NATIVE PLUGIN (Objective-C)

Create `Assets/Plugins/iOS/MetalPodNative.mm`:

```objectivec
// MetalPodNative.mm
// Native iOS plugin for Metal Pod
// Provides: Haptic Engine, VoiceOver bridge, Dynamic Type, App Store Review

#import <UIKit/UIKit.h>
#import <CoreHaptics/CoreHaptics.h>
#import <StoreKit/StoreKit.h>

// MARK: - Haptic Engine

static CHHapticEngine *hapticEngine = nil;

extern "C" {

void _PrepareHapticEngine() {
    if (@available(iOS 13.0, *)) {
        NSError *error = nil;
        hapticEngine = [[CHHapticEngine alloc] initAndReturnError:&error];
        if (error) {
            NSLog(@"[MetalPod] Haptic engine init error: %@", error.localizedDescription);
            return;
        }
        [hapticEngine startAndReturnError:&error];
        if (error) {
            NSLog(@"[MetalPod] Haptic engine start error: %@", error.localizedDescription);
        }

        // Auto-restart if stopped
        hapticEngine.stoppedHandler = ^(CHHapticEngineStoppedReason reason) {
            NSError *startError = nil;
            [hapticEngine startAndReturnError:&startError];
        };
        hapticEngine.resetHandler = ^{
            NSError *startError = nil;
            [hapticEngine startAndReturnError:&startError];
        };
    }
}

void _TriggerImpactHaptic(int style, float intensity) {
    UIImpactFeedbackStyle feedbackStyle;
    switch (style) {
        case 0: feedbackStyle = UIImpactFeedbackStyleLight; break;
        case 1: feedbackStyle = UIImpactFeedbackStyleMedium; break;
        case 2: feedbackStyle = UIImpactFeedbackStyleHeavy; break;
        default: feedbackStyle = UIImpactFeedbackStyleMedium; break;
    }

    UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc]
                                            initWithStyle:feedbackStyle];
    [generator prepare];
    [generator impactOccurredWithIntensity:intensity];
}

void _TriggerNotificationHaptic(int type) {
    UINotificationFeedbackType feedbackType;
    switch (type) {
        case 0: feedbackType = UINotificationFeedbackTypeSuccess; break;
        case 1: feedbackType = UINotificationFeedbackTypeWarning; break;
        case 2: feedbackType = UINotificationFeedbackTypeError; break;
        default: feedbackType = UINotificationFeedbackTypeSuccess; break;
    }

    UINotificationFeedbackGenerator *generator = [[UINotificationFeedbackGenerator alloc] init];
    [generator prepare];
    [generator notificationOccurred:feedbackType];
}

void _TriggerSelectionHaptic() {
    UISelectionFeedbackGenerator *generator = [[UISelectionFeedbackGenerator alloc] init];
    [generator prepare];
    [generator selectionChanged];
}

// MARK: - VoiceOver

bool _IsVoiceOverRunning() {
    return UIAccessibilityIsVoiceOverRunning();
}

void _PostVoiceOverAnnouncement(const char *message) {
    NSString *nsMessage = [NSString stringWithUTF8String:message];
    UIAccessibilityPostNotification(UIAccessibilityAnnouncementNotification, nsMessage);
}

// MARK: - Dynamic Type

float _GetPreferredFontScale() {
    UIContentSizeCategory category = [UIApplication sharedApplication].preferredContentSizeCategory;

    if ([category isEqualToString:UIContentSizeCategoryExtraSmall]) return 0.8f;
    if ([category isEqualToString:UIContentSizeCategorySmall]) return 0.85f;
    if ([category isEqualToString:UIContentSizeCategoryMedium]) return 0.9f;
    if ([category isEqualToString:UIContentSizeCategoryLarge]) return 1.0f; // Default
    if ([category isEqualToString:UIContentSizeCategoryExtraLarge]) return 1.1f;
    if ([category isEqualToString:UIContentSizeCategoryExtraExtraLarge]) return 1.2f;
    if ([category isEqualToString:UIContentSizeCategoryExtraExtraExtraLarge]) return 1.3f;
    if ([category isEqualToString:UIContentSizeCategoryAccessibilityMedium]) return 1.4f;
    if ([category isEqualToString:UIContentSizeCategoryAccessibilityLarge]) return 1.5f;
    if ([category isEqualToString:UIContentSizeCategoryAccessibilityExtraLarge]) return 1.6f;
    if ([category isEqualToString:UIContentSizeCategoryAccessibilityExtraExtraLarge]) return 1.7f;
    if ([category isEqualToString:UIContentSizeCategoryAccessibilityExtraExtraExtraLarge]) return 1.8f;

    return 1.0f;
}

// MARK: - App Store Review

void _RequestAppReview() {
    if (@available(iOS 14.0, *)) {
        UIWindowScene *windowScene = nil;
        for (UIScene *scene in [UIApplication sharedApplication].connectedScenes) {
            if (scene.activationState == UISceneActivationStateForegroundActive &&
                [scene isKindOfClass:[UIWindowScene class]]) {
                windowScene = (UIWindowScene *)scene;
                break;
            }
        }
        if (windowScene) {
            [SKStoreReviewController requestReviewInScene:windowScene];
        }
    }
}

} // extern "C"
```

---

## Acceptance Criteria

- [ ] `Docs/APP_STORE_METADATA.md` — Complete with all App Store Connect fields filled in
- [ ] `Docs/APP_STORE_SUBMISSION_CHECKLIST.md` — Comprehensive step-by-step checklist
- [ ] `Docs/PRIVACY_POLICY.md` — GDPR/App Store compliant, notes "no data collected"
- [ ] `Docs/SCREENSHOT_PLAN.md` — 10 screenshot compositions with text overlays
- [ ] `Docs/APP_REVIEW_NOTES.md` — Clear instructions for Apple reviewers
- [ ] `Assets/Plugins/iOS/PrivacyInfo.xcprivacy` — Valid XML, correct API reason codes
- [ ] `Assets/Plugins/iOS/MetalPodNative.mm` — Compiles with Xcode, covers haptics + VoiceOver + review
- [ ] `Assets/Scripts/Accessibility/AccessibilityManager.cs` — VoiceOver + Dynamic Type support
- [ ] `Assets/Scripts/Accessibility/AccessibilityLabels.cs` — All UI labels defined
- [ ] `Assets/Scripts/Accessibility/HapticFeedbackManager.cs` — All game events mapped to haptic types
- [ ] `Assets/Scripts/Core/iOSNativePlugin.cs` — Safe DllImport bridge, no-ops on non-iOS
- [ ] `Assets/Scripts/Editor/AppIconGenerator.cs` — Generates all 17 icon sizes from 1024x1024 source
- [ ] `Assets/Scripts/Editor/BuildPreprocessor.cs` — Auto-configures iOS build settings + Info.plist
- [ ] All scripts use `MetalPod` or `MetalPod.Accessibility` or `MetalPod.Editor` namespaces
- [ ] All scripts compile without errors (verify `using` statements match existing codebase)
- [ ] No conflicts with existing files in the project

---

## Cross-References

This task's output integrates with:
- **HapticFeedbackManager** should be called from:
  - `HovercraftController.cs` — boost, collision events
  - `HovercraftHealth.cs` — damage, destruction events
  - `Checkpoint.cs` — checkpoint reached
  - `Collectible.cs` — collectible pickup
  - `ResultsScreenUI.cs` — medal earned
  - `UpgradeUI.cs` / `CustomizationUI.cs` — purchase events
- **AccessibilityManager.Announce()** should be called from:
  - `CourseManager.cs` — checkpoint/finish announcements
  - `HUD.cs` — countdown announcements
  - `ResultsScreenUI.cs` — result announcements
- **iOSNativePlugin.RequestAppReview()** should be called from:
  - `ProgressionManager.cs` — after earning 3rd gold medal (good engagement signal)
- **BuildPreprocessor** runs automatically on iOS builds — no integration needed
- **AppIconGenerator** is a standalone editor menu item
