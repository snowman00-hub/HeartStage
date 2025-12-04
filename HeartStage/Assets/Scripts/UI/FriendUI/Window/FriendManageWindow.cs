using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendManageWindow : MonoBehaviour
{
    public static FriendManageWindow Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("연결된 창")]
    [SerializeField] private FriendListWindow friendListWindow;
    [SerializeField] private MessageWindow messageWindow;

    [Header("탭 버튼")]
    [SerializeField] private Button receivedTabButton;
    [SerializeField] private Button sentTabButton;
    [SerializeField] private Button manageTabButton;

    [Header("탭 이미지 (선택 표시용)")]
    [SerializeField] private Image receivedTabImage;
    [SerializeField] private Image sentTabImage;
    [SerializeField] private Image manageTabImage;
    [SerializeField] private Color selectedTabColor = Color.white;
    [SerializeField] private Color unselectedTabColor = Color.gray;

    [Header("상단 정보")]
    [SerializeField] private TMP_Text requestCountText;
    [SerializeField] private TMP_Text friendCountText;

    [Header("리스트")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private FriendManageItemUI itemPrefab;

    [Header("버튼")]
    [SerializeField] private Button closeButton;

    [Header("로딩")]
    [SerializeField] private GameObject loadingPanel;

    private readonly List<FriendManageItemUI> _spawned = new();
    private bool _isRefreshing = false;

    private enum TabType { Received, Sent, Manage }
    private TabType _currentTab = TabType.Received;

    private void Awake()
    {
        Instance = this;

        if (root != null)
            root.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (receivedTabButton != null)
            receivedTabButton.onClick.AddListener(() => SwitchTab(TabType.Received));

        if (sentTabButton != null)
            sentTabButton.onClick.AddListener(() => SwitchTab(TabType.Sent));

        if (manageTabButton != null)
            manageTabButton.onClick.AddListener(() => SwitchTab(TabType.Manage));

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    public void Open()
    {
        if (root != null)
            root.SetActive(true);

        _currentTab = TabType.Received;
        UpdateTabVisual();
        RefreshAsync().Forget();
    }

    public void Close()
    {
        if (root != null)
            root.SetActive(false);

        // 닫을 때 FriendListWindow도 갱신하여 친구 수 동기화
        if (friendListWindow != null)
        {
            friendListWindow.RefreshAsync().Forget();
        }
        friendListWindow?.Show();
    }

    public void Show()
    {
        if (root != null)
            root.SetActive(true);
    }

    private void SwitchTab(TabType tab)
    {
        if (_currentTab == tab)
            return;

        _currentTab = tab;
        UpdateTabVisual();
        RefreshAsync().Forget();
    }

    private void UpdateTabVisual()
    {
        if (receivedTabImage != null)
            receivedTabImage.color = _currentTab == TabType.Received ? selectedTabColor : unselectedTabColor;

        if (sentTabImage != null)
            sentTabImage.color = _currentTab == TabType.Sent ? selectedTabColor : unselectedTabColor;

        if (manageTabImage != null)
            manageTabImage.color = _currentTab == TabType.Manage ? selectedTabColor : unselectedTabColor;
    }

    private void ClearList()
    {
        foreach (var item in _spawned)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        _spawned.Clear();
    }

    public async UniTask RefreshAsync()
    {
        if (_isRefreshing) return;
        _isRefreshing = true;

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        try
        {
            ClearList();
            await UpdateHeaderAsync();

            switch (_currentTab)
            {
                case TabType.Received:
                    await ShowReceivedRequestsAsync();
                    break;
                case TabType.Sent:
                    await ShowSentRequestsAsync();
                    break;
                case TabType.Manage:
                    await ShowFriendListAsync();
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FriendManageWindow] RefreshAsync Error: {e}");
        }
        finally
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(false);
            _isRefreshing = false;
        }
    }

    private async UniTask UpdateHeaderAsync()
    {
        // 병렬로 로드
        var (friendUids, requestCounts) = await UniTask.WhenAll(
            FriendService.GetMyFriendUidListAsync(syncLocal: true),
            FriendService.GetRequestCountsAsync()
        );

        if (friendCountText != null)
            friendCountText.text = $"친구 수: {friendUids.Count}/{FriendService.MAX_FRIEND_COUNT}";

        int totalRequests = requestCounts.received + requestCounts.sent;
        if (requestCountText != null)
            requestCountText.text = $"친구 신청: {totalRequests}/{FriendService.MAX_REQUEST_COUNT}";
    }

    private async UniTask ShowReceivedRequestsAsync()
    {
        var requests = await FriendService.GetReceivedRequestsAsync();

        foreach (var fromUid in requests)
        {
            var item = Instantiate(itemPrefab, contentRoot);
            item.Setup(fromUid, FriendManageItemUI.Mode.ReceivedRequest, OnItemActionCompleted, messageWindow);
            _spawned.Add(item);
        }

        Debug.Log($"[FriendManageWindow] 받은 신청 {requests.Count}개 표시");
    }

    private async UniTask ShowSentRequestsAsync()
    {
        var requests = await FriendService.GetSentRequestsAsync();

        foreach (var toUid in requests)
        {
            var item = Instantiate(itemPrefab, contentRoot);
            item.Setup(toUid, FriendManageItemUI.Mode.SentRequest, OnItemActionCompleted, messageWindow);
            _spawned.Add(item);
        }

        Debug.Log($"[FriendManageWindow] 보낸 신청 {requests.Count}개 표시");
    }

    private async UniTask ShowFriendListAsync()
    {
        var friendUids = await FriendService.GetMyFriendUidListAsync(syncLocal: false);

        foreach (var friendUid in friendUids)
        {
            var item = Instantiate(itemPrefab, contentRoot);
            item.Setup(friendUid, FriendManageItemUI.Mode.FriendManage, OnItemActionCompleted, messageWindow);
            _spawned.Add(item);
        }

        Debug.Log($"[FriendManageWindow] 친구 목록 {friendUids.Count}명 표시");
    }

    private void OnItemActionCompleted()
    {
        RefreshAsync().Forget();
    }
}