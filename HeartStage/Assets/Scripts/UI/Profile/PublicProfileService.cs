using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
public class PublicProfileData
{
    public string uid;
    public string nickname;
    public int fanAmount;
    public int equippedTitleId;
    public string profileIconKey;
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

            // 🔹 specialStageBestSeconds는 지금은 안 올림 (공석)
            // 나중에 필요하면 ["specialStageBestSeconds"] 추가

            ["lastLoginUnixMillis"] = now,
        };

        await FirebaseDatabase.DefaultInstance
            .RootReference
            .Child("publicProfiles")
            .Child(uid)
            .UpdateChildrenAsync(dict);
    }

    public static async UniTask<PublicProfileData> GetPublicProfileAsync(string uid)
    {
        var snap = await Root.Child("publicProfiles").Child(uid).GetValueAsync();
        if (!snap.Exists) return null;

        var data = new PublicProfileData();
        data.uid = uid;
        data.nickname = snap.Child("nickname").Value?.ToString() ?? uid;
        data.profileIconKey = snap.Child("profileIconId").Value?.ToString() ?? "ProfileIcon_Default";

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

        return data;
    }
}
