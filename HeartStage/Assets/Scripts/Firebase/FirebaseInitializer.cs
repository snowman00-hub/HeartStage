using UnityEngine;
using Firebase;
using Cysharp.Threading.Tasks;

public class FirebaseInitializer : MonoBehaviour
{
    private static FirebaseInitializer instance;
    public static FirebaseInitializer Instance => instance;

    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    private FirebaseApp firebaseApp;
    public FirebaseApp FirebaseApp => firebaseApp;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeFirebaseAsync().Forget();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // Firebase가 사용할 준비 되어있는지 체크
    private async UniTaskVoid InitializeFirebaseAsync()
    {
        try
        {
            var status = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();
            if (status == DependencyStatus.Available)
            {
                firebaseApp = FirebaseApp.DefaultInstance;
                isInitialized = true;
            }
            else
            {
                Debug.LogError($"[Firebase] 초기화 오류: {status}");
                isInitialized = false;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Firebase] 초기화 오류: {ex.Message}");
            isInitialized = false;
        }
    }

    // 다른 스크립트에서 await해서 안전하게 초기화 기다리기
    public async UniTask WaitForInitilazationAsync()
    {
        await UniTask.WaitUntil(() => isInitialized);
    }
}