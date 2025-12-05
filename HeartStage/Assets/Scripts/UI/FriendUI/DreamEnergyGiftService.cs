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

    public const int GiftAmountPerSend = 1;

    private static bool _isSending = false;
    private static bool _isClaiming = false;

    private static HashSet<string> _sentTodayCache = new HashSet<string>();
    private static int _sentTodayCacheDate = 0;

    private static int _pendingGiftCount = 0;
    private static bool _pendingGiftCountLoaded = false;


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

    private static async UniTask<int> GetServerTodayYmdAsync()
    {
        try
        {
            var offsetRef = FirebaseDatabase.DefaultInstance.GetReference(". info/serverTimeOffset");
            var snapshot = await offsetRef.GetValueAsync();

            long offsetMs = 0;
            if (snapshot.Exists && snapshot.Value != null)
            {
                offsetMs = Convert.ToInt64(snapshot.Value);
            }

            var serverTime = DateTime.UtcNow.AddMilliseconds(offsetMs);
            return serverTime.Year * 10000 + serverTime.Month * 100 + serverTime.Day;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DreamEnergyGiftService] 서버 시간 가져오기 실패, 로컬 시간 사용: {e.Message}");
            return GetTodayYmd();
        }
    }

    public static bool HasSentTodayCached(string friendUid)
    {
        int today = GetTodayYmd();

        if (_sentTodayCacheDate != today)
            return false;

        return _sentTodayCache.Contains(friendUid);
    }

    public static int GetPendingGiftCountCached()
    {
        return _pendingGiftCountLoaded ? _pendingGiftCount : 0;
    }

    public static async UniTask<int> GetPendingGiftCountAsync()
    {
        string myUid = GetMyUid();
        if (string.IsNullOrEmpty(myUid))
            return 0;

        try
        {
            long thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeMilliseconds();

            var snap = await Root.Child("dreamGifts").Child(myUid)
                .OrderByChild("createdAt")
                .StartAt(thirtyDaysAgo)
                .GetValueAsync();

            if (!snap.Exists)
            {
                _pendingGiftCount = 0;
                _pendingGiftCountLoaded = true;
                return 0;
            }

            int count = 0;
            foreach (var child in snap.Children)
            {
                bool claimed = false;
                if (child.Child("claimed").Value is bool c)
                    claimed = c;

                if (!claimed)
                    count++;
            }

            _pendingGiftCount = count;
            _pendingGiftCountLoaded = true;

            Debug.Log($"[DreamEnergyGiftService] 받을 수 있는 선물: {count}개");
            return count;
        }
        catch (Exception e)
        {
            Debug.LogError($"[DreamEnergyGiftService] GetPendingGiftCountAsync Error: {e}");
            return 0;
        }
    }

    public static async UniTask<bool> TrySendDreamEnergyAsync(string friendUid)
    {
        if (_isSending)
        {
            Debug.Log("[DreamEnergyGiftService] 이미 전송 중입니다.");
            return false;
        }

        string myUid = GetMyUid();
        if (string.IsNullOrEmpty(myUid) || string.IsNullOrEmpty(friendUid))
            return false;
        if (myUid == friendUid)
            return false;

        if (SaveLoadManager.Data is not SaveDataV1 data)
            return false;

        _isSending = true;

        try
        {
            int today = await GetServerTodayYmdAsync();

            var alreadySentSnap = await Root
                .Child("sentGiftsToday")
                .Child(myUid)
                .Child(today.ToString())
                .Child(friendUid)
                .GetValueAsync();

            if (alreadySentSnap.Exists)
            {
                Debug.Log($"[DreamEnergyGiftService] 오늘 이미 {friendUid}에게 선물을 보냈습니다.");
                _sentTodayCache.Add(friendUid);
                return false;
            }

            var counterRef = Root.Child("userStats").Child(myUid).Child("dreamGiftCounter");

            bool transactionSuccess = false;
            int finalCount = 0;

            await counterRef.RunTransaction(mutableData =>
            {
                if (mutableData.Value == null)
                {
                    mutableData.Value = new Dictionary<string, object>
                    {
                        ["date"] = today,
                        ["count"] = 1
                    };
                    finalCount = 1;
                    transactionSuccess = true;
                    return TransactionResult.Success(mutableData);
                }

                var dict = mutableData.Value as Dictionary<string, object>;
                if (dict == null)
                {
                    mutableData.Value = new Dictionary<string, object>
                    {
                        ["date"] = today,
                        ["count"] = 1
                    };
                    finalCount = 1;
                    transactionSuccess = true;
                    return TransactionResult.Success(mutableData);
                }

                int savedDate = dict.ContainsKey("date") ? Convert.ToInt32(dict["date"]) : 0;
                int savedCount = dict.ContainsKey("count") ? Convert.ToInt32(dict["count"]) : 0;

                if (savedDate != today)
                {
                    dict["date"] = today;
                    dict["count"] = 1;
                    finalCount = 1;
                    transactionSuccess = true;
                    mutableData.Value = dict;
                    return TransactionResult.Success(mutableData);
                }

                if (savedCount >= data.dreamSendDailyLimit)
                {
                    Debug.Log($"[DreamEnergyGiftService] 일일 한도 도달: {savedCount}/{data.dreamSendDailyLimit}");
                    transactionSuccess = false;
                    return TransactionResult.Abort();
                }

                dict["count"] = savedCount + 1;
                finalCount = savedCount + 1;
                transactionSuccess = true;
                mutableData.Value = dict;
                return TransactionResult.Success(mutableData);
            });

            if (!transactionSuccess)
            {
                Debug.Log("[DreamEnergyGiftService] 오늘 보낼 수 있는 횟수를 모두 사용했습니다.");
                return false;
            }

            string key = Root.Child("dreamGifts").Child(friendUid).Push().Key;
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var updates = new Dictionary<string, object>
            {
                [$"dreamGifts/{friendUid}/{key}"] = new Dictionary<string, object>
                {
                    ["fromUid"] = myUid,
                    ["amount"] = GiftAmountPerSend,
                    ["createdAt"] = now,
                    ["claimed"] = false,
                },
                [$"sentGiftsToday/{myUid}/{today}/{friendUid}"] = now
            };

            await Root.UpdateChildrenAsync(updates);

            _sentTodayCache.Add(friendUid);
            _sentTodayCacheDate = today;

            data.dreamLastSendDate = today;
            data.dreamSendTodayCount = finalCount;
            await SaveLoadManager.SaveToServer();

            Debug.Log($"[DreamEnergyGiftService] 드림 에너지 선물 전송 완료: {friendUid} (오늘: {finalCount}회)");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[DreamEnergyGiftService] TrySendDreamEnergyAsync Error: {e}");
            return false;
        }
        finally
        {
            _isSending = false;
        }
    }

    public static async UniTask<int> ClaimAllGiftsAsync()
    {
        if (_isClaiming)
        {
            Debug.Log("[DreamEnergyGiftService] 이미 수령 중입니다.");
            return 0;
        }

        string myUid = GetMyUid();
        if (string.IsNullOrEmpty(myUid))
            return 0;

        if (SaveLoadManager.Data is not SaveDataV1 data)
            return 0;

        _isClaiming = true;
        int totalReceived = 0;

        try
        {
            long thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeMilliseconds();

            var snap = await Root.Child("dreamGifts").Child(myUid)
                .OrderByChild("createdAt")
                .StartAt(thirtyDaysAgo)
                .GetValueAsync();

            if (!snap.Exists)
            {
                Debug.Log("[DreamEnergyGiftService] 받을 선물이 없습니다.");
                _pendingGiftCount = 0;
                return 0;
            }

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
                var energyRef = Root.Child("userStats").Child(myUid).Child("dreamEnergy");

                await energyRef.RunTransaction(mutableData =>
                {
                    int currentEnergy = mutableData.Value != null ? Convert.ToInt32(mutableData.Value) : 0;
                    mutableData.Value = currentEnergy + totalReceived;
                    return TransactionResult.Success(mutableData);
                });

                ItemInvenHelper.AddItem(ItemID.DreamEnergy, totalReceived);

                if (LobbyManager.Instance != null)
                {
                    LobbyManager.Instance.MoneyUISet();
                }

                await SaveLoadManager.SaveToServer();

                if (updates.Count > 0)
                    await Root.UpdateChildrenAsync(updates);

                _pendingGiftCount = 0;

                Debug.Log($"[DreamEnergyGiftService] 드림 에너지 수령 완료: +{totalReceived}");
            }
            else
            {
                Debug.Log("[DreamEnergyGiftService] 받을 수 있는 선물이 없습니다.");
                _pendingGiftCount = 0;
            }

            return totalReceived;
        }
        catch (Exception e)
        {
            Debug.LogError($"[DreamEnergyGiftService] ClaimAllGiftsAsync Error: {e}");
            return 0;
        }
        finally
        {
            _isClaiming = false;
        }
    }

    public static async UniTask SyncCounterFromServerAsync()
    {
        string myUid = GetMyUid();
        if (string.IsNullOrEmpty(myUid))
            return;

        if (SaveLoadManager.Data is not SaveDataV1 data)
            return;

        try
        {
            int today = await GetServerTodayYmdAsync();

            var counterTask = Root.Child("userStats").Child(myUid).Child("dreamGiftCounter").GetValueAsync();
            var sentTodayTask = Root.Child("sentGiftsToday").Child(myUid).Child(today.ToString()).GetValueAsync();

            await UniTask.WhenAll(counterTask.AsUniTask(), sentTodayTask.AsUniTask());

            var counterSnap = counterTask.Result;
            var sentTodaySnap = sentTodayTask.Result;

            if (counterSnap.Exists)
            {
                var dict = counterSnap.Value as Dictionary<string, object>;
                if (dict != null)
                {
                    int savedDate = dict.ContainsKey("date") ? Convert.ToInt32(dict["date"]) : 0;
                    int savedCount = dict.ContainsKey("count") ? Convert.ToInt32(dict["count"]) : 0;

                    if (savedDate == today)
                    {
                        data.dreamLastSendDate = today;
                        data.dreamSendTodayCount = savedCount;
                    }
                    else
                    {
                        data.dreamLastSendDate = today;
                        data.dreamSendTodayCount = 0;
                    }
                }
            }

            _sentTodayCache.Clear();
            _sentTodayCacheDate = today;

            if (sentTodaySnap.Exists)
            {
                foreach (var child in sentTodaySnap.Children)
                {
                    _sentTodayCache.Add(child.Key);
                }
            }

            await GetPendingGiftCountAsync();

            Debug.Log($"[DreamEnergyGiftService] 동기화 완료 - 오늘 보낸 수: {data.dreamSendTodayCount}, 보낸 친구: {_sentTodayCache.Count}명, 받을 선물: {_pendingGiftCount}개");
        }
        catch (Exception e)
        {
            Debug.LogError($"[DreamEnergyGiftService] SyncCounterFromServerAsync Error: {e}");
        }
    }

    public static async UniTask CleanupOldGiftsAsync()
    {
        string myUid = GetMyUid();
        if (string.IsNullOrEmpty(myUid))
            return;

        try
        {
            long thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeMilliseconds();

            var snap = await Root.Child("dreamGifts").Child(myUid)
                .OrderByChild("createdAt")
                .EndAt(thirtyDaysAgo)
                .GetValueAsync();

            if (!snap.Exists) return;

            var updates = new Dictionary<string, object>();
            foreach (var child in snap.Children)
            {
                updates[$"dreamGifts/{myUid}/{child.Key}"] = null;
            }

            if (updates.Count > 0)
            {
                await Root.UpdateChildrenAsync(updates);
                Debug.Log($"[DreamEnergyGiftService] {updates.Count}개의 오래된 선물 삭제 완료");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[DreamEnergyGiftService] CleanupOldGiftsAsync Error: {e}");
        }
    }

    /// <summary>
    /// 특정 친구에게 받은 선물만 수령
    /// </summary>
    public static async UniTask<int> ClaimGiftFromFriendAsync(string fromUid)
    {
        if (_isClaiming)
        {
            Debug.Log("[DreamEnergyGiftService] 이미 수령 중입니다.");
            return 0;
        }

        string myUid = GetMyUid();
        if (string.IsNullOrEmpty(myUid) || string.IsNullOrEmpty(fromUid))
            return 0;

        if (SaveLoadManager.Data is not SaveDataV1 data)
            return 0;

        _isClaiming = true;
        int totalReceived = 0;

        try
        {
            long thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeMilliseconds();

            var snap = await Root.Child("dreamGifts").Child(myUid)
                .OrderByChild("createdAt")
                .StartAt(thirtyDaysAgo)
                .GetValueAsync();

            if (!snap.Exists)
            {
                Debug.Log("[DreamEnergyGiftService] 받을 선물이 없습니다.");
                return 0;
            }

            var updates = new Dictionary<string, object>();

            foreach (var child in snap.Children)
            {
                // fromUid가 일치하는 것만 처리
                string giftFromUid = child.Child("fromUid").Value?.ToString();
                if (giftFromUid != fromUid)
                    continue;

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
                var energyRef = Root.Child("userStats").Child(myUid).Child("dreamEnergy");

                await energyRef.RunTransaction(mutableData =>
                {
                    int currentEnergy = mutableData.Value != null ? Convert.ToInt32(mutableData.Value) : 0;
                    mutableData.Value = currentEnergy + totalReceived;
                    return TransactionResult.Success(mutableData);
                });

                ItemInvenHelper.AddItem(ItemID.DreamEnergy, totalReceived);

                if (LobbyManager.Instance != null)
                    LobbyManager.Instance.MoneyUISet();

                await SaveLoadManager.SaveToServer();

                if (updates.Count > 0)
                    await Root.UpdateChildrenAsync(updates);

                // 캐시 갱신
                if (_pendingGiftsByFriend.ContainsKey(fromUid))
                    _pendingGiftsByFriend[fromUid] = 0;

                _pendingGiftCount = Math.Max(0, _pendingGiftCount - updates.Count);

                Debug.Log($"[DreamEnergyGiftService] {fromUid}에게 받은 선물 수령 완료: +{totalReceived}");
            }
            else
            {
                Debug.Log($"[DreamEnergyGiftService] {fromUid}에게 받은 선물이 없습니다.");
            }

            return totalReceived;
        }
        catch (Exception e)
        {
            Debug.LogError($"[DreamEnergyGiftService] ClaimGiftFromFriendAsync Error: {e}");
            return 0;
        }
        finally
        {
            _isClaiming = false;
        }
    }

    /// <summary>
    /// 특정 친구에게 받을 선물이 있는지 확인 (캐시용)
    /// </summary>
    private static Dictionary<string, int> _pendingGiftsByFriend = new();

    public static int GetPendingGiftCountFromFriend(string fromUid)
    {
        return _pendingGiftsByFriend.TryGetValue(fromUid, out int count) ? count : 0;
    }

    /// <summary>
    /// 친구별 받을 선물 수 캐시 갱신
    /// </summary>
    public static async UniTask RefreshPendingGiftsByFriendAsync()
    {
        string myUid = GetMyUid();
        if (string.IsNullOrEmpty(myUid))
            return;

        _pendingGiftsByFriend.Clear();

        try
        {
            long thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeMilliseconds();

            var snap = await Root.Child("dreamGifts").Child(myUid)
                .OrderByChild("createdAt")
                .StartAt(thirtyDaysAgo)
                .GetValueAsync();

            if (!snap.Exists)
            {
                Debug.Log("[DreamEnergyGiftService] dreamGifts 데이터 없음");  // ← 추가
                _pendingGiftCount = 0;
                _pendingGiftCountLoaded = true;
                return;
            }

            int totalCount = 0;

            foreach (var child in snap.Children)
            {
                bool claimed = false;
                if (child.Child("claimed").Value is bool c)
                    claimed = c;

                // 디버그 추가
                string fromUid = child.Child("fromUid").Value?.ToString();
                Debug.Log($"[DEBUG] 선물 key={child.Key}, fromUid={fromUid}, claimed={claimed}");

                if (claimed)
                    continue;

                if (string.IsNullOrEmpty(fromUid))
                {
                    Debug.LogWarning($"[DEBUG] fromUid가 없는 선물 발견: {child.Key}");  // ← 이거 찍히면 문제!
                    continue;
                }

                if (!_pendingGiftsByFriend.ContainsKey(fromUid))
                    _pendingGiftsByFriend[fromUid] = 0;

                _pendingGiftsByFriend[fromUid]++;
                totalCount++;
            }

            _pendingGiftCount = totalCount;
            _pendingGiftCountLoaded = true;

            Debug.Log($"[DreamEnergyGiftService] 친구별 선물 캐시 갱신: {_pendingGiftsByFriend.Count}명에게서 총 {totalCount}개");

            // 캐시 내용 출력
            foreach (var kvp in _pendingGiftsByFriend)
            {
                Debug.Log($"[DEBUG] 캐시: {kvp.Key} = {kvp.Value}개");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[DreamEnergyGiftService] RefreshPendingGiftsByFriendAsync Error: {e}");
        }
    }
}
