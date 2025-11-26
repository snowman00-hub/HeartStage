using Cysharp.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    private static AuthManager instance;
    public static AuthManager Instance => instance;

    private FirebaseAuth auth = null;
    private FirebaseUser currentUser = null;
    private bool isInitialized = false;

    public FirebaseUser CurrentUser => currentUser;
    public bool IsLoggedIn => currentUser != null;
    public string UserId => currentUser?.UserId ?? string.Empty;
    public bool IsInitialized => isInitialized;

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
    }

    private async UniTaskVoid Start()
    {
        // 초기화 기다리기
        await FirebaseInitializer.Instance.WaitForInitilazationAsync();
                
        auth = FirebaseAuth.DefaultInstance;
        // Firebase Auth 내부 상태가 바뀔 때 (로그인, 로그아웃, currentUser 내부 값 변경)
        auth.StateChanged += OnAuthStateChanger;

        currentUser = auth.CurrentUser;

        if (currentUser != null)
        {
            Debug.Log($"[Auth] 이미 로그인됨: {UserId}");
        }
        else
        {
            Debug.Log($"[Auth] 로그인 필요");
        }

        isInitialized = true;
    }

    private void OnDestroy()
    {
        if (auth != null)
        {
            auth.StateChanged -= OnAuthStateChanger;
        }
    }


    // 익명 로그인 시도
    public async UniTask<(bool success, string error)> SignInAnonymouslyAsync()
    {
        try
        {
            AuthResult result = await auth.SignInAnonymouslyAsync().AsUniTask();
            currentUser = result.User;
            Debug.Log($"[Auth] 익명 로그인 성공: {UserId}");
            return (true, null);
        }
        catch (System.Exception ex)
        {
            Debug.Log($"[Auth] 익명 로그인 실패: {ex.Message}");
            return (false, ex.Message);
        }
    }

    // 회원가입 시도
    public async UniTask<(bool success, string error)> CreateUserWithEmailAsync(string email, string passwd)
    {
        // 회원가입 형식 안 맞추면 false
        // 이메일 형식 맞추기 aaa@bbb.com
        // 비번은 6자 이상
        try
        {
            AuthResult result = await auth.CreateUserWithEmailAndPasswordAsync(email, passwd).AsUniTask();
            currentUser = result.User;
            Debug.Log($"[Auth] 회원 가입 성공: {UserId}");
            return (true, null);
        }
        catch (System.Exception ex)
        {
            Debug.Log($"[Auth] 회원 가입 실패: {ex.Message}");
            return (false, ex.Message);
        }
    }

    // 로그인 시도
    public async UniTask<(bool success, string error)> SignInWithEmailAsync(string email, string passwd)
    {
        try
        {
            AuthResult result = await auth.SignInWithEmailAndPasswordAsync(email, passwd).AsUniTask();
            currentUser = result.User;
            Debug.Log($"[Auth] 로그인(이메일) 성공: {UserId}");
            return (true, null);
        }
        catch (System.Exception ex)
        {
            Debug.Log($"[Auth] 로그인(이메일) 실패: {ex.Message}");
            return (false, ex.Message);
        }
    }

    // 로그아웃 시도
    public void SignOut()
    {
        if (auth != null && currentUser != null)
        {
            Debug.Log("[Auth] 로그아웃");
            auth.SignOut();
            currentUser = null;

            // 세이브 데이터 초기화
            SaveLoadManager.ResetData();
            // 로그아웃 후 BootScene으로 이동
            SceneManager.LoadScene(0);
        }
    }

    private void OnAuthStateChanger(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != currentUser)
        {
            bool signedIn = auth.CurrentUser != null;
            if (!signedIn && currentUser != null)
            {
                Debug.Log("[Auth] 로그 아웃 됨");
            }

            currentUser = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log($"[Auth] 이미 로그인됨: {UserId}");
            }
        }
    }
}