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
    [SerializeField] private TextMeshProUGUI lastLoginText;

    [Header("아이콘")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Button iconButton;

    [Header("버튼")]
    [SerializeField] private Button requestButton;

    [Header("버튼 텍스트")]
    [SerializeField] private TextMeshProUGUI requestButtonText;

    private PublicProfileSummary _profileData;
    private CancellationTokenSource _cts;
    private MessageWindow _messageWindow;

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    public void Setup(PublicProfileSummary profileData, MessageWindow messageWindow = null)
    {
        _profileData = profileData;
        _messageWindow = messageWindow;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        if (nicknameText != null)
            nicknameText.text = GetDisplayNickname(profileData.nickname, profileData.uid);

        if (fanAmountText != null)
            fanAmountText.text = $"팬: {CalculateLevel(profileData.fanAmount)}";

        if (lastLoginText != null)
            lastLoginText.text = "최근 접속 시간\n방금 전";

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

        if (iconButton != null)
        {
            iconButton.onClick.RemoveAllListeners();
            iconButton.onClick.AddListener(OnClickIcon);
        }

        if (requestButton != null)
        {
            requestButton.onClick.RemoveAllListeners();
            requestButton.onClick.AddListener(() => OnClickRequestAsync().Forget());
            requestButton.interactable = true;
        }

        if (requestButtonText != null)
            requestButtonText.text = "친구\n신청";
    }

    private void OnClickIcon()
    {
        if (_profileData == null || string.IsNullOrEmpty(_profileData.uid))
            return;

        if (FriendProfileWindow.Instance != null)
            FriendProfileWindow.Instance.Open(_profileData.uid);
    }

    private string GetDisplayNickname(string nickname, string uid)
    {
        if (string.IsNullOrEmpty(nickname) || nickname == uid)
            return "하트스테이지팬";
        return nickname;
    }

    private int CalculateLevel(int fanAmount)
    {
        return Mathf.Min(999, fanAmount / 100 + 1);
    }

    private async UniTaskVoid OnClickRequestAsync()
    {
        if (_profileData == null)
            return;

        requestButton.interactable = false;
        string displayName = GetDisplayNickname(_profileData.nickname, _profileData.uid);

        try
        {
            bool success = await FriendService.SendFriendRequestAsync(_profileData.uid)
                .AttachExternalCancellation(_cts.Token);

            if (success)
            {
                if (requestButtonText != null)
                    requestButtonText.text = "신청\n완료";

                if (FriendAddWindow.Instance != null)
                    FriendAddWindow.Instance.OnFriendRequestSent();

                // 성공 메시지
                _messageWindow?.OpenSuccess("친구 신청", $"{displayName}님에게\n친구 신청을 보냈습니다!");
            }
            else
            {
                requestButton.interactable = true;

                // 실패 메시지 (이미 친구이거나 이미 요청을 보낸 경우)
                _messageWindow?.OpenFail("친구 신청 실패", $"{displayName}님에게 이미 친구 신청을 보냈거나\n이미 친구 상태입니다.");
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

            _messageWindow?.OpenFail("오류", "친구 신청 중 오류가 발생했습니다.");
        }
    }

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