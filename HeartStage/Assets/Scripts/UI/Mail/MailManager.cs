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
}