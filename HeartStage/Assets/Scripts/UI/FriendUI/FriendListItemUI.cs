using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendListItemUI : MonoBehaviour
{
    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI fanAmountText;

    [Header("아이콘")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Button iconButton;

    [Header("버튼")]
    [SerializeField] private Button sendEnergyButton;
    [SerializeField] private Button receiveEnergyButton;
    [SerializeField] private Button visitHouseButton;

    private string _friendUid;
    private CancellationTokenSource _cts;

    private void Awake()
    {
        if (iconImage == null)
            Debug.LogError($"[FriendListItemUI] iconImage가 연결되지 않았습니다!", this);
        if (iconButton == null)
            Debug.LogWarning($"[FriendListItemUI] iconButton이 연결되지 않았습니다!", this);
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    public void Setup(string friendUid)
    {
        if (string.IsNullOrEmpty(friendUid))
        {
            Debug.LogWarning("[FriendListItemUI] Setup: friendUid가 비어있습니다.");
            return;
        }

        _friendUid = friendUid;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        // 기본 표시값
        if (nicknameText != null)
            nicknameText.text = "로딩 중...";

        if (fanAmountText != null)
            fanAmountText.text = "로딩 중... ";

        // 기본 아이콘 (GetSprite 사용!)
        if (iconImage != null)
        {
            var defaultSprite = ResourceManager.Instance?.GetSprite("ProfileIcon_Default");
            if (defaultSprite != null)
                iconImage.sprite = defaultSprite;
        }

        // 아이콘 버튼 클릭
        if (iconButton != null)
        {
            iconButton.onClick.RemoveAllListeners();
            iconButton.onClick.AddListener(OnClickIcon);
        }

        // 버튼 리스너 초기화
        if (sendEnergyButton != null)
        {
            sendEnergyButton.onClick.RemoveAllListeners();
            sendEnergyButton.onClick.AddListener(() => OnClickSendAsync().Forget());
            sendEnergyButton.interactable = false;
        }

        if (receiveEnergyButton != null)
        {
            receiveEnergyButton.onClick.RemoveAllListeners();
            receiveEnergyButton.onClick.AddListener(() => OnClickReceiveAsync().Forget());
            receiveEnergyButton.interactable = false;
        }

        if (visitHouseButton != null)
        {
            visitHouseButton.onClick.RemoveAllListeners();
            visitHouseButton.interactable = false;
        }

        LoadPublicProfileAsync().Forget();
    }

    private void OnClickIcon()
    {
        if (string.IsNullOrEmpty(_friendUid))
            return;

        if (FriendProfileWindow.Instance != null)
        {
            FriendProfileWindow.Instance.Open(_friendUid);
        }
        else
        {
            Debug.LogError("[FriendListItemUI] FriendProfileWindow. Instance가 null입니다!");
        }
    }

    private string GetDisplayNickname(string nickname, string uid)
    {
        if (string.IsNullOrEmpty(nickname) || nickname == uid)
            return "하트스테이지팬";
        return nickname;
    }

    private async UniTaskVoid LoadPublicProfileAsync()
    {
        try
        {
            // 캐시 먼저 확인!  
            var data = LobbySceneController.GetCachedFriendProfile(_friendUid);

            // 캐시에 없으면 서버에서 로드
            if (data == null)
            {
                data = await PublicProfileService.GetPublicProfileAsync(_friendUid);

                // 로드 성공하면 캐시에 저장
                if (data != null)
                {
                    LobbySceneController.UpdateCachedProfile(_friendUid, data);
                }
            }

            if (data == null)
            {
                if (nicknameText != null)
                    nicknameText.text = "하트스테이지팬";
                if (fanAmountText != null)
                    fanAmountText.text = "팬: 0";
                return;
            }

            if (nicknameText != null)
                nicknameText.text = GetDisplayNickname(data.nickname, _friendUid);

            if (fanAmountText != null)
                fanAmountText.text = $"♥ {data.fanAmount:N0}";

            // GetSprite 사용
            if (iconImage != null)
            {
                var sprite = ResourceManager.Instance?.GetSprite(data.profileIconKey);
                if (sprite != null)
                {
                    iconImage.sprite = sprite;
                }
                else
                {
                    var defaultSprite = ResourceManager.Instance?.GetSprite("hanaicon");
                    if (defaultSprite != null)
                        iconImage.sprite = defaultSprite;
                }
            }

            UpdateButtonStates();
        }
        catch (OperationCanceledException)
        {
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FriendListItemUI] LoadPublicProfileAsync Error: {e}");
        }
    }

    private void UpdateButtonStates()
    {
        if (sendEnergyButton != null)
        {
            bool canSend = CanSendGift();
            sendEnergyButton.interactable = canSend;
        }

        if (receiveEnergyButton != null)
        {
            int pendingCount = DreamEnergyGiftService.GetPendingGiftCountCached();
            receiveEnergyButton.interactable = pendingCount > 0;
        }
    }

    private bool CanSendGift()
    {
        if (SaveLoadManager.Data is not SaveDataV1 data)
            return false;

        int today = GetTodayYmd();
        if (data.dreamLastSendDate == today && data.dreamSendTodayCount >= data.dreamSendDailyLimit)
            return false;

        if (DreamEnergyGiftService.HasSentTodayCached(_friendUid))
            return false;

        return true;
    }

    private int GetTodayYmd()
    {
        var now = DateTime.Now;
        return now.Year * 10000 + now.Month * 100 + now.Day;
    }

    private async UniTaskVoid OnClickSendAsync()
    {
        if (sendEnergyButton != null)
            sendEnergyButton.interactable = false;

        try
        {
            bool success = await DreamEnergyGiftService.TrySendDreamEnergyAsync(_friendUid)
                .AttachExternalCancellation(_cts.Token);

            if (success)
            {
                Debug.Log($"[FriendListItemUI] 드림 에너지 전송 성공: {_friendUid}");

                if (FriendListWindow.Instance != null)
                    FriendListWindow.Instance.RefreshHeader();
            }
            else
            {
                Debug.Log($"[FriendListItemUI] 드림 에너지 전송 실패");
                UpdateButtonStates();
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendListItemUI] OnClickSendAsync Error: {e}");
            UpdateButtonStates();
        }
    }

    private async UniTaskVoid OnClickReceiveAsync()
    {
        if (receiveEnergyButton != null)
            receiveEnergyButton.interactable = false;

        try
        {
            int received = await DreamEnergyGiftService.ClaimAllGiftsAsync()
                .AttachExternalCancellation(_cts.Token);

            if (received > 0)
            {
                Debug.Log($"[FriendListItemUI] 드림 에너지 수령: +{received}");

                if (LobbyManager.Instance != null)
                    LobbyManager.Instance.MoneyUISet();

                if (FriendListWindow.Instance != null)
                    FriendListWindow.Instance.RefreshHeader();
            }
            else
            {
                Debug.Log("[FriendListItemUI] 받을 선물이 없습니다.");
            }

            UpdateButtonStates();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendListItemUI] OnClickReceiveAsync Error: {e}");
            UpdateButtonStates();
        }
    }
}