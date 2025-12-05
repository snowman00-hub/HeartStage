using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendManageItemUI : MonoBehaviour
{
    public enum Mode
    {
        ReceivedRequest,
        SentRequest,
        FriendManage
    }

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI fanAmountText;
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI lastLoginText;

    [Header("아이콘")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Button iconButton;

    [Header("버튼")]
    [SerializeField] private Button actionButton;

    private string _targetUid;
    private string _nickname;
    private Mode _mode;
    private Action _onCompleted;
    private MessageWindow _messageWindow;

    public void Setup(string targetUid, Mode mode, Action onCompleted, MessageWindow messageWindow)
    {
        _targetUid = targetUid;
        _mode = mode;
        _onCompleted = onCompleted;
        _messageWindow = messageWindow;
        _nickname = "하트스테이지팬";

        if (fanAmountText != null)
            fanAmountText.text = "팬: ???";

        if (nicknameText != null)
            nicknameText.text = "로딩 중...";

        if (lastLoginText != null)
            lastLoginText.text = "로딩 중...";

        if (iconButton != null)
        {
            iconButton.onClick.RemoveAllListeners();
            iconButton.onClick.AddListener(OnClickIcon);
        }

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnClickAction);
            actionButton.interactable = true;
        }

        LoadProfileAsync().Forget();
    }

    private void OnClickIcon()
    {
        if (string.IsNullOrEmpty(_targetUid))
            return;

        if (FriendProfileWindow.Instance != null)
            FriendProfileWindow.Instance.Open(_targetUid);
    }

    private string GetDisplayNickname(string nickname, string uid)
    {
        if (string.IsNullOrEmpty(nickname) || nickname == uid)
            return "하트스테이지팬";
        return nickname;
    }

    private async UniTaskVoid LoadProfileAsync()
    {
        try
        {
            var data = await PublicProfileService.GetPublicProfileAsync(_targetUid);

            if (data == null)
            {
                if (nicknameText != null)
                    nicknameText.text = "하트스테이지팬";
                if (fanAmountText != null)
                    fanAmountText.text = "팬: 0";
                return;
            }

            _nickname = GetDisplayNickname(data.nickname, _targetUid);

            if (nicknameText != null)
                nicknameText.text = _nickname;

            if (fanAmountText != null)
                fanAmountText.text = $"팬: {data.fanAmount}";

            if (lastLoginText != null)
                lastLoginText.text = "방금 전";

            if (iconImage != null)
            {
                var sprite = ResourceManager.Instance.Get<Sprite>(data.profileIconKey);
                if (sprite != null)
                    iconImage.sprite = sprite;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManageItemUI] LoadProfileAsync Error: {e}");
        }
    }

    private void OnClickAction()
    {
        if (_messageWindow == null)
        {
            Debug.LogError("[FriendManageItemUI] MessageWindow가 연결되지 않았습니다!");
            return;
        }

        switch (_mode)
        {
            case Mode.ReceivedRequest:
                _messageWindow.OpenTwoButton(
                    "친구 신청",
                    $"{_nickname}님의 친구 신청을\n어떻게 하시겠습니까?",
                    "수락",
                    "거절",
                    onConfirm: () => AcceptRequestAsync().Forget(),
                    onCancel: () => DeclineRequestAsync().Forget()
                );
                break;

            case Mode.SentRequest:
                _messageWindow.OpenTwoButton(
                    "신청 취소",
                    $"{_nickname}님에게 보낸 친구 신청을\n취소하시겠습니까?",
                    "취소하기",
                    "아니오",
                    onConfirm: () => CancelRequestAsync().Forget()
                );
                break;

            case Mode.FriendManage:
                _messageWindow.OpenTwoButton(
                    "친구 삭제",
                    $"{_nickname}님을 친구 목록에서\n삭제하시겠습니까?",
                    "삭제",
                    "취소",
                    onConfirm: () => RemoveFriendAsync().Forget()
                );
                break;
        }
    }

    private async UniTaskVoid AcceptRequestAsync()
    {
        if (actionButton != null)
            actionButton.interactable = false;

        try
        {
            bool success = await FriendService.AcceptFriendRequestAsync(_targetUid);

            if (success)
            {
                _messageWindow?.OpenSuccess("친구 수락", $"{_nickname}님과 친구가 되었습니다!");
                _onCompleted?.Invoke();
            }
            else
            {
                _messageWindow?.OpenFail("수락 실패", "친구 수가 최대치이거나\n이미 친구 상태입니다.");
                if (actionButton != null)
                    actionButton.interactable = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManageItemUI] AcceptRequestAsync Error: {e}");
            _messageWindow?.OpenFail("오류", "친구 수락 중 오류가 발생했습니다.");
            if (actionButton != null)
                actionButton.interactable = true;
        }
    }

    private async UniTaskVoid DeclineRequestAsync()
    {
        if (actionButton != null)
            actionButton.interactable = false;

        try
        {
            bool success = await FriendService.DeclineFriendRequestAsync(_targetUid);

            if (success)
            {
                _messageWindow?.OpenSuccess("신청 거절", $"{_nickname}님의 친구 신청을\n거절했습니다.");
                _onCompleted?.Invoke();
            }
            else
            {
                _messageWindow?.OpenFail("거절 실패", "요청 처리 중 문제가 발생했습니다.");
                if (actionButton != null)
                    actionButton.interactable = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManageItemUI] DeclineRequestAsync Error: {e}");
            _messageWindow?.OpenFail("오류", "요청 거절 중 오류가 발생했습니다.");
            if (actionButton != null)
                actionButton.interactable = true;
        }
    }

    private async UniTaskVoid CancelRequestAsync()
    {
        if (actionButton != null)
            actionButton.interactable = false;

        try
        {
            bool success = await FriendService.CancelSentRequestAsync(_targetUid);

            if (success)
            {
                _messageWindow?.OpenSuccess("신청 취소", $"{_nickname}님에게 보낸\n친구 신청을 취소했습니다.");
                _onCompleted?.Invoke();
            }
            else
            {
                _messageWindow?.OpenFail("취소 실패", "요청 처리 중 문제가 발생했습니다.");
                if (actionButton != null)
                    actionButton.interactable = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManageItemUI] CancelRequestAsync Error: {e}");
            _messageWindow?.OpenFail("오류", "요청 취소 중 오류가 발생했습니다.");
            if (actionButton != null)
                actionButton.interactable = true;
        }
    }

    private async UniTaskVoid RemoveFriendAsync()
    {
        if (actionButton != null)
            actionButton.interactable = false;

        try
        {
            bool success = await FriendService.RemoveFriendAsync(_targetUid);

            if (success)
            {
                _messageWindow?.OpenSuccess("친구 삭제", $"{_nickname}님을\n친구 목록에서 삭제했습니다.");
                _onCompleted?.Invoke();
            }
            else
            {
                _messageWindow?.OpenFail("삭제 실패", "친구 삭제 중 문제가 발생했습니다.");
                if (actionButton != null)
                    actionButton.interactable = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManageItemUI] RemoveFriendAsync Error: {e}");
            _messageWindow?.OpenFail("오류", "친구 삭제 중 오류가 발생했습니다.");
            if (actionButton != null)
                actionButton.interactable = true;
        }
    }
}