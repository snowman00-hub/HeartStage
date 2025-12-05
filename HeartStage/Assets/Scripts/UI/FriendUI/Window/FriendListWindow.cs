using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendListWindow : MonoBehaviour
{
    public static FriendListWindow Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("연결된 창")]
    [SerializeField] private FriendAddWindow friendAddWindow;
    [SerializeField] private FriendManageWindow friendManageWindow;
    [SerializeField] private MessageWindow messageWindow;

    [Header("상단 정보")]
    [SerializeField] private TMP_Text friendCountText;
    [SerializeField] private TMP_Text dailyLimitText;

    [Header("리스트")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private FriendListItemUI itemPrefab;

    [Header("버튼")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button claimAllButton;
    [SerializeField] private Button addFriendButton;
    [SerializeField] private Button manageFriendButton;

    [Header("로딩")]
    [SerializeField] private GameObject loadingPanel;

    private readonly List<FriendListItemUI> _spawned = new();
    private bool _isRefreshing = false;

    private List<string> _cachedFriendUids;
    private bool _isPrewarmed = false;

    private void Awake()
    {
        Instance = this;

        if (root != null)
            root.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (claimAllButton != null)
            claimAllButton.onClick.AddListener(() => OnClickClaimAllAsync().Forget());

        if (addFriendButton != null)
            addFriendButton.onClick.AddListener(OnClickAddFriend);

        if (manageFriendButton != null)
            manageFriendButton.onClick.AddListener(OnClickManageFriend);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    public void Open()
    {
        if (root != null)
            root.SetActive(true);

        if (_isPrewarmed && _cachedFriendUids != null)
        {
            ShowCachedData();
        }
        else
        {
            RefreshAsync().Forget();
        }
    }

    private void ShowCachedData()
    {
        ClearList();

        if (SaveLoadManager.Data is not SaveDataV1 data)
            return;

        RefreshHeader(_cachedFriendUids.Count);

        foreach (var friendUid in _cachedFriendUids)
        {
            var item = Instantiate(itemPrefab, contentRoot);
            item.Setup(friendUid, messageWindow);  // messageWindow 전달
            _spawned.Add(item);
        }

        _isPrewarmed = false;
        _cachedFriendUids = null;

        Debug.Log($"[FriendListWindow] 캐시 데이터로 표시 완료: {_spawned.Count}명");
    }
    public void Close()
    {
        if (root != null)
            root.SetActive(false);
    }

    public void Show()
    {
        if (root != null)
            root.SetActive(true);
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

    public async UniTask PrewarmAsync()
    {
        if (_isPrewarmed) return;

        try
        {
            _cachedFriendUids = await FriendService.GetMyFriendUidListAsync(syncLocal: true);
            await DreamEnergyGiftService.SyncCounterFromServerAsync();

            _isPrewarmed = true;
            Debug.Log($"[FriendListWindow] Prewarm 완료: 친구 {_cachedFriendUids.Count}명");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FriendListWindow] PrewarmAsync Error: {e}");
        }
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

            // 병렬로 캐시 갱신
            await UniTask.WhenAll(
                FriendService.RefreshAllCacheAsync(),
                DreamEnergyGiftService.RefreshPendingGiftsByFriendAsync()
            );

            var friendUids = FriendService.GetCachedFriendUids();

            RefreshHeader();
            // UpdateClaimButtonState();  ← 삭제!

            foreach (var friendUid in friendUids)
            {
                var item = Instantiate(itemPrefab, contentRoot);
                item.Setup(friendUid, messageWindow);
                _spawned.Add(item);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FriendListWindow] RefreshAsync Error: {e}");
        }
        finally
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(false);
            _isRefreshing = false;
        }
    }

    public void RefreshHeader(int? actualFriendCount = null)
    {
        if (SaveLoadManager.Data is not SaveDataV1 data)
            return;

        if (friendCountText != null)
        {
            int currentCount = actualFriendCount ?? FriendService.CachedFriendCount;
            friendCountText.text = $"친구 수: {currentCount}/{FriendService.MAX_FRIEND_COUNT}";
        }

        if (dailyLimitText != null)
        {
            int limit = data.dreamSendDailyLimit;
            int todayCount = GetTodaySendCount(data);
            dailyLimitText.text = $"일일 한도: {todayCount}/{limit}";
        }
    }

    private int GetTodaySendCount(SaveDataV1 data)
    {
        int today = GetTodayYmd();
        if (data.dreamLastSendDate != today)
            return 0;
        return data.dreamSendTodayCount;
    }

    private int GetTodayYmd()
    {
        var now = System.DateTime.Now;
        return now.Year * 10000 + now.Month * 100 + now.Day;
    }

    private async UniTaskVoid OnClickClaimAllAsync()
    {
        if (claimAllButton != null)
            claimAllButton.interactable = false;

        try
        {
            int gained = await DreamEnergyGiftService.ClaimAllGiftsAsync();

            if (gained > 0)
            {
                Debug.Log($"[FriendListWindow] 드림 에너지 +{gained} 획득");

                // 상단 UI 갱신
                if (LobbyManager.Instance != null)
                {
                    LobbyManager.Instance.MoneyUISet();
                }

                RefreshHeader();

                if (messageWindow != null)
                    messageWindow.OpenSuccess("선물 수령", $"드림 에너지 +{gained} 획득!");
            }
            else
            {
                Debug.Log("[FriendListWindow] 받을 선물이 없습니다.");

                if (messageWindow != null)
                    messageWindow.Open("알림", "받을 선물이 없습니다.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FriendListWindow] OnClickClaimAllAsync Error: {e}");

            if (messageWindow != null)
                messageWindow.OpenFail("오류", "선물 수령에 실패했습니다.");
        }
        finally
        {
            if (claimAllButton != null)
                claimAllButton.interactable = true;
        }
    }

    private void OnClickAddFriend()
    {
        if (!FriendService.CanAddMoreFriends())
        {
            Debug.Log($"[FriendListWindow] 친구가 이미 {FriendService.MAX_FRIEND_COUNT}명입니다.");

            if (messageWindow != null)
            {
                messageWindow.OpenFail(
                    "친구 수 제한",
                    $"친구는 최대 {FriendService.MAX_FRIEND_COUNT}명까지 추가할 수 있습니다."
                );
            }
            return;
        }

        Close();

        if (friendAddWindow != null)
        {
            friendAddWindow.Open();
        }
        else
        {
            Debug.LogError("[FriendListWindow] FriendAddWindow가 연결되지 않았습니다!", this);
        }
    }

    private void OnClickManageFriend()
    {
        Close();

        if (friendManageWindow != null)
        {
            friendManageWindow.Open();
        }
        else
        {
            Debug.LogError("[FriendListWindow] FriendManageWindow가 연결되지 않았습니다!", this);
        }
    }
}