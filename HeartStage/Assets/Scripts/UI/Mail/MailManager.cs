using Cysharp.Threading.Tasks;
using Firebase.Database;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MailManager : MonoBehaviour
{
    public static MailManager Instance;

    private DatabaseReference db;

    public event Action<List<MailData>> OnMailsLoaded;
    public event Action<MailData> OnMailReceived;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        await FirebaseInitializer.Instance.WaitForInitilazationAsync();
        db = FirebaseDatabase.DefaultInstance.RootReference;


        // 테스트
        await CreateTestMail();
    }


    public async UniTask<List<MailData>> GetUserMailsAsync(string userId) // 네트워크 작업은 비동기로 
    {
        try
        {
            var snapshot = await db.Child("mails").Child(userId).GetValueAsync();
            var mails = new List<MailData>();

            if (snapshot.Exists)
            {
                foreach (var child in snapshot.Children)
                {
                    string json = child.GetRawJsonValue();
                    MailData mail = JsonUtility.FromJson<MailData>(json);
                    mails.Add(mail);
                }

                mails = mails.OrderByDescending(m => m.timestamp).ToList(); // 최신 메일이 위로 오도록 정렬
            }

            OnMailsLoaded?.Invoke(mails);
            return mails;
        }

        catch(Exception ex)
        {
            Debug.LogError($"메일 로드 실패: {ex.Message}");
            return new List<MailData>();
        }
    }

    public async UniTask<bool> SendMailAsync(MailData mailData)
    {
        try
        {
            string json = JsonUtility.ToJson(mailData);
            await db.Child("mails").Child(mailData.receiverId).Child(mailData.mailId).SetRawJsonValueAsync(json);
            
            OnMailReceived?.Invoke(mailData);
            Debug.Log("메일 전송 성공");
            return true;
        }
        catch(Exception ex)
        {
            Debug.LogError($"메일 전송 실패: {ex.Message}");
            return false;
        }
    }

    // 메일 읽음 처리
    public async UniTask MarkAsReadAsync(string userId, string mailId)
    {
        try
        {
            await db.Child("mails").Child(userId).Child(mailId).Child("isRead").SetValueAsync(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"메일 읽음 처리 실패: {ex.Message}");
        }
    }

    // 메일 삭제
    public async UniTask DeleteMailAsync(string userId, string mailId)
    {
        try
        {
            await db.Child("mails").Child(userId).Child(mailId).RemoveValueAsync();
            Debug.Log("메일 삭제 완료");
        }
        catch (Exception ex)
        {
            Debug.LogError($"메일 삭제 실패: {ex.Message}");
        }
    }

    public async UniTask UpdateRewardStatusAsync(string userId, string mailId, bool isRewarded)
    {
        try
        {
            await db.Child("mails").Child(userId).Child(mailId).Child("isRewarded").SetValueAsync(isRewarded);
            Debug.Log($"보상 수령 상태 업데이트: {mailId} - {isRewarded}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"보상 상태 업데이트 실패: {ex.Message}");
        }
    }

    public async UniTask UpdateMultipleRewardStatusAsync(string userId, List<string> mailIds)
    {
        try
        {
            var updates = new Dictionary<string, object>();
            foreach (string mailId in mailIds)
            {
                updates[$"mails/{userId}/{mailId}/isRewarded"] = true;
            }

            await db.UpdateChildrenAsync(updates);
            Debug.Log($"다중 보상 상태 업데이트 완료: {mailIds.Count}개");
        }
        catch (Exception ex)
        {
            Debug.LogError($"다중 보상 상태 업데이트 실패: {ex.Message}");
        }
    }

    //테스트 
    public async UniTask CreateTestMail()
    {
        string userId = AuthManager.Instance.UserId;

        var testItems = new List<ItemAttachment>
    {
        new ItemAttachment("7101", 100), // 라이트스틱 100개
        new ItemAttachment("7102", 50),  // 하트스틱 50개
        new ItemAttachment("7104", 10)   // 드림에너지 10개
    };

        var testMail = new MailData(
            mailId: System.Guid.NewGuid().ToString(),
            senderId: "admin",
            senderName: "관리자",
            receiverId: userId,
            title: "환영 선물!",
            content: "게임에 오신 것을 환영합니다! 선물을 받아주세요.",
            itemList: testItems
        );

        await MailManager.Instance.SendMailAsync(testMail);
    }
}