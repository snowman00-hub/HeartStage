using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class DreamEnergyGiftService
{
    private static DatabaseReference Root => FirebaseDatabase.DefaultInstance.RootReference;
    private static FirebaseAuth Auth => FirebaseAuth.DefaultInstance;

    public const int GiftAmountPerSend = 1; // 한 번 보낼 때 1 에너지

    private static string GetMyUid()
    {
        var user = Auth.CurrentUser;
        return user?.UserId;
    }

    private static int GetTodayYmd()
    {
        var now = DateTime.Now;
        return now.Year * 10000 + now.Month * 100 + now.Day;
    }

    /// <summary>
    /// 오늘 횟수/날짜 체크 후, 친구에게 드림 에너지 선물하기
    /// dreamGifts/friendUid/autoKey = { fromUid, amount, createdAt, claimed:false }
    /// </summary>
    public static async UniTask<bool> TrySendDreamEnergyAsync(string friendUid)
    {
        string myUid = GetMyUid();
        if (string.IsNullOrEmpty(myUid) || string.IsNullOrEmpty(friendUid))
            return false;
        if (myUid == friendUid)
            return false;

        if (SaveLoadManager.Data is not SaveDataV1 data)
            return false;

        int today = GetTodayYmd();
        if (data.dreamLastSendDate != today)
        {
            data.dreamLastSendDate = today;
            data.dreamSendTodayCount = 0;
        }

        if (data.dreamSendTodayCount >= data.dreamSendDailyLimit)
        {
            Debug.Log("[DreamEnergyGiftService] 오늘 보낼 수 있는 횟수를 모두 사용했습니다.");
            return false;
        }

        try
        {
            string key = Root.Child("dreamGifts").Child(friendUid).Push().Key;
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var dict = new Dictionary<string, object>
            {
                ["fromUid"] = myUid,
                ["amount"] = GiftAmountPerSend,
                ["createdAt"] = now,
                ["claimed"] = false,
            };

            await Root.Child("dreamGifts").Child(friendUid).Child(key).SetValueAsync(dict);

            data.dreamSendTodayCount++;
            await SaveLoadManager.SaveToServer();

            Debug.Log("[DreamEnergyGiftService] 드림 에너지 선물 전송 완료");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[DreamEnergyGiftService] TrySendDreamEnergyAsync Error: {e}");
            return false;
        }
    }

    /// <summary>
    /// 내가 받은 선물 전부 수령 → dreamEnergy에 합산
    /// </summary>
    public static async UniTask<int> ClaimAllGiftsAsync()
    {
        string myUid = GetMyUid();
        if (string.IsNullOrEmpty(myUid))
            return 0;

        if (SaveLoadManager.Data is not SaveDataV1 data)
            return 0;

        int totalReceived = 0;

        try
        {
            var snap = await Root.Child("dreamGifts").Child(myUid).GetValueAsync();
            if (!snap.Exists) return 0;

            var updates = new Dictionary<string, object>();

            foreach (var child in snap.Children)
            {
                bool claimed = false;
                int amount = 0;

                if (child.Child("claimed").Value is bool c)
                    claimed = c;
                if (child.Child("amount").Value is long a)
                    amount = (int)a;

                if (!claimed && amount > 0)
                {
                    totalReceived += amount;
                    updates[$"dreamGifts/{myUid}/{child.Key}/claimed"] = true;
                }
            }

            if (totalReceived > 0)
            {
                data.dreamEnergy += totalReceived;
                await SaveLoadManager.SaveToServer();

                if (updates.Count > 0)
                    await Root.UpdateChildrenAsync(updates);
            }

            Debug.Log($"[DreamEnergyGiftService] 받은 드림 에너지: {totalReceived}");
            return totalReceived;
        }
        catch (Exception e)
        {
            Debug.LogError($"[DreamEnergyGiftService] ClaimAllGiftsAsync Error: {e}");
            return 0;
        }
    }
}
