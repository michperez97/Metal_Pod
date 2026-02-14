// CloudSaveNative.mm
// Native iOS plugin for iCloud NSUbiquitousKeyValueStore access.
// Separate from MetalPodNative.mm to keep concerns isolated.

#import <Foundation/Foundation.h>
#include <stdlib.h>
#include <string.h>

// Unity callback bridge
extern void UnitySendMessage(const char *obj, const char *method, const char *msg);

static const char *kCallbackObject = "CloudSaveManager"; // GameObject name

extern "C"
{
    // Availability
    bool _CloudSave_IsAvailable()
    {
        NSFileManager *fm = [NSFileManager defaultManager];
        NSURL *ubiquityURL = [fm URLForUbiquityContainerIdentifier:nil];
        return ubiquityURL != nil;
    }

    // Write
    void _CloudSave_SetString(const char *key, const char *value)
    {
        if (key == NULL)
        {
            return;
        }

        NSString *nsKey = [NSString stringWithUTF8String:key];
        NSString *nsValue = value == NULL ? @"" : [NSString stringWithUTF8String:value];
        if (nsValue == nil)
        {
            nsValue = @"";
        }

        NSUbiquitousKeyValueStore *store = [NSUbiquitousKeyValueStore defaultStore];
        [store setString:nsValue forKey:nsKey];
    }

    void _CloudSave_SetTimestamp(const char *key, long long timestamp)
    {
        if (key == NULL)
        {
            return;
        }

        NSString *nsKey = [NSString stringWithUTF8String:key];
        NSUbiquitousKeyValueStore *store = [NSUbiquitousKeyValueStore defaultStore];
        [store setLongLong:timestamp forKey:nsKey];
    }

    // Read
    const char *_CloudSave_GetString(const char *key)
    {
        if (key == NULL)
        {
            return "";
        }

        NSString *nsKey = [NSString stringWithUTF8String:key];
        NSUbiquitousKeyValueStore *store = [NSUbiquitousKeyValueStore defaultStore];
        NSString *value = [store stringForKey:nsKey];

        if (value == nil)
        {
            return "";
        }

        const char *utf8 = [value UTF8String];
        if (utf8 == NULL)
        {
            return "";
        }

        char *result = (char *)malloc(strlen(utf8) + 1);
        strcpy(result, utf8);
        return result;
    }

    long long _CloudSave_GetTimestamp(const char *key)
    {
        if (key == NULL)
        {
            return 0;
        }

        NSString *nsKey = [NSString stringWithUTF8String:key];
        NSUbiquitousKeyValueStore *store = [NSUbiquitousKeyValueStore defaultStore];
        return [store longLongForKey:nsKey];
    }

    // Sync
    bool _CloudSave_Synchronize()
    {
        NSUbiquitousKeyValueStore *store = [NSUbiquitousKeyValueStore defaultStore];
        return [store synchronize];
    }

    // Delete
    void _CloudSave_Remove(const char *key)
    {
        if (key == NULL)
        {
            return;
        }

        NSString *nsKey = [NSString stringWithUTF8String:key];
        NSUbiquitousKeyValueStore *store = [NSUbiquitousKeyValueStore defaultStore];
        [store removeObjectForKey:nsKey];
    }

    // Notifications
    void _CloudSave_RegisterForNotifications()
    {
        NSUbiquitousKeyValueStore *store = [NSUbiquitousKeyValueStore defaultStore];

        [[NSNotificationCenter defaultCenter] addObserverForName:NSUbiquitousKeyValueStoreDidChangeExternallyNotification
                                                          object:store
                                                           queue:[NSOperationQueue mainQueue]
                                                      usingBlock:^(NSNotification *notification)
        {
            NSDictionary *userInfo = notification.userInfo;
            NSNumber *reason = userInfo[NSUbiquitousKeyValueStoreChangeReasonKey];

            // reason: 0 = ServerChange, 1 = InitialSyncChange, 2 = QuotaViolation, 3 = AccountChange
            int reasonInt = reason ? [reason intValue] : 0;

            NSString *msg = [NSString stringWithFormat:@"%d", reasonInt];
            UnitySendMessage(kCallbackObject, "OnCloudDataChanged", [msg UTF8String]);
        }];

        // Trigger initial sync
        [store synchronize];
    }
}
