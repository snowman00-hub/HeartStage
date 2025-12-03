using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FriendListWindow : MonoBehaviour
{
    public static FriendListWindow Instance;

    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("리스트")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private FriendListItemUI itemPrefab;

    [Header("상단 버튼")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button claimAllGiftButton;
    [SerializeField] private TMP_Text dreamEnergyText;

    private readonly List<FriendListItemUI> _spawned = new();

    private void Awake()
    {
        Instance = this;
        root.SetActive(false);

        closeButton.onClick.AddListener(Close);
        refreshButton.onClick.AddListener(() => RefreshAsync().Forget());
        claimAllGiftButton.onClick.AddListener(() => OnClickClaimAll().Forget());
    }

    public void Open()
    {
        root.SetActive(true);
        RefreshAsync().Forget();
    }

    private async UniTask RefreshAsync()
    {
        // 기존 아이템 정리
        foreach (var it in _spawned) Destroy(it.gameObject);
        _spawned.Clear();

        if (SaveLoadManager.Data is not SaveDataV1 data)
            return;

        // 드림 에너지 숫자 갱신
        dreamEnergyText.text = data.dreamEnergy.ToString("N0");

        // 🔹 서버에서 친구 목록 가져오기 (동시에 SaveDataV1.friendUidList도 동기화)
        List<string> friendUids = await FriendService.GetMyFriendUidListAsync(syncLocal: true);

        // 가져온 목록 기준으로 UI 다시 생성
        foreach (var friendUid in friendUids)
        {
            var item = Instantiate(itemPrefab, contentRoot);
            item.Setup(friendUid);
            _spawned.Add(item);
        }
    }

    private async UniTaskVoid OnClickClaimAll()
    {
        int amount = await DreamEnergyGiftService.ClaimAllGiftsAsync();
        if (amount > 0 && SaveLoadManager.Data is SaveDataV1 data)
        {
            dreamEnergyText.text = data.dreamEnergy.ToString("N0");
        }
    }

    public void Close() => root.SetActive(false);
}
