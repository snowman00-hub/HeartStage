using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendAddItemUI : MonoBehaviour
{
    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI fanAmountText;         
    [SerializeField] private TextMeshProUGUI lastLoginText;      // 최근 접속 시간

    [Header("아이콘")]
    [SerializeField] private Image iconImage;

    [Header("버튼")]
    [SerializeField] private Button requestButton;        // 친구 신청 버튼

    [Header("버튼 텍스트")]
    [SerializeField] private TextMeshProUGUI requestButtonText;  // "친구 신청" 텍스트

    private PublicProfileSummary _profileData;
    private CancellationTokenSource _cts;

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    /// <summary>
    /// PublicProfileSummary 데이터로 셋업
    /// </summary>
    public void Setup(PublicProfileSummary profileData)
    {
        _profileData = profileData;

        // 이전 토큰 취소
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        // 기본 표시값
        if (nicknameText != null)
            nicknameText.text = profileData.nickname;

        if (fanAmountText != null)
            fanAmountText.text = $"팬: {CalculateLevel(profileData.fanAmount)}";

        if (lastLoginText != null)
            lastLoginText.text = "최근 접속 시간\n방금 전"; // TODO: 실제 시간 계산

        // 아이콘 설정
        if (iconImage != null)
        {
            var sprite = ResourceManager.Instance.Get<Sprite>(profileData.profileIconKey);
            if (sprite != null)
                iconImage.sprite = sprite;
            else
            {
                var defaultSprite = ResourceManager.Instance.Get<Sprite>("ProfileIcon_Default");
                if (defaultSprite != null)
                    iconImage.sprite = defaultSprite;
            }
        }

        // 버튼 설정
        if (requestButton != null)
        {
            requestButton.onClick.RemoveAllListeners();
            requestButton.onClick.AddListener(() => OnClickRequestAsync().Forget());
            requestButton.interactable = true;
        }

        if (requestButtonText != null)
            requestButtonText.text = "친구\n신청";
    }

    /// <summary>
    /// 팬 수로 레벨 계산 (임시 로직)
    /// </summary>
    private int CalculateLevel(int fanAmount)
    {
        // 간단한 레벨 계산 예시
        // 실제 게임 로직에 맞게 수정 필요
        return Mathf.Min(999, fanAmount / 100 + 1);
    }

    /// <summary>
    /// 친구 신청 버튼 클릭
    /// </summary>
    private async UniTaskVoid OnClickRequestAsync()
    {
        if (_profileData == null)
            return;

        // 버튼 중복 클릭 방지
        requestButton.interactable = false;

        try
        {
            bool success = await FriendService.SendFriendRequestAsync(_profileData.uid)
                .AttachExternalCancellation(_cts.Token);

            if (success)
            {
                Debug.Log($"[FriendAddItemUI] 친구 요청 전송 성공: {_profileData.nickname}");

                // 버튼 텍스트 변경
                if (requestButtonText != null)
                    requestButtonText.text = "신청\n완료";

                // TODO: 성공 이펙트/토스트
                // ShowToast("친구 요청을 보냈습니다");

                // 친구 추가 창 헤더 갱신
                if (FriendAddWindow.Instance != null)
                    FriendAddWindow.Instance.OnFriendRequestSent();
            }
            else
            {
                Debug.Log($"[FriendAddItemUI] 친구 요청 전송 실패: {_profileData.nickname}");
                // TODO: 실패 토스트
                // ShowToast("친구 요청을 보낼 수 없습니다");

                requestButton.interactable = true; // 실패 시 다시 활성화
            }
        }
        catch (OperationCanceledException)
        {
            // 정상적인 취소
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendAddItemUI] OnClickRequestAsync Error: {e}");
            requestButton.interactable = true;
        }
    }

    /// <summary>
    /// 마지막 접속 시간 표시 (옵션)
    /// </summary>
    public void SetLastLoginTime(long unixMillis)
    {
        if (lastLoginText == null)
            return;

        var lastLogin = DateTimeOffset.FromUnixTimeMilliseconds(unixMillis).LocalDateTime;
        var now = DateTime.Now;
        var diff = now - lastLogin;

        string timeText;
        if (diff.TotalMinutes < 1)
            timeText = "방금 전";
        else if (diff.TotalHours < 1)
            timeText = $"{(int)diff.TotalMinutes}분 전";
        else if (diff.TotalDays < 1)
            timeText = $"{(int)diff.TotalHours}시간 전";
        else if (diff.TotalDays < 7)
            timeText = $"{(int)diff.TotalDays}일 전";
        else
            timeText = lastLogin.ToString("yyyy-MM-dd");

        lastLoginText.text = $"최근 접속 시간\n{timeText}";
    }
}