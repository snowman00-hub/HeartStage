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
    [SerializeField] private MessageWindow messageWindow;

    [Header("상단 정보")]
    [SerializeField] private TMP_Text friendCountText;    // 친구 수: 3/20
    [SerializeField] private TMP_Text dailyLimitText;     // 일일 한도: 5/20

    [Header("리스트")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private FriendListItemUI itemPrefab;

    [Header("버튼")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button claimAllButton;
    [SerializeField] private Button addFriendButton;    // 친구 추가 화면으로 이동
    [SerializeField] private Button manageFriendButton; // 친구 관리 화면으로 이동

    [Header("로딩")]
    [SerializeField] private GameObject loadingPanel;

    private readonly List<FriendListItemUI> _spawned = new();
    private bool _isRefreshing = false;

    // 캐시된 친구 UIDs (로컬 데이터 기준)
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
        Debug.Log($"[FriendListWindow] root == null? {root == null}");
        Debug.Log($"[FriendListWindow] root name: {root?.name}");
        Debug.Log($"[FriendListWindow] this.gameObject name: {gameObject.name}");
        Debug.Log($"[FriendListWindow] root == gameObject? {root == gameObject}");

        if (root != null)
        {
            root.SetActive(true);
            Debug.Log($"[FriendListWindow] SetActive 후 activeSelf: {root.activeSelf}");
        }

        Debug.Log($"[FriendListWindow] root.SetActive(true) 완료, activeSelf: {root?.activeSelf}");

        if (_isPrewarmed && _cachedFriendUids != null)
        {
            Debug.Log($"[FriendListWindow] 캐시 사용");
            ShowCachedData();
        }
        else
        {
            Debug.Log($"[FriendListWindow] RefreshAsync 시작");
            RefreshAsync().Forget();
        }
    }
    private void ShowCachedData()
    {
        ClearList();

        if (SaveLoadManager.Data is not SaveDataV1 data)
            return;

        // 헤더 업데이트
        RefreshHeader(_cachedFriendUids.Count);

        // 친구 아이템 생성
        foreach (var friendUid in _cachedFriendUids)
        {
            var item = Instantiate(itemPrefab, contentRoot);
            item.Setup(friendUid);
            _spawned.Add(item);
        }

        // 캐시 사용 완료 - 다음 Open에서는 새로 로드
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
        if(root != null)
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
            // 친구 목록 미리 로드
            _cachedFriendUids = await FriendService.GetMyFriendUidListAsync(syncLocal: true);

            // 드림 에너지 카운터 동기화
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

            if (SaveLoadManager.Data is not SaveDataV1 data)
                return;

            // Prewarm된 데이터가 있으면 사용, 없으면 서버에서 로드
            List<string> friendUids;
            if (_isPrewarmed && _cachedFriendUids != null)
            {
                friendUids = _cachedFriendUids;
                _isPrewarmed = false; // 한 번 사용 후 다음엔 서버에서 새로 로드
            }
            else
            {
                friendUids = await FriendService.GetMyFriendUidListAsync(syncLocal: true);
            }

            // 헤더 업데이트
            RefreshHeader(friendUids.Count);

            // 친구 아이템 생성
            foreach (var friendUid in friendUids)
            {
                var item = Instantiate(itemPrefab, contentRoot);
                item.Setup(friendUid);
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

    /// <summary>
    /// 헤더 정보만 갱신 (친구 수, 일일 한도)
    /// </summary>
    /// <param name="actualFriendCount">서버에서 가져온 실제 친구 수 (null이면 로컬 데이터 사용)</param>
    public void RefreshHeader(int? actualFriendCount = null)
    {
        if (SaveLoadManager.Data is not SaveDataV1 data)
            return;

        // 친구 수 - 서버에서 가져온 실제 값 우선 사용
        if (friendCountText != null)
        {
            int currentCount = actualFriendCount ?? data.friendUidList.Count;
            friendCountText.text = $"친구 수: {currentCount}/{FriendService.MAX_FRIEND_COUNT}";
        }

        // 일일 한도
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

    /// <summary>
    /// "모두 받기" 버튼
    /// </summary>
    private async UniTaskVoid OnClickClaimAllAsync()
    {
        // 버튼 중복 클릭 방지
        claimAllButton.interactable = false;

        try
        {
            int gained = await DreamEnergyGiftService.ClaimAllGiftsAsync();

            if (gained > 0)
            {
                Debug.Log($"[FriendListWindow] 드림 에너지 +{gained} 획득");

                // 헤더 갱신
                RefreshHeader();

                // TODO: 성공 토스트/팝업
                // ShowToast($"드림 에너지 +{gained} 획득!");
            }
            else
            {
                Debug.Log("[FriendListWindow] 받을 선물이 없습니다.");

                // TODO: 안내 토스트
                // ShowToast("받을 선물이 없습니다.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FriendListWindow] OnClickClaimAllAsync Error: {e}");

            // TODO: 에러 토스트
            // ShowToast("선물 수령에 실패했습니다.");
        }
        finally
        {
            claimAllButton.interactable = true;
        }
    }

    /// <summary>
    /// 친구 추가 버튼 - 친구 검색 화면으로 이동
    /// </summary>
    private void OnClickAddFriend()
    {
        if (!FriendService.CanAddMoreFriends())
        {
            Debug.Log($"[FriendListWindow] 친구가 이미 {FriendService.MAX_FRIEND_COUNT}명입니다.");

            // ✏️ 이 부분 수정
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

        // ✏️ 이 부분 수정
        if (friendAddWindow != null)
        {
            friendAddWindow.Open();
        }
        else
        {
            Debug.LogError("[FriendListWindow] FriendAddWindow가 연결되지 않았습니다!", this);
        }
    }

    /// <summary>
    /// 친구 관리 버튼 - 친구 관리 화면으로 이동
    /// </summary>
    private void OnClickManageFriend()
    {
        // 친구 관리 창 열기
        Close();
        // TODO: 친구 관리 창 구현 후 연결
        // FriendManageWindow.Instance?.Open();
        Debug.Log("[FriendListWindow] 친구 관리 화면으로 이동");
    }

    // TODO: 토스트 메시지 시스템 연동
    // private void ShowToast(string message)
    // {
    //     ToastManager.Instance?.Show(message);
    // }
}