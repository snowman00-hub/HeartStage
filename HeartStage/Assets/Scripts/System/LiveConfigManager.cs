using System;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Database;
using UnityEngine;

[Serializable]
public class AppConfigData
{
    public int minVersionCodeAndroid;
    public int minVersionCodeIOS;
    public int recommendVersionCode;
}

[Serializable]
public class MaintenanceData
{
    public bool active;
    public string message;
    public string startAt;
    public string endAt;
    public bool showRemainTime;
}

public class LiveConfigManager : MonoBehaviour
{
    public static LiveConfigManager Instance { get; private set; }

    public AppConfigData AppConfig { get; private set; } = new AppConfigData();
    public MaintenanceData Maintenance { get; private set; } = new MaintenanceData();

    /// <summary>appConfig 값 바뀌면 호출되는 이벤트</summary>
    public event Action OnAppConfigChanged;

    /// <summary>maintenance 값 바뀌면 호출되는 이벤트</summary>
    public event Action OnMaintenanceChanged;

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

        // 이후 값 바뀌는 것도 실시간으로 듣기
        _rootRef.Child("appConfig").ValueChanged += (s, e) =>
        {
            ApplyAppConfig(e.Snapshot);
        };

        _rootRef.Child("maintenance").ValueChanged += (s, e) =>
        {
            ApplyMaintenance(e.Snapshot);
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
                minVersionCodeIOS = ToInt(snap.Child("minVersionCodeIOS").Value),
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

    private int ToInt(object value)
    {
        if (value == null) return 0;
        if (int.TryParse(value.ToString(), out int v)) return v;
        return 0;
    }
}
