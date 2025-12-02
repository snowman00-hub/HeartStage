using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

[DisallowMultipleComponent]
public class TitleSceneController : MonoBehaviour
{
    [Header("페이드 / 로고 / 배경")]
    [SerializeField] private CanvasGroup fadeCanvas;          // 검은 패널 CanvasGroup + Image
    [SerializeField] private GameObject logoRoot;             // Logo 오브젝트
    [SerializeField] private CanvasGroup logoCanvasGroup;     // Logo에 달린 CanvasGroup
    [SerializeField] private GameObject titleBackgroundRoot;  // 타이틀 배경 Root

    [Header("하단 상태 텍스트 / Touch to Start")]
    [SerializeField] private TextMeshProUGUI statusText;      // "로딩중...", "로그인이 필요합니다", "Touch to Start"
    [SerializeField] private GameObject touchToStartPanel;    // "Touch to Start" 텍스트만 있는 패널

    [Header("로그인 UI 루트 (LoginUI 루트 오브젝트)")]
    [SerializeField] private GameObject loginUIRoot;          // LoginUI 전체 루트

    [Header("씬 이동 설정")]
    [SerializeField] private SceneType lobbySceneType = SceneType.LobbyScene;

    [Header("인트로 연출 타이밍 (초)")]
    [SerializeField] private float firstBlackDelay = 0.3f;  // 검은 화면만 보이는 시간
    [SerializeField] private float logoFadeInTime = 0.4f;   // 로고가 서서히 나타나는 시간
    [SerializeField] private float logoHoldTime = 0.6f;     // 로고가 완전히 보인 채로 유지되는 시간
    [SerializeField] private float fadeOutTime = 0.5f;      // 검은 화면 + 로고 같이 사라지는 시간

    [Header("로딩 텍스트 ... 속도 (초)")]
    [SerializeField] private float dotInterval = 0.4f;      // . 하나씩 늘어나는 간격

    [Header("점검 / 강제 업데이트 팝업")]
    [SerializeField] private GameObject maintenancePopupRoot;
    [SerializeField] private TextMeshProUGUI maintenanceMessageText;
    [SerializeField] private GameObject forceUpdatePopupRoot;
    [SerializeField] private TextMeshProUGUI forceUpdateMessageText;

    [Header("스토어 URL (Android)")]
    [SerializeField] private string androidStoreUrl; // 예: "https://play.google.com/store/apps/details?id=com.Company.HeartStage"

    private bool _readyToStart = false;

    // 상태 텍스트 애니메이션용
    private CancellationTokenSource _statusCts;
    private string _currentBaseStatus = string.Empty;

    #region Unity 생명주기

    private void Awake()
    {
        if (titleBackgroundRoot != null)
            titleBackgroundRoot.SetActive(true);

        // 로고는 처음부터 켜두고 alpha = 0 으로 숨기기
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

        if (maintenancePopupRoot != null)
            maintenancePopupRoot.SetActive(false);
        if (forceUpdatePopupRoot != null)
            forceUpdatePopupRoot.SetActive(false);
    }

    private async void Start()
    {
        // 1) 인트로 연출: 검은 화면 → 로고 → 둘이 같이 페이드아웃
        await IntroSequenceAsync();

        // 2) 인트로 끝난 시점에서 버전 / 점검 상태 체크
        //    막혀야 하면 여기서 리턴해서 뒤 로직 안 타게 함.
        if (!CheckForceUpdateAndMaintenance())
        {
            return;
        }

        // 3) 로그인 UI 켜기
        if (loginUIRoot != null)
            loginUIRoot.SetActive(true);

        // 4) 로딩/로그인/세이브/출석 처리
        await PostLoginFlowAsync();

        // 5) 모든 준비 완료 → "Touch to Start" 표시 + 터치 대기
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

    private void OnDestroy()
    {
        _statusCts?.Cancel();
        _statusCts?.Dispose();
        _statusCts = null;
    }

    #endregion

    #region 1. 인트로 연출 (검은 화면 + 로고 같이 있다가 동시에 페이드아웃)

    private async UniTask IntroSequenceAsync()
    {
        // 1) 검은 화면만 잠깐 유지
        if (firstBlackDelay > 0f)
            await UniTask.Delay(TimeSpan.FromSeconds(firstBlackDelay), DelayType.UnscaledDeltaTime);

        // 2) 검은 화면 + 로고 붙어서 나오는 느낌
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
            float inv = 1f - n;                           // 1 -> 0

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
            // 최초 접속 시 기본 세이브 생성
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

            // 기본 자원 / 출석 처리
            ItemInvenHelper.AddItem(ItemID.DreamEnergy, 100);
            QuestManager.Instance.OnAttendance();

            await SaveLoadManager.SaveToServer();
        }
    }

    private static async UniTask UpdateLastLoginTimeAsync()
    {
        var now = FirebaseTime.GetServerTime();
        var last = SaveLoadManager.Data.LastLoginTime;

        // 날짜가 바뀌었으면 출석 처리
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
        // SFXManager.Instance.Play("ui_touch");

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

    #endregion

    #region 5. 점검 / 강제 업데이트 체크

    /// <summary>
    /// 인트로 직후 호출. 강제 업데이트 / 점검 중이면 팝업 띄우고 진행 막음.
    /// true = 계속 진행, false = 여기서 멈춤.
    /// </summary>
    private bool CheckForceUpdateAndMaintenance()
    {
        // LiveConfigManager 초기화 실패했으면 그냥 진행 (최악의 경우)
        if (LiveConfigManager.Instance == null)
            return true;

        // 1) 강제 업데이트 우선
        if (IsForceUpdateNeeded(out string updateMsg))
        {
            if (forceUpdatePopupRoot != null && forceUpdateMessageText != null)
            {
                forceUpdateMessageText.text = updateMsg;
                forceUpdatePopupRoot.SetActive(true);
            }
            else if (statusText != null)
            {
                statusText.text = updateMsg;
            }

            // 막아야 하므로 false
            return false;
        }

        // 2) 점검 모드 체크
        if (IsInMaintenance(out string maintenanceMsg))
        {
            if (maintenancePopupRoot != null && maintenanceMessageText != null)
            {
                maintenanceMessageText.text = maintenanceMsg;
                maintenancePopupRoot.SetActive(true);
            }
            else if (statusText != null)
            {
                statusText.text = maintenanceMsg;
            }

            return false;
        }

        // 둘 다 아니면 계속 진행
        return true;
    }

    private bool IsForceUpdateNeeded(out string message)
    {
        var config = LiveConfigManager.Instance.AppConfig;

        int minVersion = config.minVersionCodeAndroid;

        if (minVersion <= 0 || ClientVersion.VersionCode >= minVersion)
        {
            message = null;
            return false;
        }

        message =
            $"현재 버전({ClientVersion.VersionCode})은 더 이상 지원되지 않습니다.\n" +
            $"스토어에서 최신 버전으로 업데이트 후 이용해 주세요.";
        return true;
    }

    private bool IsInMaintenance(out string message)
    {
        message = null;

        if (LiveConfigManager.Instance == null)
            return false;

        var m = LiveConfigManager.Instance.Maintenance;
        if (m == null)
            return false;

        var now = FirebaseTime.GetServerTime();

        // 정말 점검 중인지 먼저 판단
        if (!MaintenanceUtil.IsMaintenanceNow(m, now))
        {
            // 점검 아님
            return false;
        }

        // 여기까지 왔으면 "점검 중"
        message = string.IsNullOrEmpty(m.message)
            ? "현재 서버 점검 중입니다. 잠시 후 다시 접속해 주세요."
            : m.message;

        // 남은 시간 표시 (선택)
        if (m.showRemainTime && !string.IsNullOrEmpty(m.endAt))
        {
            if (DateTimeOffset.TryParse(m.endAt, out var end) && end > now)
            {
                var remain = end - now;
                int min = (int)Math.Max(0, remain.TotalMinutes);
                message += $"\n(점검 종료까지 약 {min}분 남았습니다.)";
            }
        }

        return true;
    }


    // 강제 업데이트 팝업 버튼용
    public void OnClickForceUpdate_OpenStore()
    {
        if (!string.IsNullOrEmpty(androidStoreUrl))
        {
            Application.OpenURL(androidStoreUrl);
        }
    }

    public void OnClickForceUpdate_Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 점검 팝업 버튼용 (확인 누르면 종료)
    public void OnClickMaintenance_Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion
}
