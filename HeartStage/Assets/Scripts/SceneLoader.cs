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
        _loadingUI?.SetProgress(0f);

        var handle = Addressables.LoadSceneAsync(address, mode, activateOnLoad: false);

        while (!handle.IsDone)
        {
            _loadingUI?.SetProgress(handle.PercentComplete);
            await UniTask.Yield();
        }

        SceneInstance sceneInstance = handle.Result;
        var activateOp = sceneInstance.ActivateAsync();

        while (!activateOp.isDone)
        {
            _loadingUI?.SetProgress(Mathf.Lerp(handle.PercentComplete, 1f, activateOp.progress));
            await UniTask.Yield();
        }

        _loadingUI?.SetProgress(1f);
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
}
