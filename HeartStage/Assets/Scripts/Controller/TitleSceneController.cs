using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class TitleSceneController : MonoBehaviour
{
    [Header("페이드 / 로고 / 배경")]
    [SerializeField] private CanvasGroup fadeCanvas;          // 검은 패널(Drak) CanvasGroup + Image
    [SerializeField] private GameObject logoRoot;             // Logo 오브젝트
    [SerializeField] private CanvasGroup logoCanvasGroup;     // Logo에 달린 CanvasGroup
    [SerializeField] private GameObject titleBackgroundRoot;  // 타이틀 배경(BackGroundRoot)

    [Header("하단 상태 텍스트 / Touch to Start")]
    [SerializeField] private TextMeshProUGUI statusText;      // "로딩중...", "로그인이 필요합니다", "Touch to Start"
    [SerializeField] private GameObject touchToStartPanel;    // ★ "Touch to Start" 텍스트만 있는 패널 (BackGroundRoot 말고 별도 오브젝트!)

    [Header("로그인 UI 루트 (LoginUI 루트 오브젝트)")]
    [SerializeField] private GameObject loginUIRoot;          // LoginUI 전체 루트

    [Header("씬 이동 설정")]
    [SerializeField] private SceneType lobbySceneType = SceneType.LobbyScene;

    [Header("인트로 연출 타이밍 (초)")]
    [SerializeField] private float firstBlackDelay = 0.3f;  // 검은 화면만 보이는 시간
    [SerializeField] private float logoFadeInTime = 0.4f;   // 로고가 서서히 나타나는 시간
    [SerializeField] private float logoHoldTime = 0.6f;    // 로고가 완전히 보인 채로 유지되는 시간
    [SerializeField] private float fadeOutTime = 0.5f;    // 검은 화면 + 로고 같이 사라지는 시간

    [Header("로딩 텍스트 ... 속도 (초)")]
    [SerializeField] private float dotInterval = 0.4f;        // . 하나씩 늘어나는 간격

    private bool _readyToStart = false;

    // 상태 텍스트 애니메이션용
    private CancellationTokenSource _statusCts;
    private string _currentBaseStatus = string.Empty;

    private void Awake()
    {
        if (titleBackgroundRoot != null)
            titleBackgroundRoot.SetActive(true);

        // 🔹 로고는 처음부터 켜두고, alpha = 0 으로 숨겨놓기
        if (logoRoot != null)
            logoRoot.SetActive(true);
        if (logoCanvasGroup != null)
            logoCanvasGroup.alpha = 0f;

        if (fadeCanvas != null)
            fadeCanvas.alpha = 1f; // 완전 검은 화면

        if (statusText != null)
            statusText.text = string.Empty;

        if (touchToStartPanel != null)
            touchToStartPanel.SetActive(false);

        if (loginUIRoot != null)
            loginUIRoot.SetActive(false);
    }

    private async void Start()
    {
        // 1) 인트로 연출: 검은 화면 → 로고 같이 보임 → 둘이 동시에 페이드아웃
        await IntroSequenceAsync();

        // 2) 인트로가 완전히 끝난 뒤에야 로그인 UI를 켠다.
        if (loginUIRoot != null)
            loginUIRoot.SetActive(true);

        // 3) 이제부터 로딩/로그인/세이브/출석 처리 시작
        await PostLoginFlowAsync();

        // 4) 모든 준비 완료 → "Touch to Start" 표시 + 터치 대기
        ShowTouchToStart();
    }

    private void Update()
    {
        if (!_readyToStart)
            return;

        if (IsAnyScreenTouchDown())
        {
            _readyToStart = false;
            GoToLobby().Forget();
        }
    }

    #region 1. 인트로 연출 (검은 화면 + 로고 같이 있다가 동시에 페이드아웃)

    private async UniTask IntroSequenceAsync()
    {
        // 1) 검은 화면만 잠깐 유지
        if (firstBlackDelay > 0f)
            await UniTask.Delay(TimeSpan.FromSeconds(firstBlackDelay), DelayType.UnscaledDeltaTime);

        // 2) 검은 화면 + 로고 붙어서 나오는 느낌으로
        //    → 검은 화면은 그대로 두고, 로고만 alpha 0 -> 1 로 서서히 페이드인
        float t = 0f;
        float fadeInDuration = Mathf.Max(0.01f, logoFadeInTime);

        if (logoCanvasGroup != null)
            logoCanvasGroup.alpha = 0f;
        if (fadeCanvas != null)
            fadeCanvas.alpha = 1f; // 계속 검은 상태

        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            float n = Mathf.Clamp01(t / fadeInDuration); // 0 -> 1

            if (logoCanvasGroup != null)
                logoCanvasGroup.alpha = n; // 로고만 서서히 보이게

            await UniTask.Yield();
        }

        if (logoCanvasGroup != null)
            logoCanvasGroup.alpha = 1f; // 완전히 보이게 고정

        // 3) 로고가 완전히 보인 채로 잠깐 유지
        if (logoHoldTime > 0f)
            await UniTask.Delay(TimeSpan.FromSeconds(logoHoldTime), DelayType.UnscaledDeltaTime);

        // 4) 검은 화면 + 로고를 동시에 페이드아웃
        t = 0f;
        float fadeOutDuration = Mathf.Max(0.01f, fadeOutTime);

        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            float n = Mathf.Clamp01(t / fadeOutDuration); // 0 -> 1
            float inv = 1f - n;                             // 1 -> 0

            if (fadeCanvas != null)
                fadeCanvas.alpha = inv;
            if (logoCanvasGroup != null)
                logoCanvasGroup.alpha = inv;

            await UniTask.Yield();
        }

        if (fadeCanvas != null)
            fadeCanvas.alpha = 0f;
        if (logoCanvasGroup != null)
            logoCanvasGroup.alpha = 0f;
    }

    #endregion

    #region 2. 로그인 / 세이브 / 출석 처리

    private async UniTask PostLoginFlowAsync()
    {
        // 1) AuthManager 준비될 때까지 "로딩중..." ... 애니메이션
        SetStatus("로딩중", animateDots: true);

        await UniTask.WaitUntil(() =>
            AuthManager.Instance != null &&
            AuthManager.Instance.IsInitialized);

        // 2) 초기화는 됐는데 아직 로그인 안 되어 있으면 → 로그인 필요
        if (!AuthManager.Instance.IsLoggedIn)
        {
            SetStatus("로그인이 필요합니다", animateDots: false);

            await UniTask.WaitUntil(() =>
                AuthManager.Instance.IsLoggedIn);
        }

        // 3) 여기부터는 로그인 완료 상태
        SetStatus("유저 데이터 불러오는 중", animateDots: true);
        await LoadOrCreateSaveAsync();

        SetStatus("출석 정보 확인 중", animateDots: true);
        await UpdateLastLoginTimeAsync();
    }

    private static async UniTask LoadOrCreateSaveAsync()
    {
        bool loaded = await SaveLoadManager.LoadFromServer();

        if (!loaded)
        {
            var charTable = DataTableManager.CharacterTable;

            charTable.BuildDefaultSaveDictionaries(
                new[] { "하나" },
                out var unlockedByName,
                out var expById,
                out var ownedBaseIds
            );

            SaveLoadManager.Data.unlockedByName = unlockedByName;
            SaveLoadManager.Data.expById = expById;

            foreach (var id in ownedBaseIds)
                SaveLoadManager.Data.ownedIds.Add(id);

            ItemInvenHelper.AddItem(ItemID.DreamEnergy, 100);
            QuestManager.Instance.OnAttendance();

            await SaveLoadManager.SaveToServer();
        }
    }

    private static async UniTask UpdateLastLoginTimeAsync()
    {
        var now = FirebaseTime.GetServerTime();
        var last = SaveLoadManager.Data.LastLoginTime;

        if (last.Date != now.Date)
        {
            QuestManager.Instance.OnAttendance();
        }

        SaveLoadManager.Data.lastLoginBinary = now.ToBinary();
        await SaveLoadManager.SaveToServer();
    }

    #endregion

    #region 3. Touch to Start & 씬 이동

    private void ShowTouchToStart()
    {
        SetStatus("Touch to Start", animateDots: false);

        if (touchToStartPanel != null)
            touchToStartPanel.SetActive(true);

        _readyToStart = true;
    }

    private async UniTaskVoid GoToLobby()
    {
        // TODO: 터치 SFX 있으면 여기서 재생
        // SFXManager.Play("ui_touch");

        await GameSceneManager.ChangeScene(lobbySceneType);
    }

    private bool IsAnyScreenTouchDown()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        if (Input.GetMouseButtonDown(0))
            return true;
#endif
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                    return true;
            }
        }
        return false;
    }

    #endregion

    #region 4. 상태 텍스트 / ... 애니메이션

    private void SetStatus(string baseText, bool animateDots)
    {
        _currentBaseStatus = baseText ?? string.Empty;

        _statusCts?.Cancel();
        _statusCts?.Dispose();
        _statusCts = null;

        if (statusText == null)
            return;

        if (!animateDots)
        {
            statusText.text = _currentBaseStatus;
            return;
        }

        _statusCts = new CancellationTokenSource();
        StatusDotsLoop(_currentBaseStatus, _statusCts.Token).Forget();
    }

    private async UniTaskVoid StatusDotsLoop(string baseText, CancellationToken token)
    {
        int dotCount = 0;

        while (!token.IsCancellationRequested)
        {
            string dots = new string('.', dotCount);
            statusText.text = baseText + dots;

            dotCount = (dotCount + 1) % 4;

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(dotInterval),
                                    DelayType.UnscaledDeltaTime,
                                    cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private void OnDestroy()
    {
        _statusCts?.Cancel();
        _statusCts?.Dispose();
        _statusCts = null;
    }

    #endregion
}
