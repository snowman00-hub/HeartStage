using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class NoticeWindowUI : MonoBehaviour
{
    [Header("루트 패널 (전체 공지창)")]
    [SerializeField] private GameObject root;

    [Header("리스트 영역")]
    [SerializeField] private RectTransform listContent;   // ScrollView Content
    [SerializeField] private NoticeItemUI itemPrefab;

    [Header("버튼")]
    [SerializeField] private Button closeButton;

    private readonly List<NoticeItemUI> _items = new();

    private void Awake()
    {
        if (root != null)
            root.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnClickClose);
        }
    }

    public void Show()
    {
        if (root != null)
            root.SetActive(true);

        RebuildList();
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }

    private void RebuildList()
    {
        var mgr = LiveConfigManager.Instance;
        if (mgr == null)
        {
            Debug.LogWarning("[NoticeWindowUI] LiveConfigManager.Instance 없음");
            return;
        }

        var notices = mgr.Notices;

        // 기존 카드 삭제
        foreach (var item in _items)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        _items.Clear();

        if (notices == null || notices.Count == 0)
            return;

        foreach (var n in notices)
        {
            var item = Instantiate(itemPrefab, listContent);
            // layoutRoot에 listContent 넣어주면, 카드가 펼쳐질 때 전체 컨텐츠 높이 재계산됨
            item.Init(n);
            _items.Add(item);
        }

        // 기본적으로 첫 번째 공지를 펼쳐두고 싶으면:
        if (_items.Count > 0)
        {
            // 강제로 한 번 펼치기
            // _items[0].ForceExpandIf원하면 여기에 추가 메서드 만들어 사용 가능
        }
    }

    private void OnClickClose()
    {
        Hide();
        UpdateLastSeenNoticeAsync().Forget();
    }

    private async UniTaskVoid UpdateLastSeenNoticeAsync()
    {
        var mgr = LiveConfigManager.Instance;
        if (mgr == null || mgr.Notices == null || mgr.Notices.Count == 0)
            return;

        if (SaveLoadManager.Data == null)
            return;

        int maxId = mgr.Notices[0].id;
        if (maxId <= SaveLoadManager.Data.lastSeenNoticeId)
            return;

        SaveLoadManager.Data.lastSeenNoticeId = maxId;
        await SaveLoadManager.SaveToServer();
    }
}
