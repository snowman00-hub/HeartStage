using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class FriendService
{
    private static DatabaseReference Root => FirebaseDatabase.DefaultInstance.RootReference;
    private static FirebaseAuth Auth => FirebaseAuth.DefaultInstance;

    // 최대 친구 수 제한
    public const int MAX_FRIEND_COUNT = 20;

    // 동시성 제어
    private static bool _isProcessingRequest = false;

    private static string GetMyUid()
    {
        var user = Auth.CurrentUser;
        return user?.UserId;
    }

    /// <summary>
    /// 상대 uid로 친구 요청 보내기
    /// friendRequests/targetUid/myUid = true
    /// </summary>
    public static async UniTask<bool> SendFriendRequestAsync(string targetUid)
    {
        string myUid = GetMyUid();
        if (string.IsNullOrEmpty(myUid) || string.IsNullOrEmpty(targetUid))
        {
            Debug.LogWarning("[FriendService] 유효하지 않은 UID입니다.");
            return false;
        }

        if (myUid == targetUid)
        {
            Debug.LogWarning("[FriendService] 자기 자신에게 친구 요청을 보낼 수 없습니다.");
            return false;
        }

        try
        {
            // 이미 친구인지 확인
            var myFriendRef = Root.Child("friends").Child(myUid).Child(targetUid);
            var snap = await myFriendRef.GetValueAsync();
            if (snap.Exists)
            {
                Debug.Log("[FriendService] 이미 친구 상태입니다.");
                return false;
            }

            // 이미 요청을 보냈는지 확인
            var requestRef = Root.Child("friendRequests").Child(targetUid).Child(myUid);
            var requestSnap = await requestRef.GetValueAsync();
            if (requestSnap.Exists)
            {
                Debug.Log("[FriendService] 이미 친구 요청을 보냈습니다.");
                return false;
            }

            // 친구 요청 전송
            await requestRef.SetValueAsync(true);

            var updates = new Dictionary<string, object>
            {
                [$"friendRequests/{targetUid}/{myUid}"] = true,
                [$"sentRequests/{myUid}/{targetUid}"] = true,
            };

            await Root.UpdateChildrenAsync(updates);

            Debug.Log($"[FriendService] 친구 요청 전송 완료: {targetUid}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendService] SendFriendRequestAsync Error: {e}");
            return false;
        }
    }

    /// <summary>
    /// 내가 받은 친구 요청 목록(uid 리스트)
    /// friendRequests/myUid/ 밑의 key들
    /// </summary>
    public static async UniTask<List<string>> GetReceivedRequestsAsync()
    {
        string myUid = GetMyUid();
        var result = new List<string>();
        if (string.IsNullOrEmpty(myUid))
            return result;

        try
        {
            var snap = await Root.Child("friendRequests").Child(myUid).GetValueAsync();
            if (!snap.Exists) return result;

            foreach (var child in snap.Children)
            {
                string fromUid = child.Key;
                result.Add(fromUid);
            }

            Debug.Log($"[FriendService] 받은 친구 요청: {result.Count}개");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendService] GetReceivedRequestsAsync Error: {e}");
        }
        return result;
    }

    /// <summary>
    /// 친구 요청 수락
    /// friends 양쪽 추가 + friendRequests 삭제 + SaveData.friendUidList 추가
    /// </summary>
    public static async UniTask<bool> AcceptFriendRequestAsync(string fromUid)
    {
        if (_isProcessingRequest)
        {
            Debug.Log("[FriendService] 이미 요청 처리 중입니다.");
            return false;
        }

        string myUid = GetMyUid();
        if (string.IsNullOrEmpty(myUid) || string.IsNullOrEmpty(fromUid))
            return false;

        _isProcessingRequest = true;

        try
        {
            if (SaveLoadManager.Data is not SaveDataV1 data)
                return false;

            // 친구 수 한도 체크
            if (data.friendUidList.Count >= MAX_FRIEND_COUNT)
            {
                Debug.Log($"[FriendService] 친구 수가 최대치({MAX_FRIEND_COUNT}명)입니다.");
                return false;
            }

            // 이미 친구인지 확인
            if (data.friendUidList.Contains(fromUid))
            {
                Debug.Log("[FriendService] 이미 친구 상태입니다.");
                // 요청은 삭제
                await Root.Child("friendRequests").Child(myUid).Child(fromUid).SetValueAsync(null);
                return false;
            }

            // Firebase 업데이트 (friends 양방향 + 요청 삭제)
            var updates = new Dictionary<string, object>
            {
                [$"friends/{myUid}/{fromUid}"] = true,
                [$"friends/{fromUid}/{myUid}"] = true,
                [$"friendRequests/{myUid}/{fromUid}"] = null,
            };

            await Root.UpdateChildrenAsync(updates);

            // 로컬 데이터 업데이트
            data.friendUidList.Add(fromUid);
            await SaveLoadManager.SaveToServer();

            Debug.Log($"[FriendService] 친구 요청 수락 완료: {fromUid}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendService] AcceptFriendRequestAsync Error: {e}");
            return false;
        }
        finally
        {
            _isProcessingRequest = false;
        }
    }

    /// <summary>
    /// 친구 요청 거절
    /// </summary>
    public static async UniTask<bool> DeclineFriendRequestAsync(string fromUid)
    {
        string myUid = GetMyUid();
        if (string.IsNullOrEmpty(myUid) || string.IsNullOrEmpty(fromUid))
            return false;

        try
        {
            await Root.Child("friendRequests").Child(myUid).Child(fromUid)
                .SetValueAsync(null);

            Debug.Log($"[FriendService] 친구 요청 거절 완료: {fromUid}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendService] DeclineFriendRequestAsync Error: {e}");
            return false;
        }
    }

    /// <summary>
    /// 내 친구 목록 가져오기 (서버 기준)
    /// </summary>
    public static async UniTask<List<string>> GetMyFriendUidListAsync(bool syncLocal = true)
    {
        var result = new List<string>();

        string myUid = GetMyUid();
        if (string.IsNullOrEmpty(myUid))
            return result;

        try
        {
            var snap = await Root.Child("friends").Child(myUid).GetValueAsync();
            if (snap.Exists)
            {
                foreach (var child in snap.Children)
                {
                    string friendUid = child.Key;
                    if (!string.IsNullOrEmpty(friendUid))
                        result.Add(friendUid);
                }
            }

            // 로컬 세이브와 동기화
            if (syncLocal && SaveLoadManager.Data is SaveDataV1 data)
            {
                data.friendUidList.Clear();
                data.friendUidList.AddRange(result);
                // 굳이 여기서 SaveToServer까지 안 해도 됨 (friends가 진짜 소스라서)
            }

            Debug.Log($"[FriendService] 친구 목록 로드 완료: {result.Count}명");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendService] GetMyFriendUidListAsync Error: {e}");
        }

        return result;
    }

    /// <summary>
    /// 친구 삭제 (friends 양쪽 제거 + SaveData.friendUidList에서 제거)
    /// </summary>
    public static async UniTask<bool> RemoveFriendAsync(string friendUid)
    {
        string myUid = GetMyUid();
        if (string.IsNullOrEmpty(myUid) || string.IsNullOrEmpty(friendUid))
            return false;

        try
        {
            var updates = new Dictionary<string, object>
            {
                [$"friends/{myUid}/{friendUid}"] = null,
                [$"friends/{friendUid}/{myUid}"] = null,
            };

            await Root.UpdateChildrenAsync(updates);

            if (SaveLoadManager.Data is SaveDataV1 data)
            {
                data.friendUidList.Remove(friendUid);
                await SaveLoadManager.SaveToServer();
            }

            Debug.Log($"[FriendService] 친구 삭제 완료: {friendUid}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendService] RemoveFriendAsync Error: {e}");
            return false;
        }
    }

    /// <summary>
    /// 친구 수 체크 (로컬 기준)
    /// </summary>
    public static bool CanAddMoreFriends()
    {
        if (SaveLoadManager.Data is not SaveDataV1 data)
            return false;

        return data.friendUidList.Count < MAX_FRIEND_COUNT;
    }

    /// <summary>
    /// 내가 보낸 친구 요청 목록
    /// </summary>
    public static async UniTask<List<string>> GetSentRequestsAsync()
    {
        string myUid = GetMyUid();
        var result = new List<string>();
        if (string.IsNullOrEmpty(myUid))
            return result;

        try
        {
            var snap = await Root.Child("sentRequests").Child(myUid).GetValueAsync();
            if (!snap.Exists) return result;

            foreach (var child in snap.Children)
            {
                string toUid = child.Key;
                result.Add(toUid);
            }

            Debug.Log($"[FriendService] 보낸 친구 요청: {result.Count}개");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendService] GetSentRequestsAsync Error: {e}");
        }
        return result;
    }

    /// <summary>
    /// 보낸 친구 요청 취소
    /// </summary>
    public static async UniTask<bool> CancelSentRequestAsync(string toUid)
    {
        string myUid = GetMyUid();
        if (string.IsNullOrEmpty(myUid) || string.IsNullOrEmpty(toUid))
            return false;

        try
        {
            var updates = new Dictionary<string, object>
            {
                [$"friendRequests/{toUid}/{myUid}"] = null,
                [$"sentRequests/{myUid}/{toUid}"] = null,
            };

            await Root.UpdateChildrenAsync(updates);

            Debug.Log($"[FriendService] 보낸 요청 취소 완료: {toUid}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendService] CancelSentRequestAsync Error: {e}");
            return false;
        }
    }
}