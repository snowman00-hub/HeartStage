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

    // 🔹 다른 스크립트(Owned/Stage/SceneController)가 찍는 "목표 프로그레스"
    private float _targetProgress = 0f;

    // 🔹 실제로 로딩바에 그리는 값 (이게 서서히 _targetProgress를 따라감)
    private float _displayProgress = 0f;

    // 🔹 0~60% 구간에서 쓸 속도
    [SerializeField] private float fastSpeedTo60 = 3.0f;

    // 🔹 60% 이후에서 쓸 속도
    [SerializeField] private float slowSpeedAfter60 = 1.0f;

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

    private void Update()
    {
        if (_loadingUI == null)
            return;

        if (_displayProgress < _targetProgress)
        {
            // 🔹 현재 표시값이 60% 이전이면 빠르게,
            //    60% 이후면 느리게
            float speed = (_displayProgress < 0.6f)
                ? fastSpeedTo60
                : slowSpeedAfter60;

            _displayProgress = Mathf.MoveTowards(
                _displayProgress,
                _targetProgress,
                speed * Time.unscaledDeltaTime
            );

            _loadingUI.SetProgress(_displayProgress);
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
        if (_loadingUI != null)
            _loadingUI.Show();

        // 시작할 때 0으로 초기화
        _targetProgress = 0f;
        _displayProgress = 0f;
        SetProgressInternal(0f);

        // 🔹 씬 로딩은 전체의 0% ~ 60%만 사용
        const float sceneLoadStart = 0f;
        const float sceneLoadEnd = 0.6f;

        var handle = Addressables.LoadSceneAsync(address, mode, activateOnLoad: false);

        while (!handle.IsDone)
        {
            float p = handle.PercentComplete; // 0 ~ 1
            float mapped = Mathf.Lerp(sceneLoadStart, sceneLoadEnd, p);
            SetProgressInternal(mapped);
            await UniTask.Yield();
        }

        var sceneInstance = handle.Result;
        var activateOp = sceneInstance.ActivateAsync();

        // 🔹 활성화 단계는 60%까지 채웠다고 가정하고 유지
        while (!activateOp.isDone)
        {
            SetProgressInternal(sceneLoadEnd);
            await UniTask.Yield();
        }

        // 나머지 60% ~ 100%는 각 씬 내부 컨트롤러
        // (OwnedCharacterSetup / StageSetupWindow / StageSceneController)에서
        // SceneLoader.SetProgressExternal()로 채운다.
    }

    public static void HideLoading()
    {
        if (Instance == null || Instance._loadingUI == null)
            return;

        Instance._loadingUI.Hide();
    }

    public static async UniTask HideLoadingWithDelay(int ms = 300)
    {
        if (Instance == null || Instance._loadingUI == null)
            return;

        await UniTask.Delay(ms);
        Instance._loadingUI.Hide();
    }

    // 🔹 내부에서 목표 프로그레스만 갱신 (바로 UI를 건드리지 않음)
    private void SetProgressInternal(float value01)
    {
        if (_loadingUI == null)
            return;

        // 0~1 클램프 + "지금까지 목표 값보다 작아지지 않도록" 보장
        float clamped = Mathf.Clamp01(value01);
        _targetProgress = Mathf.Max(_targetProgress, clamped);
    }

    // 🔹 외부(다른 스크립트)에서 불러쓰는 함수
    public static void SetProgressExternal(float value01)
    {
        if (Instance == null)
            return;

        Instance.SetProgressInternal(value01);
    }
}
