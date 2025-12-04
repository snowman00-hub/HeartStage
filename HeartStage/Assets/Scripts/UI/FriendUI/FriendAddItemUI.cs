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
    [SerializeField] private Button iconButton;  // 추가: 아이콘 버튼

    [Header("버튼")]
    [SerializeField] private Button requestButton;

    [Header("버튼 텍스트")]
    [SerializeField] private TextMeshProUGUI requestButtonText;

    private PublicProfileSummary _profileData;
    private CancellationTokenSource _cts;

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    public void Setup(PublicProfileSummary profileData)
    {
        _profileData = profileData;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        // 닉네임 (uid면 "하트스테이지팬"으로 표시)
        if (nicknameText != null)
            nicknameText.text = GetDisplayNickname(profileData.nickname, profileData.uid);

        if (fanAmountText != null)
            fanAmountText.text = $"팬: {CalculateLevel(profileData.fanAmount)}";

        if (lastLoginText != null)
            lastLoginText.text = "최근 접속 시간\n방금 전";

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

        // 아이콘 버튼 클릭 → 프로필 창 열기
        if (iconButton != null)
        {
            iconButton.onClick.RemoveAllListeners();
            iconButton.onClick.AddListener(OnClickIcon);
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
    /// 아이콘 클릭 시 프로필 창 열기
    /// </summary>
    private void OnClickIcon()
    {
        if (_profileData == null || string.IsNullOrEmpty(_profileData.uid))
            return;

        if (FriendProfileWindow.Instance != null)
        {
            FriendProfileWindow.Instance.Open(_profileData.uid);
        }
        else
        {
            Debug.LogWarning("[FriendAddItemUI] FriendProfileWindow. Instance가 null입니다.");
        }
    }

    /// <summary>
    /// 닉네임이 uid와 같으면 "하트스테이지팬" 반환
    /// </summary>
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

        try
        {
            bool success = await FriendService.SendFriendRequestAsync(_profileData.uid)
                .AttachExternalCancellation(_cts.Token);

            if (success)
            {
                Debug.Log($"[FriendAddItemUI] 친구 요청 전송 성공: {_profileData.nickname}");

                if (requestButtonText != null)
                    requestButtonText.text = "신청\n완료";

                if (FriendAddWindow.Instance != null)
                    FriendAddWindow.Instance.OnFriendRequestSent();
            }
            else
            {
                Debug.Log($"[FriendAddItemUI] 친구 요청 전송 실패: {_profileData.nickname}");
                requestButton.interactable = true;
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