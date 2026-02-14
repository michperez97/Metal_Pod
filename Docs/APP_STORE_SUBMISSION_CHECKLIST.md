# Metal Pod - App Store Submission Checklist

## Phase 1: Pre-Build Preparation

### Xcode Project Settings
- [ ] Bundle Identifier: `com.crocobyte.metalpod`
- [ ] Version: `1.0.0`
- [ ] Build Number: `1`
- [ ] Deployment Target: iOS 15.0+
- [ ] Device Family: iPhone + iPad (Universal)
- [ ] Supported Orientations: Landscape Left, Landscape Right
- [ ] Requires Full Screen: YES
- [ ] Status Bar: Hidden
- [ ] `ITSAppUsesNonExemptEncryption` = `false`

### Capabilities
- [ ] Game Center: OFF (v1.0)
- [ ] In-App Purchase: OFF
- [ ] Push Notifications: OFF
- [ ] Sign in with Apple: OFF

### Info.plist Required Keys
- [ ] `NSMotionUsageDescription` = "Metal Pod uses device motion for tilt-based steering controls."
- [ ] `ITSAppUsesNonExemptEncryption` = `false`
- [ ] `UIRequiresFullScreen` = `true`
- [ ] `UISupportedInterfaceOrientations` includes only landscape

### Privacy
- [ ] `Assets/Plugins/iOS/PrivacyInfo.xcprivacy` included in Xcode project
- [ ] Privacy policy hosted at public URL
- [ ] App Store Connect privacy questionnaire answered accurately (no collected data)

## Phase 2: Unity Build Configuration

### Player Settings (Unity)
- [ ] Company Name: Crocobyte
- [ ] Product Name: Metal Pod
- [ ] Default Orientation: Landscape Left
- [ ] Auto Graphics API: OFF (Metal only)
- [ ] Color Space: Linear
- [ ] Scripting Backend: IL2CPP
- [ ] Architecture: ARM64
- [ ] Target iOS Version: 15.0+
- [ ] Accelerometer support enabled
- [ ] Requires ARKit: OFF
- [ ] Allow HTTP downloads: OFF

### Build Optimization
- [ ] Strip Engine Code: ON
- [ ] Managed Stripping Level: Medium (verify runtime)
- [ ] Script Call Optimization: Fast but No Exceptions (if compatible)
- [ ] Texture compression: ASTC profile selected
- [ ] Audio compression tuned for mobile
- [ ] Remove unused Unity packages/modules

### Scenes in Build
- [ ] `_Persistent`
- [ ] `MainMenu`
- [ ] `Workshop`
- [ ] `TestCourse`
- [ ] Lava courses
- [ ] Ice courses
- [ ] Toxic courses

## Phase 3: Testing

### Device Testing
- [ ] Test on physical iPhone (accelerometer + haptics)
- [ ] Test on oldest supported iOS hardware target
- [ ] Test on latest iPhone hardware target
- [ ] Test on iPad layout and input scaling
- [ ] Verify stable frame rate on target devices
- [ ] Verify all courses complete without crash
- [ ] Verify save/load across app restart
- [ ] Verify upgrades and cosmetics persistence
- [ ] Verify first-launch tutorial behavior

### Edge Cases
- [ ] App backgrounding during race pauses/resumes safely
- [ ] Interruptions (call/notification/control center)
- [ ] Rapid scene transitions do not crash
- [ ] Corrupt or missing save recovers safely
- [ ] Fresh install unlock state is correct

## Phase 4: App Store Connect

### App Information
- [ ] Create app record in App Store Connect
- [ ] Fill metadata from `Docs/APP_STORE_METADATA.md`
- [ ] Upload 1024x1024 icon (no alpha)
- [ ] Upload required screenshots for iPhone and iPad sizes
- [ ] Upload optional 30s app preview video
- [ ] Complete age rating questionnaire
- [ ] Set price tier and availability territories

### Review Information
- [ ] Add reviewer notes from `Docs/APP_REVIEW_NOTES.md`
- [ ] Add contact details for review team
- [ ] Confirm no demo account required

### Privacy
- [ ] Complete privacy nutrition labels accurately
- [ ] Add privacy policy URL
- [ ] ATT prompt not required (no tracking)

## Phase 5: Build and Submit
- [ ] Archive in Xcode (`Product > Archive`)
- [ ] Validate archive passes checks
- [ ] Upload with Xcode Organizer
- [ ] Wait for build processing
- [ ] Select processed build in App Store Connect
- [ ] Submit for review
- [ ] Monitor review status and resolution center

## Phase 6: Post-Submission
- [ ] Prepare launch/support communication
- [ ] Monitor crash reports and diagnostics
- [ ] Respond to App Store reviews
- [ ] Plan v1.1 update backlog from user feedback

## Sign-Off
- [ ] Product owner approval
- [ ] QA lead approval
- [ ] Release manager approval

Last checklist review date: February 14, 2026
