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
    [SerializeField] private TextMeshProUGUI FanAmount;
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI lastLoginText;

    [Header("아이콘")]
    [SerializeField] private Image iconImage;

    [Header("버튼")]
    [SerializeField] private Button actionButton;        // X 버튼 (삭제/거절/취소)
    [SerializeField] private Button acceptButton;        // 수락 버튼 (받은 신청에서만 사용)

    [Header("버튼 이미지 (선택)")]
    [SerializeField] private Image actionButtonImage;

    private string _targetUid;
    private Mode _mode;
    private Action _onCompleted;

    public void Setup(string targetUid, Mode mode, Action onCompleted)
    {
        _targetUid = targetUid;
        _mode = mode;
        _onCompleted = onCompleted;

        // 기본값
        FanAmount.text = "팬: ???";
        nicknameText.text = "로딩 중...";
        lastLoginText.text = "최근 접속 시간\n---";

        // 버튼 설정
        SetupButtons();

        // 프로필 로드
        LoadProfileAsync().Forget();
    }

    private void SetupButtons()
    {
        // 액션 버튼 (X 버튼) - 항상 표시
        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => OnClickActionAsync().Forget());
            actionButton.gameObject.SetActive(true);
        }

        // 수락 버튼 - 받은 신청에서만 표시
        if (acceptButton != null)
        {
            acceptButton.onClick.RemoveAllListeners();

            if (_mode == Mode.ReceivedRequest)
            {
                acceptButton.onClick.AddListener(() => OnClickAcceptAsync().Forget());
                acceptButton.gameObject.SetActive(true);
            }
            else
            {
                acceptButton.gameObject.SetActive(false);
            }
        }
    }

    private async UniTaskVoid LoadProfileAsync()
    {
        try
        {
            var data = await PublicProfileService.GetPublicProfileAsync(_targetUid);
            if (data == null)
            {
                nicknameText.text = "알 수 없음";
                return;
            }

            nicknameText.text = data.nickname;
            FanAmount.text = $"팬: {data.fanAmount}";

            // 최근 접속 시간 (TODO: publicProfiles에 lastLoginUnixMillis 있으면 사용)
            lastLoginText.text = "최근 접속 시간\n방금 전";

            var sprite = ResourceManager.Instance.Get<Sprite>(data.profileIconKey);
            if (sprite != null)
                iconImage.sprite = sprite;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManageItemUI] LoadProfileAsync Error: {e}");
        }
    }

    private int CalculateLevel(int fanAmount)
    {
        return Mathf.Min(999, fanAmount / 100 + 1);
    }

    /// <summary>
    /// X 버튼 클릭 (삭제/거절/취소)
    /// </summary>
    private async UniTaskVoid OnClickActionAsync()
    {
        actionButton.interactable = false;
        if (acceptButton != null)
            acceptButton.interactable = false;

        try
        {
            bool success = false;

            switch (_mode)
            {
                case Mode.ReceivedRequest:
                    // 거절
                    success = await FriendService.DeclineFriendRequestAsync(_targetUid);
                    break;

                case Mode.SentRequest:
                    // 취소
                    success = await FriendService.CancelSentRequestAsync(_targetUid);
                    break;

                case Mode.FriendManage:
                    // 삭제
                    success = await FriendService.RemoveFriendAsync(_targetUid);
                    break;
            }

            if (success)
            {
                Debug.Log($"[FriendManageItemUI] 액션 완료: {_mode}, {_targetUid}");
                _onCompleted?.Invoke();
            }
            else
            {
                actionButton.interactable = true;
                if (acceptButton != null)
                    acceptButton.interactable = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManageItemUI] OnClickActionAsync Error: {e}");
            actionButton.interactable = true;
            if (acceptButton != null)
                acceptButton.interactable = true;
        }
    }

    /// <summary>
    /// 수락 버튼 클릭 (받은 신청에서만)
    /// </summary>
    private async UniTaskVoid OnClickAcceptAsync()
    {
        actionButton.interactable = false;
        acceptButton.interactable = false;

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
                actionButton.interactable = true;
                acceptButton.interactable = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManageItemUI] OnClickAcceptAsync Error: {e}");
            actionButton.interactable = true;
            acceptButton.interactable = true;
        }
    }
}