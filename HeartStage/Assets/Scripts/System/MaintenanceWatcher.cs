using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class MaintenanceWatcher : MonoBehaviour
{
    private static MaintenanceWatcher _instance;

    [Header("런타임 점검 팝업 프리팹 (Canvas 포함)")]
    [SerializeField] private GameObject popupPrefab;   // RuntimeMaintenancePopup 프리팹

    [SerializeField] private string titleSceneName = "TitleScene";

    private GameObject _popupInstance;
    private bool _handlingMaintenance = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        WaitAndSubscribe().Forget();
    }

    private void OnDisable()
    {
        if (LiveConfigManager.Instance != null)
        {
            LiveConfigManager.Instance.OnMaintenanceChanged -= HandleMaintenanceChanged;
        }
    }

    /// <summary>
    /// LiveConfigManager.Instance 생성될 때까지 기다렸다가 이벤트 구독.
    /// </summary>
    private async UniTaskVoid WaitAndSubscribe()
    {
        if (!isActiveAndEnabled)
            return;

        await UniTask.WaitUntil(() => LiveConfigManager.Instance != null);

        if (!isActiveAndEnabled)
            return;

        // 혹시 중복 구독 방지
        LiveConfigManager.Instance.OnMaintenanceChanged -= HandleMaintenanceChanged;
        LiveConfigManager.Instance.OnMaintenanceChanged += HandleMaintenanceChanged;

        Debug.Log("[MaintenanceWatcher] Subscribed to OnMaintenanceChanged");
    }

    private void HandleMaintenanceChanged()
    {
        var m = LiveConfigManager.Instance.Maintenance;
        if (m == null)
            return;

        bool isNow = IsMaintenanceNow(m);
        Debug.Log($"[MaintenanceWatcher] HandleMaintenanceChanged: active={m.active}, isNow={isNow}");

        // 🔹 지금은 점검 시간이 아님 → 팝업 있으면 닫고 타임스케일 복구
        if (!isNow)
        {
            if (_popupInstance != null)
            {
                Destroy(_popupInstance);
                _popupInstance = null;
            }

            _handlingMaintenance = false;
            Time.timeScale = 1f;
            return;
        }

        // 🔹 여기부터는 "지금은 점검 상태"인 경우

        // 이미 점검 처리 중이면 또 만들 필요 없음
        if (_handlingMaintenance)
            return;

        // TitleScene / BootScene 에서는 TitleSceneController가 처리하니까 패스
        var scene = SceneManager.GetActiveScene();
        if (scene.name == titleSceneName || scene.name == "BootScene")
            return;

        _handlingMaintenance = true;
        ShowRuntimeMaintenancePopup(m);
    }


    private void ShowRuntimeMaintenancePopup(MaintenanceData m)
    {
        if (popupPrefab == null)
        {
            Debug.LogError("[MaintenanceWatcher] popupPrefab 이 없습니다. 앱을 종료합니다.");
            QuitApp();
            return;
        }

        if (_popupInstance != null)
        {
            Destroy(_popupInstance);
            _popupInstance = null;
        }

        // Canvas까지 포함된 프리팹을 그냥 Instantiate하면 됨
        _popupInstance = Instantiate(popupPrefab);

        // 점검 팝업 뜨는 순간 게임 정지
        Time.timeScale = 0f;

        var runtimePopup = _popupInstance.GetComponent<RuntimeMaintenancePopup>();
        if (runtimePopup != null)
        {
            string msg = string.IsNullOrEmpty(m.message)
                ? "점검이 시작되어 게임 이용이 제한됩니다.\n앱이 종료됩니다."
                : m.message;

            if (m.showRemainTime && !string.IsNullOrEmpty(m.endAt))
            {
                if (System.DateTimeOffset.TryParse(m.endAt, out var end))
                {
                    var now = System.DateTimeOffset.Now;
                    if (end > now)
                    {
                        var remain = end - now;
                        int min = (int)System.Math.Max(0, remain.TotalMinutes);
                        msg += $"\n(점검 종료까지 약 {min}분 남았습니다.)";
                    }
                }
            }

            runtimePopup.SetMessage(msg);
            runtimePopup.Init(() =>
            {
                OnClickRuntimeMaintenanceOk().Forget();
            });
        }
        else
        {
            Debug.LogWarning("[MaintenanceWatcher] RuntimeMaintenancePopup 컴포넌트를 찾지 못했습니다. 앱을 종료합니다.");
            Time.timeScale = 1f;
            QuitApp();
        }
    }

    private async UniTaskVoid OnClickRuntimeMaintenanceOk()
    {
        // 필요하면 마지막 저장:
        await SaveLoadManager.SaveToServer();

        // 종료 직전에 타임스케일 복구
        Time.timeScale = 1f;
        QuitApp();
    }

    private bool IsMaintenanceNow(MaintenanceData m)
    {
        if (m == null)
            return false;

        // 1) 운영자가 강제로 active = true 넣으면 무조건 점검
        if (m.active)
            return true;

        // 2) 시간 기반 점검 (startAt ~ endAt)
        if (!string.IsNullOrEmpty(m.startAt))
        {
            if (DateTimeOffset.TryParse(m.startAt, out var start))
            {
                // now는 네가 쓰는 시간 소스에 맞게 (서버시간 쓰고 싶으면 그걸로 교체)
                var now = DateTimeOffset.Now;
                // var now = FirebaseTime.GetServerTimeOffset(); 이런 식으로 바꿔도 됨

                if (string.IsNullOrEmpty(m.endAt))
                {
                    // endAt 없으면 "start 이후 계속 점검"으로 처리
                    return now >= start;
                }
                else if (DateTimeOffset.TryParse(m.endAt, out var end))
                {
                    // start <= now <= end 구간이면 점검
                    return now >= start && now <= end;
                }
            }
        }

        return false;
    }


    private void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
