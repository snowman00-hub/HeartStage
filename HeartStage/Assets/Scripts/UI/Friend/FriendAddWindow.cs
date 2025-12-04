using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendAddWindow : MonoBehaviour
{
    public static FriendAddWindow Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("연결된 창")]
    [SerializeField] private FriendListWindow friendListWindow;
    [SerializeField] private FriendSearchWindow friendSearchWindow;
    [SerializeField] private MessageWindow messageWindow;

    [Header("상단 정보")]
    [SerializeField] private TMP_Text friendCountText;    // 친구 수: 20/20
    [SerializeField] private TMP_Text requestCountText;   // 친구 신청: 20/20

    [Header("리스트")]
    [SerializeField] private Transform contentRoot;       // 아이템 부모
    [SerializeField] private FriendAddItemUI itemPrefab;  // 아이템 프리팹

    [Header("버튼")]
    [SerializeField] private Button closeButton;          // 닫기
    [SerializeField] private Button refreshButton;        // 새로 고침 버튼
    [SerializeField] private Button searchButton;         // 닉네임 검색 버튼 (별도 창 오픈)

    [Header("로딩")]
    [SerializeField] private GameObject loadingPanel;

    [Header("설정")]
    [SerializeField] private int randomCandidateCount = 20; // 추천 친구 수

    private readonly List<FriendAddItemUI> _spawned = new();
    private bool _isRefreshing = false;

    private void Awake()
    {
        Instance = this;

        if (root != null)
            root.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (refreshButton != null)
            refreshButton.onClick.AddListener(() => RefreshAsync().Forget());

        // 🔍 검색 버튼은 이제 별도 검색창만 연다
        if (searchButton != null)
            searchButton.onClick.AddListener(OnClickOpenSearchWindow);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    public async void Open()
    {
        if (root != null)
            root.SetActive(true);

        await RefreshAsync();
    }

    public void Close()
    {
        if (root != null)
            root.SetActive(false);

        friendListWindow.Open();
    }

    /// <summary>
    /// 리스트 클리어
    /// </summary>
    private void ClearList()
    {
        foreach (var item in _spawned)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        _spawned.Clear();
    }

    /// <summary>
    /// 추천 친구 목록 새로 고침
    /// </summary>
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

            // 헤더 업데이트
            await UpdateHeaderAsync();

            // 추천 친구 표시
            await ShowRecommendedFriendsAsync();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FriendAddWindow] RefreshAsync Error: {e}");
        }
        finally
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(false);
            _isRefreshing = false;
        }
    }

    /// <summary>
    /// 추천 친구 목록 표시
    /// </summary>
    private async UniTask ShowRecommendedFriendsAsync()
    {
        // 랜덤 추천 친구 가져오기
        var candidates = await FriendSearchService.GetRandomCandidatesAsync(randomCandidateCount);

        // 친구 아이템 생성
        foreach (var candidate in candidates)
        {
            var item = Instantiate(itemPrefab, contentRoot);
            item.Setup(candidate);
            _spawned.Add(item);
        }

        Debug.Log($"[FriendAddWindow] 추천 친구 {candidates.Count}명 로드 완료");
    }

    /// <summary>
    /// 헤더 정보 업데이트
    /// </summary>
    private async UniTask UpdateHeaderAsync()
    {
        if (SaveLoadManager.Data is not SaveDataV1 data)
            return;

        // 친구 수 (서버에서 가져오기)
        var friendUids = await FriendService.GetMyFriendUidListAsync(syncLocal: true);
        if (friendCountText != null)
        {
            friendCountText.text = $"친구 수: {friendUids.Count}/{FriendService.MAX_FRIEND_COUNT}";
        }

        // 친구 신청 수 (받은 요청)
        var receivedRequests = await FriendService.GetReceivedRequestsAsync();
        if (requestCountText != null)
        {
            // TODO: 보낸 요청도 합쳐서 표시할지 결정
            requestCountText.text = $"친구 신청: {receivedRequests.Count}/??";
        }
    }

    /// <summary>
    /// 검색 결과를 받아서 리스트에 표시
    /// (FriendSearchWindow에서 호출)
    /// </summary>
    public void ShowSearchResults(List<PublicProfileSummary> results)
    {
        ClearList();

        if (results == null || results.Count == 0)
        {
            Debug.Log("[FriendAddWindow] ShowSearchResults: 결과가 비어 있습니다.");
            return;
        }

        foreach (var profile in results)
        {
            var item = Instantiate(itemPrefab, contentRoot);
            item.Setup(profile);
            _spawned.Add(item);
        }

        Debug.Log($"[FriendAddWindow] 검색 결과 {results.Count}명을 리스트에 표시했습니다.");
    }

    /// <summary>
    /// 닉네임 검색창 열기 (친구 수 초과 시 MessageWindow로 알림)
    /// </summary>
    private void OnClickOpenSearchWindow()
    {
        if (!FriendService.CanAddMoreFriends())
        {
            Debug.Log($"[FriendAddWindow] 친구가 이미 {FriendService.MAX_FRIEND_COUNT}명입니다.");

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

        // ✏️ 이 부분 수정
        if (friendSearchWindow != null)
        {
            friendSearchWindow.Open();
        }
        else
        {
            Debug.LogError("[FriendAddWindow] FriendSearchWindow가 연결되지 않았습니다!", this);

            if (messageWindow != null)
            {
                messageWindow.OpenFail(
                    "검색 기능 오류",
                    "친구 검색 기능이 준비되지 않았습니다."
                );
            }
        }
    }

    /// <summary>
    /// 외부에서 친구 요청 성공 시 호출 (헤더 갱신용)
    /// </summary>
    public void OnFriendRequestSent()
    {
        UpdateHeaderAsync().Forget();
    }
}
