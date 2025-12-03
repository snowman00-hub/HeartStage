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
            return false;
        if (myUid == targetUid)
            return false;

        try
        {
            // 이미 친구면 패스
            var myFriendRef = Root.Child("friends").Child(myUid).Child(targetUid);
            var snap = await myFriendRef.GetValueAsync();
            if (snap.Exists)
            {
                Debug.Log("[FriendService] 이미 친구 상태입니다.");
                return false;
            }

            await Root.Child("friendRequests").Child(targetUid).Child(myUid)
                .SetValueAsync(true);

            Debug.Log("[FriendService] 친구 요청 전송 완료");
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
        string myUid = GetMyUid();
        if (string.IsNullOrEmpty(myUid) || string.IsNullOrEmpty(fromUid))
            return false;

        try
        {
            var updates = new Dictionary<string, object>
            {
                [$"friends/{myUid}/{fromUid}"] = true,
                [$"friends/{fromUid}/{myUid}"] = true,
                [$"friendRequests/{myUid}/{fromUid}"] = null,
            };

            await Root.UpdateChildrenAsync(updates);

            if (SaveLoadManager.Data is SaveDataV1 data)
            {
                if (!data.friendUidList.Contains(fromUid))
                    data.friendUidList.Add(fromUid);

                await SaveLoadManager.SaveToServer();
            }

            Debug.Log("[FriendService] 친구 요청 수락 완료");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendService] AcceptFriendRequestAsync Error: {e}");
            return false;
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
            Debug.Log("[FriendService] 친구 요청 거절 완료");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendService] DeclineFriendRequestAsync Error: {e}");
            return false;
        }
    }

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

            // 로컬 세이브와 동기화 옵션
            if (syncLocal && SaveLoadManager.Data is SaveDataV1 data)
            {
                data.friendUidList.Clear();
                data.friendUidList.AddRange(result);
                // 굳이 여기서 SaveToServer까지 안 해도 됨 (friends가 진짜 소스라서)
                // 필요하면 아래 주석 해제
                // await SaveLoadManager.SaveToServer();
            }
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

            Debug.Log("[FriendService] 친구 삭제 완료");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendService] RemoveFriendAsync Error: {e}");
            return false;
        }
    }
}
