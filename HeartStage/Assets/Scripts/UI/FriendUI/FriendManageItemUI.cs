using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendManageItemUI : MonoBehaviour
{
    public enum Mode
    {
        ReceivedRequest,  // 받은 신청 (수락/거절)
        SentRequest,      // 보낸 신청 (취소)
        FriendManage      // 친구 관리 (삭제)
    }

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI fanAmountText;
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI lastLoginText;

    [Header("아이콘")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Button iconButton;  // 추가: 아이콘 버튼

    [Header("버튼")]
    [SerializeField] private Button actionButton;  // X 버튼 1개

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

        // 기본값
        if (fanAmountText != null)
            fanAmountText.text = "팬: ??? ";

        if (nicknameText != null)
            nicknameText.text = "로딩 중...";

        if (lastLoginText != null)
            lastLoginText.text = "로딩 중... ";

        // 아이콘 버튼 클릭 → 프로필 창 열기
        if (iconButton != null)
        {
            iconButton.onClick.RemoveAllListeners();
            iconButton.onClick.AddListener(OnClickIcon);
        }

        // 버튼 설정
        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnClickAction);
            actionButton.interactable = true;
        }

        // 프로필 로드
        LoadProfileAsync().Forget();
    }

    /// <summary>
    /// 아이콘 클릭 시 프로필 창 열기
    /// </summary>
    private void OnClickIcon()
    {
        if (string.IsNullOrEmpty(_targetUid))
            return;

        if (FriendProfileWindow.Instance != null)
        {
            FriendProfileWindow.Instance.Open(_targetUid);
        }
        else
        {
            Debug.LogWarning("[FriendManageItemUI] FriendProfileWindow. Instance가 null입니다.");
        }
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

    /// <summary>
    /// X 버튼 클릭 → MessageWindow 팝업
    /// </summary>
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
                    $"{_nickname}님의 친구 신청을\n어떻게 하시겠습니까? ",
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
                    $"{_nickname}님을 친구 목록에서\n삭제하시겠습니까? ",
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
                Debug.Log($"[FriendManageItemUI] 친구 요청 수락: {_targetUid}");
                _onCompleted?.Invoke();
            }
            else
            {
                if (actionButton != null)
                    actionButton.interactable = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManageItemUI] AcceptRequestAsync Error: {e}");
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
                Debug.Log($"[FriendManageItemUI] 친구 요청 거절: {_targetUid}");
                _onCompleted?.Invoke();
            }
            else
            {
                if (actionButton != null)
                    actionButton.interactable = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManageItemUI] DeclineRequestAsync Error: {e}");
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
                Debug.Log($"[FriendManageItemUI] 보낸 요청 취소: {_targetUid}");
                _onCompleted?.Invoke();
            }
            else
            {
                if (actionButton != null)
                    actionButton.interactable = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManageItemUI] CancelRequestAsync Error: {e}");
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
                Debug.Log($"[FriendManageItemUI] 친구 삭제: {_targetUid}");
                _onCompleted?.Invoke();
            }
            else
            {
                if (actionButton != null)
                    actionButton.interactable = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManageItemUI] RemoveFriendAsync Error: {e}");
            if (actionButton != null)
                actionButton.interactable = true;
        }
    }
}