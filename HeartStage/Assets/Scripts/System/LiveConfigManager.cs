using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Database;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AppConfigData
{
    public int minVersionCodeAndroid;   // 최소 지원 안드로이드 버전 코드
    public int recommendVersionCode;    // 권장 버전 코드
}

[Serializable]
public class MaintenanceData
{
    public bool active;     // 점검 모드 활성화 여부
    public string message;  // 점검 메시지
    public string startAt;  // 점검 시작일시
    public string endAt;    // 점검 종료일시
    public bool showRemainTime; // 남은 시간 표시 여부
}
[Serializable]
public class NoticeData
{
    public int id;              // 공지 번호 (1,2,3...)
    public string title;        // 제목
    public string body;         // 본문
    public string createdAt;    // 생성일시 (문자열)
    public string startAt;      // 노출 시작일시
    public string endAt;        // 노출 종료일시
    public bool isImportant;    // 중요 공지 여부

    // 🔹 리스트에 짧게 보여줄 내용 (옵션)
    public string summary;

    // 🔹 네이버 카페 등 외부 링크 (없으면 "" 또는 null)
    public string externalUrl;
}

public class LiveConfigManager : MonoBehaviour
{
    public static LiveConfigManager Instance { get; private set; }

    public AppConfigData AppConfig { get; private set; } = new AppConfigData();
    public MaintenanceData Maintenance { get; private set; } = new MaintenanceData();

    // 🔹 Firebase notices 노드에서 가져온 공지 리스트 (id 내림차순 정렬)
    public List<NoticeData> Notices { get; private set; } = new List<NoticeData>();

    /// <summary>appConfig 값 바뀌면 호출되는 이벤트</summary>
    public event Action OnAppConfigChanged;

    /// <summary>maintenance 값 바뀌면 호출되는 이벤트</summary>
    public event Action OnMaintenanceChanged;

    /// <summary>notices 값 바뀌면 호출되는 이벤트</summary>
    public event Action OnNoticesChanged;

    private DatabaseReference _rootRef;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// BootScene 에서 한 번만 호출해주면 됨.
    /// </summary>
    public async UniTask InitializeAsync()
    {
        // 이미 다른 곳에서 Firebase 초기화 하고 있다면,
        // 이 부분은 여러 번 호출돼도 큰 문제는 없음.
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus != DependencyStatus.Available)
        {
            Debug.LogError($"Firebase 의존성 문제: {dependencyStatus}");
            return;
        }

        _rootRef = FirebaseDatabase.DefaultInstance.RootReference;

        // 처음 한 번 DB에서 가져오기
        await LoadAppConfigAsync();
        await LoadMaintenanceAsync();
        await LoadNoticesAsync();         // 🔹 공지도 처음에 한 번 로딩

        // 이후 값 바뀌는 것도 실시간으로 듣기
        _rootRef.Child("appConfig").ValueChanged += (s, e) =>
        {
            ApplyAppConfig(e.Snapshot);
        };

        _rootRef.Child("maintenance").ValueChanged += (s, e) =>
        {
            ApplyMaintenance(e.Snapshot);
        };

        _rootRef.Child("notices").ValueChanged += (s, e) =>
        {
            ApplyNotices(e.Snapshot);
        };
    }

    private async UniTask LoadAppConfigAsync()
    {
        var snap = await _rootRef.Child("appConfig").GetValueAsync();
        ApplyAppConfig(snap);
    }

    private async UniTask LoadMaintenanceAsync()
    {
        var snap = await _rootRef.Child("maintenance").GetValueAsync();
        ApplyMaintenance(snap);
    }

    private async UniTask LoadNoticesAsync()
    {
        var snap = await _rootRef.Child("notices").GetValueAsync();
        ApplyNotices(snap);
    }

    private void ApplyAppConfig(DataSnapshot snap)
    {
        if (!snap.Exists)
        {
            AppConfig = new AppConfigData();
        }
        else
        {
            AppConfig = new AppConfigData
            {
                minVersionCodeAndroid = ToInt(snap.Child("minVersionCodeAndroid").Value),
                recommendVersionCode = ToInt(snap.Child("recommendVersionCode").Value)
            };
        }

        OnAppConfigChanged?.Invoke();
    }

    private void ApplyMaintenance(DataSnapshot snap)
    {
        if (!snap.Exists)
        {
            Maintenance = new MaintenanceData();
        }
        else
        {
            Maintenance = new MaintenanceData
            {
                active = snap.Child("active").Value is bool b && b,
                message = snap.Child("message").Value as string ?? "",
                startAt = snap.Child("startAt").Value as string ?? "",
                endAt = snap.Child("endAt").Value as string ?? "",
                showRemainTime = snap.Child("showRemainTime").Value is bool bb && bb
            };
        }

        OnMaintenanceChanged?.Invoke();
    }

    private void ApplyNotices(DataSnapshot snap)
    {
        var list = new List<NoticeData>();

        if (snap.Exists)
        {
            foreach (var child in snap.Children)
            {
                // key ("1","2"...)를 id로 쓰되, 필드에 id가 있으면 그걸 우선
                int idFromKey = ToInt(child.Key);

                var notice = new NoticeData
                {
                    id = child.Child("id").Value != null
                         ? ToInt(child.Child("id").Value)
                         : idFromKey,
                    title = child.Child("title").Value as string ?? "",
                    body = child.Child("body").Value as string ?? "",
                    createdAt = child.Child("createdAt").Value as string ?? "",
                    startAt = child.Child("startAt").Value as string ?? "",
                    endAt = child.Child("endAt").Value as string ?? "",
                    isImportant = child.Child("isImportant").Value is bool b && b,
                    summary = child.Child("summary").Value as string ?? "",
                    externalUrl = child.Child("externalUrl").Value as string ?? "",
                };

                list.Add(notice);
            }
        }

        // 최신 공지가 앞에 오도록 id 내림차순 정렬
        list.Sort((a, b) => b.id.CompareTo(a.id));

        Notices = list;

        OnNoticesChanged?.Invoke();
    }

    private int ToInt(object value)
    {
        if (value == null) return 0;
        if (int.TryParse(value.ToString(), out int v)) return v;
        return 0;
    }
}