using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendListItemUI : MonoBehaviour
{
    [Header("텍스트")]
    [SerializeField] private TMP_Text nicknameText;   // 닉네임
    [SerializeField] private TMP_Text levelText;      // Lv (fanAmount 기반)

    [Header("아이콘")]
    [SerializeField] private Image iconImage;         // 프로필 아이콘

    [Header("버튼")]
    [SerializeField] private Button sendEnergyButton;     // 드림 에너지 주기
    [SerializeField] private Button receiveEnergyButton;  // 드림 에너지 받기 (로직은 추후 정교화)
    [SerializeField] private Button visitHouseButton;     // 친구 숙소 방문 (현재 잠금)

    private string _friendUid;

    /// <summary>
    /// 외부에서 friendUid만 넘겨서 셋업
    /// </summary>
    public void Setup(string friendUid)
    {
        _friendUid = friendUid;

        // 기본 표시값
        nicknameText.text = "로딩 중...";
        levelText.text = "로딩 중...";

        // 기본 아이콘
        var defaultSprite = ResourceManager.Instance.Get<Sprite>("ProfileIcon_Default");
        if (defaultSprite != null)
            iconImage.sprite = defaultSprite;

        // 버튼 리스너 초기화
        sendEnergyButton.onClick.RemoveAllListeners();
        sendEnergyButton.onClick.AddListener(() => OnClickSend().Forget());

        receiveEnergyButton.onClick.RemoveAllListeners();
        receiveEnergyButton.onClick.AddListener(() => OnClickReceive().Forget());

        visitHouseButton.onClick.RemoveAllListeners();
        // 현재 기획상 '잠긴' 버튼이므로 비활성화만 해둠
        visitHouseButton.interactable = false;
        // 나중에 열면 여기서 VisitFriend 같은 함수 연결하면 됨
        // visitHouseButton.onClick.AddListener(OnClickVisitHouse);

        // 공개 프로필 로드
        LoadPublicProfileAsync().Forget();
    }

    /// <summary>
    /// publicProfiles/{uid}에서 데이터 불러와서 UI 반영
    /// </summary>
    private async UniTaskVoid LoadPublicProfileAsync()
    {
        var data = await PublicProfileService.GetPublicProfileAsync(_friendUid);
        if (data == null)
            return;

        nicknameText.text = data.nickname;

        // Lv은 fanAmount 기반으로 표시
        levelText.text = $"팬: {data.fanAmount.ToString("N0")}";

        var sprite = ResourceManager.Instance.Get<Sprite>(data.profileIconKey);
        if (sprite != null)
            iconImage.sprite = sprite;

        // TODO: 나중에 per-friend 드림 상태(오늘 보냈는지/받을 수 있는지)에 따라
        // sendEnergyButton / receiveEnergyButton interactable 제어 가능
    }

    /// <summary>
    /// 드림 에너지 보내기 (친구 1명 대상)
    /// </summary>
    private async UniTaskVoid OnClickSend()
    {
        bool ok = await DreamEnergyGiftService.TrySendDreamEnergyAsync(_friendUid);
        if (ok)
        {
            // 일단 간단하게: 성공 시 버튼 비활성화
            sendEnergyButton.interactable = false;
        }
        else
        {
            // TODO: 실패 시 토스트/팝업 등 처리
        }
    }

    /// <summary>
    /// 드림 에너지 받기 버튼
    /// 현재 구조상 전체 수령(ClaimAll)과 겹칠 수 있어서 임시 구현 상태
    /// </summary>
    private async UniTaskVoid OnClickReceive()
    {
        // TODO: 기획에 맞춰서 "이 친구에게서 받은 것만" 처리하고 싶으면
        // DreamEnergyGiftService 쪽을 per-friend로 확장해야 함.

        int received = await DreamEnergyGiftService.ClaimAllGiftsAsync();
        if (received > 0)
        {
            receiveEnergyButton.interactable = false;
        }
    }

    // 나중에 숙소 방문 기능 열릴 때 쓸 자리
    private void OnClickVisitHouse()
    {
        // 예시:
        // FriendHousingService.VisitFriend(_friendUid);
    }
}
