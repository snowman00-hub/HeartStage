using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PublicProfileData
{
    public string uid;
    public string nickname;
    public string statusMessage;
    public string profileIconKey;
    public int fanAmount;
    public int equippedTitleId;
    public int mainStageStep1;
    public int mainStageStep2;
    public int achievementCompletedCount;
    public int bestFanMeetingSeconds;
    public int specialStageBestSeconds;
}

public static partial class PublicProfileService
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
        string iconKey = string.IsNullOrEmpty(data.profileIconKey) ? "hanaicon" : data.profileIconKey;

        var dict = new Dictionary<string, object>
        {
            ["nickname"] = effectiveNickname,
            ["fanAmount"] = data.fanAmount,
            ["equippedTitleId"] = data.equippedTitleId,
            ["statusMessage"] = data.statusMessage ?? "",
            ["profileIconId"] = iconKey,
            ["mainStageStep1"] = data.mainStageStep1,
            ["mainStageStep2"] = data.mainStageStep2,
            ["achievementCompletedCount"] = achievementCompletedCount,
            ["bestFanMeetingSeconds"] = data.bestFanMeetingSeconds,
            ["lastLoginUnixMillis"] = now,
        };

        await FirebaseDatabase.DefaultInstance
            .RootReference
            .Child("publicProfiles")
            .Child(uid)
            .UpdateChildrenAsync(dict);

        Debug.Log($"[PublicProfileService] 프로필 업데이트: {effectiveNickname}, 아이콘: {iconKey}");
    }

    public static async UniTask<PublicProfileData> GetPublicProfileAsync(string uid)
    {
        try
        {
            var snap = await Root.Child("publicProfiles").Child(uid).GetValueAsync();
            if (!snap.Exists)
            {
                Debug.LogWarning($"[PublicProfileService] 프로필이 존재하지 않음: {uid}");
                return null;
            }

            var data = new PublicProfileData();
            data.uid = uid;
            data.nickname = snap.Child("nickname").Value?.ToString() ?? uid;
            data.statusMessage = snap.Child("statusMessage").Value?.ToString() ?? "";

            // 그대로 가져온다.  변환 안 함.
            data.profileIconKey = snap.Child("profileIconId").Value?.ToString() ?? "hanaicon";

            if (snap.Child("fanAmount").Value is long fa)
                data.fanAmount = (int)fa;
            if (snap.Child("equippedTitleId").Value is long t)
                data.equippedTitleId = (int)t;
            if (snap.Child("mainStageStep1").Value is long s1)
                data.mainStageStep1 = (int)s1;
            if (snap.Child("mainStageStep2").Value is long s2)
                data.mainStageStep2 = (int)s2;
            if (snap.Child("achievementCompletedCount").Value is long ac)
                data.achievementCompletedCount = (int)ac;
            if (snap.Child("bestFanMeetingSeconds").Value is long bf)
                data.bestFanMeetingSeconds = (int)bf;
            if (snap.Child("specialStageBestSeconds").Value is long sp)
                data.specialStageBestSeconds = (int)sp;

            Debug.Log($"[PublicProfileService] 프로필 로드: {data.nickname}, 아이콘: {data.profileIconKey}");

            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[PublicProfileService] GetPublicProfileAsync Error: {e}");
            return null;
        }
    }
}