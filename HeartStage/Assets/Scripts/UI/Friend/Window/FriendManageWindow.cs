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
    [SerializeField] private Button receivedTabButton;   // 받은 신청
    [SerializeField] private Button sentTabButton;       // 보낸 신청
    [SerializeField] private Button manageTabButton;     // 친구 관리

    [Header("상단 정보")]
    [SerializeField] private TMP_Text requestCountText;  // 친구 신청: 20/20
    [SerializeField] private TMP_Text friendCountText;   // 친구 수: 20/20

    [Header("리스트")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private FriendManageItemUI itemPrefab;  // 공용 아이템 프리팹

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
        RefreshAsync().Forget();
    }

    public void Close()
    {
        if (root != null)
            root.SetActive(false);

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
        RefreshAsync().Forget();
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
        // 친구 수
        var friendUids = await FriendService.GetMyFriendUidListAsync(syncLocal: true);
        if (friendCountText != null)
            friendCountText.text = $"친구 수: {friendUids.Count}/{FriendService.MAX_FRIEND_COUNT}";

        // 받은 요청 수 (TODO: 보낸 요청도 합산하려면 수정)
        var received = await FriendService.GetReceivedRequestsAsync();
        if (requestCountText != null)
            requestCountText.text = $"친구 신청: {received.Count}/??";
    }

    /// <summary>
    /// 받은 신청 탭
    /// </summary>
    private async UniTask ShowReceivedRequestsAsync()
    {
        var requests = await FriendService.GetReceivedRequestsAsync();

        foreach (var fromUid in requests)
        {
            var item = Instantiate(itemPrefab, contentRoot);
            item.Setup(fromUid, FriendManageItemUI.Mode.ReceivedRequest, OnItemActionCompleted);
            _spawned.Add(item);
        }

        Debug.Log($"[FriendManageWindow] 받은 신청 {requests.Count}개 표시");
    }

    /// <summary>
    /// 보낸 신청 탭
    /// </summary>
    private async UniTask ShowSentRequestsAsync()
    {
        var requests = await FriendService.GetSentRequestsAsync();

        foreach (var toUid in requests)
        {
            var item = Instantiate(itemPrefab, contentRoot);
            item.Setup(toUid, FriendManageItemUI.Mode.SentRequest, OnItemActionCompleted);
            _spawned.Add(item);
        }

        Debug.Log($"[FriendManageWindow] 보낸 신청 {requests.Count}개 표시");
    }

    /// <summary>
    /// 친구 관리 탭
    /// </summary>
    private async UniTask ShowFriendListAsync()
    {
        var friendUids = await FriendService.GetMyFriendUidListAsync(syncLocal: false);

        foreach (var friendUid in friendUids)
        {
            var item = Instantiate(itemPrefab, contentRoot);
            item.Setup(friendUid, FriendManageItemUI.Mode.FriendManage, OnItemActionCompleted);
            _spawned.Add(item);
        }

        Debug.Log($"[FriendManageWindow] 친구 목록 {friendUids.Count}명 표시");
    }

    private void OnItemActionCompleted()
    {
        RefreshAsync().Forget();
    }
}