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
    [SerializeField] private TextMeshProUGUI fanAmountText;      // 팬 수

    [Header("아이콘")]
    [SerializeField] private Image iconImage;

    [Header("버튼")]
    [SerializeField] private Button sendEnergyButton;
    [SerializeField] private Button receiveEnergyButton;
    [SerializeField] private Button visitHouseButton;

    private string _friendUid;
    private CancellationTokenSource _cts;

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    /// <summary>
    /// 외부에서 friendUid만 넘겨서 셋업
    /// </summary>
    public void Setup(string friendUid)
    {
        _friendUid = friendUid;

        // 이전 토큰 취소
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        // 기본 표시값
        nicknameText.text = "로딩 중...";
        fanAmountText.text = "로딩 중...";

        // 기본 아이콘
        var defaultSprite = ResourceManager.Instance.Get<Sprite>("ProfileIcon_Default");
        if (defaultSprite != null)
            iconImage.sprite = defaultSprite;

        // 버튼 리스너 초기화
        sendEnergyButton.onClick.RemoveAllListeners();
        sendEnergyButton.onClick.AddListener(() => OnClickSendAsync().Forget());

        receiveEnergyButton.onClick.RemoveAllListeners();
        receiveEnergyButton.onClick.AddListener(() => OnClickReceiveAsync().Forget());

        visitHouseButton.onClick.RemoveAllListeners();
        visitHouseButton.interactable = false; // 현재 잠금

        // 공개 프로필 로드
        LoadPublicProfileAsync().Forget();
    }

    /// <summary>
    /// publicProfiles/{uid}에서 데이터 불러와서 UI 반영
    /// </summary>
    private async UniTaskVoid LoadPublicProfileAsync()
    {
        try
        {
            var data = await PublicProfileService.GetPublicProfileAsync(_friendUid);
            if (data == null)
            {
                nicknameText.text = "알 수 없음";
                fanAmountText.text = "팬: 0";
                return;
            }

            nicknameText.text = data.nickname;
            fanAmountText.text = $"♥ {data.fanAmount:N0}";

            var sprite = ResourceManager.Instance.Get<Sprite>(data.profileIconKey);
            if (sprite != null)
                iconImage.sprite = sprite;

            // 오늘 이미 보냈는지 체크해서 버튼 비활성화
            UpdateSendButtonState();
        }
        catch (OperationCanceledException)
        {
            // 정상적인 취소
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendListItemUI] LoadPublicProfileAsync Error: {e}");
        }
    }

    /// <summary>
    /// 오늘 이미 보냈는지 체크해서 버튼 상태 업데이트
    /// </summary>
    private void UpdateSendButtonState()
    {
        if (SaveLoadManager.Data is not SaveDataV1 data)
            return;

        int today = GetTodayYmd();
        if (data.dreamLastSendDate != today)
        {
            sendEnergyButton.interactable = true;
        }
        else
        {
            // 일일 한도 체크
            sendEnergyButton.interactable = data.dreamSendTodayCount < data.dreamSendDailyLimit;
        }
    }

    private int GetTodayYmd()
    {
        var now = DateTime.Now;
        return now.Year * 10000 + now.Month * 100 + now.Day;
    }

    /// <summary>
    /// 드림 에너지 보내기
    /// </summary>
    private async UniTaskVoid OnClickSendAsync()
    {
        // 버튼 중복 클릭 방지
        sendEnergyButton.interactable = false;

        try
        {
            bool success = await DreamEnergyGiftService.TrySendDreamEnergyAsync(_friendUid)
                .AttachExternalCancellation(_cts.Token);

            if (success)
            {
                Debug.Log($"[FriendListItemUI] 드림 에너지 전송 성공: {_friendUid}");
                // TODO: 성공 이펙트/토스트

                // 친구 목록 창 갱신 (헤더 정보 업데이트용)
                if (FriendListWindow.Instance != null)
                {
                    // 서버에서 다시 친구 수 가져와서 갱신
                    var friendUids = await FriendService.GetMyFriendUidListAsync(syncLocal: true)
                        .AttachExternalCancellation(_cts.Token);
                    FriendListWindow.Instance.RefreshHeader();
                }
            }
            else
            {
                Debug.Log($"[FriendListItemUI] 드림 에너지 전송 실패");
                // TODO: 실패 사유 토스트
                sendEnergyButton.interactable = true; // 실패 시 다시 활성화
            }
        }
        catch (OperationCanceledException)
        {
            // 정상적인 취소
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendListItemUI] OnClickSendAsync Error: {e}");
            sendEnergyButton.interactable = true;
        }
    }

    /// <summary>
    /// 드림 에너지 받기
    /// </summary>
    private async UniTaskVoid OnClickReceiveAsync()
    {
        receiveEnergyButton.interactable = false;

        try
        {
            int received = await DreamEnergyGiftService.ClaimAllGiftsAsync()
                .AttachExternalCancellation(_cts.Token);

            if (received > 0)
            {
                Debug.Log($"[FriendListItemUI] 드림 에너지 수령: +{received}");
                // TODO: 수령 이펙트/토스트

                // 친구 목록 창 갱신
                if (FriendListWindow.Instance != null)
                {
                    FriendListWindow.Instance.RefreshHeader();
                }
            }
            else
            {
                Debug.Log("[FriendListItemUI] 받을 선물이 없습니다.");
                // TODO: 안내 토스트
                receiveEnergyButton.interactable = true;
            }
        }
        catch (OperationCanceledException)
        {
            // 정상적인 취소
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendListItemUI] OnClickReceiveAsync Error: {e}");
            receiveEnergyButton.interactable = true;
        }
    }
}