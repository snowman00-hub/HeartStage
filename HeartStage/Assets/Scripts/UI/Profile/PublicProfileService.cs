using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

public static class PublicProfileService
{
    private static DatabaseReference Root => FirebaseDatabase.DefaultInstance.RootReference;
    private static FirebaseAuth Auth => FirebaseAuth.DefaultInstance;

    public static async UniTask UpdateMyPublicProfileAsync(
     SaveDataV1 data,
     int achievementCompletedCount
 )
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) return;

        string uid = user.UserId;
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        string effectiveNickname = ProfileNameUtil.GetEffectiveNickname(data);

        var dict = new Dictionary<string, object>
        {
            // 상단
            ["nickname"] = effectiveNickname,
            ["fanAmount"] = data.fanAmount,
            ["equippedTitleId"] = data.equippedTitleId,
            ["statusMessage"] = data.statusMessage,
            ["profileIconId"] = data.profileIconKey,

            // 공연 기록 박스
            ["mainStageStep1"] = data.mainStageStep1,
            ["mainStageStep2"] = data.mainStageStep2,
            ["achievementCompletedCount"] = achievementCompletedCount,
            ["bestFanMeetingSeconds"] = data.bestFanMeetingSeconds,

            // 🔹 지금은 스페셜 기록은 안 올림 (공석)
            // 나중에 필요하면 여기 ["specialStageBestSeconds"] 추가

            ["lastLoginUnixMillis"] = now,
        };

        await FirebaseDatabase.DefaultInstance
            .RootReference
            .Child("publicProfiles")
            .Child(uid)
            .UpdateChildrenAsync(dict);
    }
}
