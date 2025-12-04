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

    // 동시성 제어용
    private static bool _isSending = false;
    private static bool _isClaiming = false;

    private static string GetMyUid()
    {
        var user = Auth.CurrentUser;
        return user?.UserId;
    }

    /// <summary>
    /// 서버 시간 기반으로 오늘 날짜(YYYYMMDD) 가져오기
    /// </summary>
    private static async UniTask<int> GetServerTodayYmdAsync()
    {
        try
        {
            var snapshot = await Root.Child(".info/serverTimeOffset").GetValueAsync();
            long offsetMs = snapshot.Exists ? (long)snapshot.Value : 0;
            var serverTime = DateTime.UtcNow.AddMilliseconds(offsetMs);
            return serverTime.Year * 10000 + serverTime.Month * 100 + serverTime.Day;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DreamEnergyGiftService] 서버 시간 가져오기 실패, 로컬 시간 사용: {e.Message}");
            var now = DateTime.Now;
            return now.Year * 10000 + now.Month * 100 + now.Day;
        }
    }

    /// <summary>
    /// Firebase Transaction을 사용한 안전한 드림 에너지 선물 전송
    /// 카운터를 서버에서 원자적으로 증가시켜 Race Condition 방지
    /// </summary>
    public static async UniTask<bool> TrySendDreamEnergyAsync(string friendUid)
    {
        // 동시 전송 방지 (UI 레벨)
        if (_isSending)
        {
            Debug.Log("[DreamEnergyGiftService] 이미 전송 중입니다. 잠시 후 다시 시도해주세요.");
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

            // Firebase에 카운터 경로 (userStats/{uid}/dreamGiftCounter)
            var counterRef = Root.Child("userStats").Child(myUid).Child("dreamGiftCounter");

            // Transaction으로 원자적으로 카운터 증가
            bool transactionSuccess = false;
            int finalCount = 0;

            await counterRef.RunTransaction(mutableData =>
            {
                if (mutableData.Value == null)
                {
                    // 첫 전송
                    mutableData.Value = new Dictionary<string, object>
                    {
                        ["date"] = today,
                        ["count"] = 1
                    };
                    finalCount = 1;
                    transactionSuccess = true;
                    return TransactionResult.Success(mutableData);
                }

                // 기존 데이터 읽기
                var dict = mutableData.Value as Dictionary<string, object>;
                if (dict == null)
                {
                    // 데이터 형식 오류 - 초기화
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

                // 날짜가 바뀌었으면 카운트 리셋
                if (savedDate != today)
                {
                    dict["date"] = today;
                    dict["count"] = 1;
                    finalCount = 1;
                    transactionSuccess = true;
                    mutableData.Value = dict;
                    return TransactionResult.Success(mutableData);
                }

                // 일일 한도 체크
                if (savedCount >= data.dreamSendDailyLimit)
                {
                    Debug.Log($"[DreamEnergyGiftService] 일일 한도 도달: {savedCount}/{data.dreamSendDailyLimit}");
                    transactionSuccess = false;
                    return TransactionResult.Abort();
                }

                // 카운트 증가
                dict["count"] = savedCount + 1;
                finalCount = savedCount + 1;
                transactionSuccess = true;
                mutableData.Value = dict;
                return TransactionResult.Success(mutableData);
            });

            // Transaction 실패 시
            if (!transactionSuccess)
            {
                Debug.Log("[DreamEnergyGiftService] 오늘 보낼 수 있는 횟수를 모두 사용했습니다.");
                return false;
            }

            // Transaction 성공 - 선물 데이터 저장
            string key = Root.Child("dreamGifts").Child(friendUid).Push().Key;
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var giftDict = new Dictionary<string, object>
            {
                ["fromUid"] = myUid,
                ["amount"] = GiftAmountPerSend,
                ["createdAt"] = now,
                ["claimed"] = false,
            };

            await Root.Child("dreamGifts").Child(friendUid).Child(key).SetValueAsync(giftDict);

            // 로컬 데이터 동기화 (표시용)
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

    /// <summary>
    /// 내가 받은 선물 전부 수령 → dreamEnergy에 합산
    /// </summary>
    public static async UniTask<int> ClaimAllGiftsAsync()
    {
        // 동시 수령 방지
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
            // 최근 30일 이내 선물만 조회 (성능 최적화)
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
                bool claimed = false;
                int amount = 0;

                if (child.Child("claimed").Value is bool c)
                    claimed = c;
                if (child.Child("amount").Value is long a)
                    amount = (int)a;

                // 아직 안 받은 선물만 처리
                if (!claimed && amount > 0)
                {
                    totalReceived += amount;
                    updates[$"dreamGifts/{myUid}/{child.Key}/claimed"] = true;
                }
            }

            // 받은 게 있으면 처리
            if (totalReceived > 0)
            {
                // Firebase Transaction으로 안전하게 dreamEnergy 증가
                var energyRef = Root.Child("userStats").Child(myUid).Child("dreamEnergy");

                await energyRef.RunTransaction(mutableData =>
                {
                    int currentEnergy = mutableData.Value != null ? Convert.ToInt32(mutableData.Value) : 0;
                    mutableData.Value = currentEnergy + totalReceived;
                    return TransactionResult.Success(mutableData);
                });

                // 로컬 데이터 동기화
                data.dreamEnergy += totalReceived;
                await SaveLoadManager.SaveToServer();

                // claimed 플래그 업데이트
                if (updates.Count > 0)
                    await Root.UpdateChildrenAsync(updates);

                Debug.Log($"[DreamEnergyGiftService] 드림 에너지 수령 완료: +{totalReceived}");
            }
            else
            {
                Debug.Log("[DreamEnergyGiftService] 받을 수 있는 선물이 없습니다.");
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

    /// <summary>
    /// 서버의 카운터 값을 로컬과 동기화 (앱 시작 시 호출 권장)
    /// </summary>
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
            var counterRef = Root.Child("userStats").Child(myUid).Child("dreamGiftCounter");
            var snap = await counterRef.GetValueAsync();

            if (!snap.Exists)
            {
                // 서버에 카운터 없음 - 로컬 데이터 유지
                return;
            }

            var dict = snap.Value as Dictionary<string, object>;
            if (dict == null)
                return;

            int savedDate = dict.ContainsKey("date") ? Convert.ToInt32(dict["date"]) : 0;
            int savedCount = dict.ContainsKey("count") ? Convert.ToInt32(dict["count"]) : 0;

            // 날짜가 오늘이면 카운트 동기화
            if (savedDate == today)
            {
                data.dreamLastSendDate = today;
                data.dreamSendTodayCount = savedCount;
                Debug.Log($"[DreamEnergyGiftService] 카운터 동기화 완료: {savedCount}/{data.dreamSendDailyLimit}");
            }
            else
            {
                // 날짜가 바뀌었으면 리셋
                data.dreamLastSendDate = today;
                data.dreamSendTodayCount = 0;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[DreamEnergyGiftService] SyncCounterFromServerAsync Error: {e}");
        }
    }

    /// <summary>
    /// 30일 이상 지난 선물 데이터 정리 (관리자용 또는 주기적 호출)
    /// </summary>
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
}