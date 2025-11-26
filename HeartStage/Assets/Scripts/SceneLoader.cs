using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("로딩 UI 프리팹")]
    [SerializeField] private LoadingUI loadingUIPrefab;

    private LoadingUI _loadingUI;

    private float _currentProgress = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (loadingUIPrefab != null)
        {
            _loadingUI = Instantiate(loadingUIPrefab, transform);
            _loadingUI.Hide();
        }
    }

    /// <summary>
    /// Addressables 주소 기반으로 씬 로딩 + 로딩 패널 표시.
    /// 예: await SceneLoader.LoadSceneWithLoading("StageScene", LoadSceneMode.Single);
    /// </summary>
    public static UniTask LoadSceneWithLoading(string address, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (Instance == null)
        {
            Debug.LogError("[SceneLoader] Instance 없음. 부트스트랩 씬에 SceneLoader 배치 필요.");
            return UniTask.CompletedTask;
        }

        return Instance.InternalLoadScene(address, mode);
    }

    private async UniTask InternalLoadScene(string address, LoadSceneMode mode)
    {
        _loadingUI?.Show();
        SetProgressInternal(0f);

        var handle = Addressables.LoadSceneAsync(address, mode, activateOnLoad: false);

        // 🔹 Addressables 로딩 단계: 0.0 ~ 0.9까지만 사용
        while (!handle.IsDone)
        {
            float p = handle.PercentComplete * 0.9f;
            SetProgressInternal(p);
            await UniTask.Yield();
        }

        var sceneInstance = handle.Result;
        var activateOp = sceneInstance.ActivateAsync();

        // 🔹 활성화 단계에서는 굳이 계속 만지지 않고,
        //    최대 0.9까지만 유지 (혹시라도 0.8→0.9 올라갈 수는 있음)
        while (!activateOp.isDone)
        {
            float p = 0.9f; // 또는 0.9f까지 부드럽게 보간해도 됨
            SetProgressInternal(p);
            await UniTask.Yield();
        }
    }


    public static void HideLoading()
    {
        if (Instance == null) return;
        Instance._loadingUI?.Hide();
    }

    public static async UniTask HideLoadingWithDelay(int ms = 300)
    {
        if (Instance == null || Instance._loadingUI == null) return;
        await UniTask.Delay(ms);
        Instance._loadingUI.Hide();
    }

    private void SetProgressInternal(float value01)
    {
        if (_loadingUI == null) return;

        // 0~1 클램프 + "지금까지 값보다 작아지지 않도록" 보장
        _currentProgress = Mathf.Clamp01(Mathf.Max(_currentProgress, value01));
        _loadingUI.SetProgress(_currentProgress);
    }
    public static void SetProgressExternal(float value01)
    {
        if (Instance == null) return;
        Instance.SetProgressInternal(value01);
    }
}
