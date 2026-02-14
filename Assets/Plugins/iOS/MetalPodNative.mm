// MetalPodNative.mm
// Native iOS plugin for Metal Pod.
// Provides haptics, VoiceOver bridge, Dynamic Type scale, and App Store review prompt.

#import <UIKit/UIKit.h>
#import <StoreKit/StoreKit.h>
#import <CoreHaptics/CoreHaptics.h>

static CHHapticEngine *gHapticEngine = nil;

extern "C"
{
    void _PrepareHapticEngine()
    {
        if (@available(iOS 13.0, *))
        {
            NSError *error = nil;
            gHapticEngine = [[CHHapticEngine alloc] initAndReturnError:&error];
            if (error != nil)
            {
                NSLog(@"[MetalPodNative] Failed to initialize haptic engine: %@", error.localizedDescription);
                return;
            }

            [gHapticEngine startAndReturnError:&error];
            if (error != nil)
            {
                NSLog(@"[MetalPodNative] Failed to start haptic engine: %@", error.localizedDescription);
            }

            gHapticEngine.stoppedHandler = ^(CHHapticEngineStoppedReason reason)
            {
                NSError *startError = nil;
                [gHapticEngine startAndReturnError:&startError];
                if (startError != nil)
                {
                    NSLog(@"[MetalPodNative] Haptic engine restart failed: %@", startError.localizedDescription);
                }
            };

            gHapticEngine.resetHandler = ^
            {
                NSError *startError = nil;
                [gHapticEngine startAndReturnError:&startError];
                if (startError != nil)
                {
                    NSLog(@"[MetalPodNative] Haptic engine reset restart failed: %@", startError.localizedDescription);
                }
            };
        }
    }

    void _TriggerImpactHaptic(int style, float intensity)
    {
        UIImpactFeedbackStyle feedbackStyle = UIImpactFeedbackStyleMedium;
        switch (style)
        {
            case 0:
                feedbackStyle = UIImpactFeedbackStyleLight;
                break;
            case 1:
                feedbackStyle = UIImpactFeedbackStyleMedium;
                break;
            case 2:
                feedbackStyle = UIImpactFeedbackStyleHeavy;
                break;
            default:
                break;
        }

        UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:feedbackStyle];
        [generator prepare];

        if (@available(iOS 13.0, *))
        {
            [generator impactOccurredWithIntensity:intensity];
        }
        else
        {
            [generator impactOccurred];
        }
    }

    void _TriggerNotificationHaptic(int type)
    {
        UINotificationFeedbackType feedbackType = UINotificationFeedbackTypeSuccess;
        switch (type)
        {
            case 0:
                feedbackType = UINotificationFeedbackTypeSuccess;
                break;
            case 1:
                feedbackType = UINotificationFeedbackTypeWarning;
                break;
            case 2:
                feedbackType = UINotificationFeedbackTypeError;
                break;
            default:
                break;
        }

        UINotificationFeedbackGenerator *generator = [[UINotificationFeedbackGenerator alloc] init];
        [generator prepare];
        [generator notificationOccurred:feedbackType];
    }

    void _TriggerSelectionHaptic()
    {
        UISelectionFeedbackGenerator *generator = [[UISelectionFeedbackGenerator alloc] init];
        [generator prepare];
        [generator selectionChanged];
    }

    bool _IsVoiceOverRunning()
    {
        return UIAccessibilityIsVoiceOverRunning();
    }

    void _PostVoiceOverAnnouncement(const char *message)
    {
        if (message == NULL)
        {
            return;
        }

        NSString *text = [NSString stringWithUTF8String:message];
        if (text == nil || text.length == 0)
        {
            return;
        }

        UIAccessibilityPostNotification(UIAccessibilityAnnouncementNotification, text);
    }

    float _GetPreferredFontScale()
    {
        UIContentSizeCategory category = [UIApplication sharedApplication].preferredContentSizeCategory;

        if ([category isEqualToString:UIContentSizeCategoryExtraSmall]) return 0.80f;
        if ([category isEqualToString:UIContentSizeCategorySmall]) return 0.85f;
        if ([category isEqualToString:UIContentSizeCategoryMedium]) return 0.90f;
        if ([category isEqualToString:UIContentSizeCategoryLarge]) return 1.00f;
        if ([category isEqualToString:UIContentSizeCategoryExtraLarge]) return 1.10f;
        if ([category isEqualToString:UIContentSizeCategoryExtraExtraLarge]) return 1.20f;
        if ([category isEqualToString:UIContentSizeCategoryExtraExtraExtraLarge]) return 1.30f;
        if ([category isEqualToString:UIContentSizeCategoryAccessibilityMedium]) return 1.40f;
        if ([category isEqualToString:UIContentSizeCategoryAccessibilityLarge]) return 1.50f;
        if ([category isEqualToString:UIContentSizeCategoryAccessibilityExtraLarge]) return 1.60f;
        if ([category isEqualToString:UIContentSizeCategoryAccessibilityExtraExtraLarge]) return 1.70f;
        if ([category isEqualToString:UIContentSizeCategoryAccessibilityExtraExtraExtraLarge]) return 1.80f;

        return 1.00f;
    }

    void _RequestAppReview()
    {
        if (@available(iOS 14.0, *))
        {
            UIWindowScene *activeScene = nil;
            NSSet<UIScene *> *connectedScenes = [UIApplication sharedApplication].connectedScenes;
            for (UIScene *scene in connectedScenes)
            {
                if (scene.activationState == UISceneActivationStateForegroundActive && [scene isKindOfClass:[UIWindowScene class]])
                {
                    activeScene = (UIWindowScene *)scene;
                    break;
                }
            }

            if (activeScene != nil)
            {
                [SKStoreReviewController requestReviewInScene:activeScene];
            }
        }
        else if (@available(iOS 10.3, *))
        {
            [SKStoreReviewController requestReview];
        }
    }
}
