using Firebase.Database;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CloudSaveManager : MonoBehaviour
{
    public static CloudSaveManager Instance;

    private DatabaseReference db;

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

    private async UniTask Start()
    {
        await FirebaseInitializer.Instance.WaitForInitilazationAsync();
        db = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // 서버에 저장
    public async UniTask SaveAsync(string userId, string json)
    {
        await db.Child("users").Child(userId).Child("saveData").SetValueAsync(json);
        Debug.Log("클라우드 세이브 저장 완료");
    }

    // 서버에서 로드
    public async UniTask<string> LoadAsync(string userId)
    {
        var snapshot = await db.Child("users").Child(userId).Child("saveData").GetValueAsync();

        if (snapshot.Exists)
            return snapshot.Value.ToString();

        return null;
    }
}
