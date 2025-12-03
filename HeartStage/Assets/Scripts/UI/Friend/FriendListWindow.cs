using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendListWindow : MonoBehaviour
{
    public static FriendListWindow Instance;

    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("상단 정보")]
    [SerializeField] private TMP_Text friendCountText;    // 예: "친구 수 3/20"
    [SerializeField] private TMP_Text dailyLimitText;     // 예: "일일 한도 5/20"
    [SerializeField] private TMP_Text dreamEnergyText;    // 예: "드림 에너지 1,234"

    [Header("리스트")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private FriendListItemUI itemPrefab;
    [SerializeField] private GameObject emptyMessageRoot; // "등록된 친구가 없습니다" 같은 오브젝트

    [Header("버튼")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button claimAllButton;       // 모두 받기 버튼

    // 최대 친구 수 (기획 기준 20명)
    [Header("설정")]
    [SerializeField] private int maxFriendCount = 20;

    private readonly List<FriendListItemUI> _spawned = new();

    private void Awake()
    {
        Instance = this;

        if (root != null)
            root.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (refreshButton != null)
            refreshButton.onClick.AddListener(() => RefreshAsync().Forget());

        if (claimAllButton != null)
            claimAllButton.onClick.AddListener(() => OnClickClaimAll().Forget());
    }

    public void Open()
    {
        if (root != null)
            root.SetActive(true);

        RefreshAsync().Forget();
    }

    public void Close()
    {
        if (root != null)
            root.SetActive(false);
    }

    private void ClearList()
    {
        foreach (var it in _spawned)
        {
            if (it != null)
                Destroy(it.gameObject);
        }
        _spawned.Clear();
    }

    private async UniTask RefreshAsync()
    {
        ClearList();

        if (SaveLoadManager.Data is not SaveDataV1 data)
            return;

        // 우선 상단 정보 갱신 (친구 수는 서버 목록 받은 뒤 다시 업데이트)
        UpdateHeader(data, currentFriendCount: data.friendUidList.Count);

        // 서버 기준 친구 목록 가져오기 (동시에 SaveDataV1.friendUidList도 덮어씀)
        List<string> friendUids = await FriendService.GetMyFriendUidListAsync(syncLocal: true);

        // 서버에서 가져온 수 기준으로 다시 헤더 갱신
        UpdateHeader(data, currentFriendCount: friendUids.Count);

        // 친구가 하나도 없으면 빈 메시지 On
        if (friendUids.Count == 0)
        {
            if (emptyMessageRoot != null)
                emptyMessageRoot.SetActive(true);
            return;
        }

        if (emptyMessageRoot != null)
            emptyMessageRoot.SetActive(false);

        // 슬롯 생성
        foreach (var friendUid in friendUids)
        {
            var item = Instantiate(itemPrefab, contentRoot);
            item.Setup(friendUid);
            _spawned.Add(item);
        }
    }

    private void UpdateHeader(SaveDataV1 data, int currentFriendCount)
    {
        // 친구 수
        if (friendCountText != null)
        {
            friendCountText.text = $"친구 수 {currentFriendCount}/{maxFriendCount}";
        }

        // 일일 한도: 오늘 보낸 횟수 / 최대
        int limit = data.dreamSendDailyLimit;
        int todayCount = GetTodaySendCount(data);

        if (dailyLimitText != null)
        {
            dailyLimitText.text = $"일일 한도 {todayCount}/{limit}";
        }

        // 내 현재 드림 에너지
        if (dreamEnergyText != null)
        {
            dreamEnergyText.text = data.dreamEnergy.ToString("N0");
        }
    }

    private int GetTodaySendCount(SaveDataV1 data)
    {
        int today = GetTodayYmd();
        if (data.dreamLastSendDate != today)
        {
            // 날짜가 바뀌었으면 아직 안 보낸 것으로
            return 0;
        }
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
    private async UniTaskVoid OnClickClaimAll()
    {
        int gained = await DreamEnergyGiftService.ClaimAllGiftsAsync();
        if (gained > 0 && SaveLoadManager.Data is SaveDataV1 data)
        {
            // 드림 에너지 숫자 갱신
            UpdateHeader(data, currentFriendCount: data.friendUidList.Count);
            // TODO: 토스트/팝업 "드림 에너지 xN 획득" 같은 연출 원하면 여기서 처리
        }
    }
}
