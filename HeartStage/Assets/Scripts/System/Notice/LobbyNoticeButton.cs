using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LobbyNoticeButton : MonoBehaviour
{
    [Header("공지 버튼")]
    [SerializeField] private Button noticeButton;

    [Header("NEW 뱃지 오브젝트")]
    [SerializeField] private GameObject newBadge;

    [Header("공지창 UI")]
    [SerializeField] private NoticeWindowUI noticeWindow;

    private void Awake()
    {
        if (noticeButton != null)
        {
            noticeButton.onClick.RemoveAllListeners();
            noticeButton.onClick.AddListener(OnClickNotice);
        }

        if (newBadge != null)
            newBadge.SetActive(false);
    }

    private void OnEnable()
    {
        // 공지 / 세이브 상태에 따라 뉴 뱃지 갱신
        RefreshNewBadgeAsync().Forget();

        if (LiveConfigManager.Instance != null)
        {
            LiveConfigManager.Instance.OnNoticesChanged -= HandleNoticesChanged;
            LiveConfigManager.Instance.OnNoticesChanged += HandleNoticesChanged;
        }
    }

    private void OnDisable()
    {
        if (LiveConfigManager.Instance != null)
        {
            LiveConfigManager.Instance.OnNoticesChanged -= HandleNoticesChanged;
        }
    }

    private void HandleNoticesChanged()
    {
        RefreshNewBadgeAsync().Forget();
    }

    private async UniTaskVoid RefreshNewBadgeAsync()
    {
        // LiveConfig / SaveLoadManager 준비될 때까지 대기
        await UniTask.WaitUntil(() =>
            LiveConfigManager.Instance != null &&
            SaveLoadManager.Data != null);

        var mgr = LiveConfigManager.Instance;
        var notices = mgr.Notices;

        if (newBadge == null)
            return;

        if (notices == null || notices.Count == 0)
        {
            newBadge.SetActive(false);
            return;
        }

        int maxId = notices[0].id; // id 내림차순이므로 0번이 최신
        int lastSeen = SaveLoadManager.Data.lastSeenNoticeId;

        bool hasNew = maxId > lastSeen;
        newBadge.SetActive(hasNew);
    }

    private void OnClickNotice()
    {
        if (noticeWindow != null)
        {
            noticeWindow.Show();
        }

        // NEW 뱃지는 공지창 닫을 때 저장 후 꺼질 예정이지만,
        // UX상 "눌렀으면 바로 꺼지게" 하고 싶으면 아래 한 줄 활성화
        // if (newBadge != null) newBadge.SetActive(false);
    }
}
